using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;


public class SaveManager : Manager, ISaveManager
{
    private const int SlotCount = 3;
    private static string FileOf(int slot) => $"slot_{slot}.json";

    private SaveRoot[] _slots = new SaveRoot[SlotCount]; // null = 빈 슬롯
    private int _currentSlot = -1;  
    
    protected override void Register() => ServiceLocater.Register<ISaveManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<ISaveManager>(this);

    public void LoadAllSlots()
    {
        for (int i = 0; i < SlotCount; i++)
            _slots[i] = SaveSerializer.Deserialize<SaveRoot>(SaveFileIO.Read(FileOf(i))); // 없으면 null
    }

    public bool IsEmpty(int slot) => _slots[slot] == null;
    public SaveMeta GetMeta(int slot) => _slots[slot]?.meta;   // 카드가 이것만 읽음

    // 자동 저장: 현재 슬롯에 기록
    public void Save()
    {
        if (_currentSlot < 0) return;
        var root = CaptureCurrentGame();      // ← B단계: Manager → DTO 수집
        _slots[_currentSlot] = root;
        SaveFileIO.Write(FileOf(_currentSlot), SaveSerializer.Serialize(root));
    }

    // 슬롯 선택해서 게임 적용
    public async UniTask Load(int slot)
    {
        var root = _slots[slot] ??= SaveSerializer.Deserialize<SaveRoot>(SaveFileIO.Read(FileOf(slot)));
        if (root == null) return;
        _currentSlot = slot;
        await RestoreGame(root);                    // ← B단계: DTO → Manager 주입
    }

    // 새 게임: 빈 슬롯 잡고 현재 슬롯 지정
    public void StartNewGame(int slot) => _currentSlot = slot;

    private SaveRoot CaptureCurrentGame()
    {
        var gm = ServiceLocater.Get<IGameManager>();   
        var game = gm.CaptureSaveData();

        return new SaveRoot
        {
            version = 1,
            meta = new SaveMeta
            {
                playerName = game.playerName,
                playerLevel = game.playerLevel,
                completedProjectCount = game.projects.Count,
                year = gm.GetCalendarYear(),
                money = game.money,
                savedAt = DateTime.Now.ToString("yyyy.MM.dd HH:mm"),  // 실제 저장 시각
            },
            game = game,
            events = ServiceLocater.Get<IEventManager>().CaptureSaveData(),
            project = ServiceLocater.Get<IProjectManager>().CaptureSaveData(),
            staff = ServiceLocater.Get<IStaffRegister>().CaptureSaveData()
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
}