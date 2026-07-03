using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;


public class SaveManager : Manager, ISaveManager, IReadyStatus
{
    private const int SlotCount = 3;
    private static string FileOf(int slot) => $"slot_{slot}.json";

    private SaveMeta[] _metas = new SaveMeta[SlotCount]; // null = 빈 슬롯
    private int _currentSlot = -1;
    public int CurrentSlot => _currentSlot;
    private Dictionary<string, bool> _readyStatus = new(); 
    public Dictionary<string, bool> ReadyStatus => _readyStatus;

    private void OnEnable() => Register();
    private void OnDisable() => Unregister();
    
    protected override void Register() => ServiceLocater.Register<ISaveManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<ISaveManager>(this);

    protected override void Init() => LoadAllSlots();
    
    public void LoadAllSlots()
    {
        _readyStatus["LoadAllSlots"] = false;
        for (int i = 0; i < SlotCount; i++)
        {
            var root = SaveSerializer.Deserialize<SaveRoot>(SaveFileIO.Read(FileOf(i)));
            Debug.Log($"[SaveManager] saved slot [{i}] is {root}");
            _metas[i] = root?.meta;     // 파일 없으면 Deserialize=null → meta=null (빈 슬롯)
        }
        _readyStatus["LoadAllSlots"] = true;
    }

    public bool IsEmpty(int slot) => _metas[slot] == null;
    public SaveMeta GetMeta(int slot) => _metas[slot];

    // 자동 저장: 현재 슬롯에 기록
    public void Save()
    {
        if (_currentSlot < 0) return;
        var root = CaptureCurrentGame();
        _metas[_currentSlot] = root.meta;    // 카드 meta도 갱신 → 재진입 시 최신
        SaveFileIO.Write(FileOf(_currentSlot), SaveSerializer.Serialize(root));
    }

    private SaveRoot LoadSlot(int slot) => SaveSerializer.Deserialize<SaveRoot>(SaveFileIO.Read(FileOf(slot)));
    
    // 슬롯 선택해서 게임 적용
    public async UniTask Load(int slot)
    {
        var root = LoadSlot(slot);
        if (root == null) return;
        _currentSlot = slot;
        await RestoreGame(root);
    }

    // 새 게임: 빈 슬롯 잡고 현재 슬롯 지정
    public void SelectSlot(int slot) => _currentSlot = slot;
    
    private SaveRoot CaptureCurrentGame()
    {
        var gm = ServiceLocater.Get<IGameManager>();   
        var game = gm.CaptureSaveData();
        var staff = ServiceLocater.Get<IStaffRegister>().CaptureSaveData();

        return new SaveRoot
        {
            version = 1,
            meta = new SaveMeta
            {
                playerName = game.playerName,
                floor = gm.Floor,
                completedProjectCount = game.projects.Count,
                procNameId = (int)game.procName + 9900023,
                year = gm.GetCalendarYear(),
                money = game.money,
                savedAt = DateTime.Now.ToString("yyyy년\nMM월dd일\nHH:mm"),  // 실제 저장 시각
            },
            game = game,
            events = ServiceLocater.Get<IEventManager>().CaptureSaveData(),
            project = ServiceLocater.Get<IProjectManager>().CaptureSaveData(),
            staff = staff
        };
    }

    private async UniTask RestoreGame(SaveRoot root)
    {
        if (root.staff != null)
            await ServiceLocater.Get<IStaffRegister>().RestoreSaveData(root.staff);
        
        ServiceLocater.Get<IGameManager>().RestoreSaveData(root.game);
        if (root.events != null) ServiceLocater.Get<IEventManager>().RestoreSaveData(root.events);
        if (root.project != null) ServiceLocater.Get<IProjectManager>().RestoreSaveData(root.project);
    }
    
    public void DeleteSlot(int slot)
    {
        SaveFileIO.Delete(FileOf(slot));               // 파일 삭제 (Exists 가드 내장 → 안전)
        _metas[slot] = null;                            // 메타 비움 → 빈 슬롯
        if (_currentSlot == slot) _currentSlot = -1;    // 현재 선택 슬롯이면 선택 해제(방어)
    }
}