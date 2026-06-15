using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class TagSelectUIController : MonoBehaviour, IUIRender
{
    [Header("UI Object Setting")] 
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button tagRerollBtn;
    [SerializeField] private TextMeshProUGUI rerollTxt;
    [SerializeField] private Button tagConfirmBtn;
    [SerializeField] private List<TagPanelUIController> tagPanels;
    [SerializeField] private TagPanelUITargetStaffRender tagPanelTargetStaffRender;
    
    [Header("Init Data Setting")]
    [SerializeField] private uint initCost = 300;
    [SerializeField] private uint incrementCost = 300;

    public ReactiveProperty<TagRow> selectedTag = new();
    private uint _cost;
    
    // ToDo. 추후 Text 내용 데이터로 받을 수 있게 준비하기
    private string _presetRerollText() => $"다른 선택지 보기: {_cost}";

    private void Awake() => _cost = initCost;

    private void OnEnable()
    {
        tagRerollBtn.onClick.AddListener(Reroll);
    }
    private void OnDisable()
    {
        tagRerollBtn.onClick.RemoveListener(Reroll);
    }

    private void Start()
    {
        rerollTxt.text = _presetRerollText();
        selectedTag.Subscribe(tag =>
        {
            if (tag == null) tagConfirmBtn.gameObject.SetActive(false);
            else tagConfirmBtn.gameObject.SetActive(true);
        }).AddTo(this);
        GetRandomThreeTags();
    }
    
    public void Render(UIRenderData data)
    {
        if (data is TagUIRenderData renderData)
        {
            mainPanel.SetActive(true);
            RenderStaffData(renderData.StaffEntity);
            tagConfirmBtn.onClick.RemoveAllListeners();
            tagConfirmBtn.onClick.AddListener(() => renderData.OnConfirmCallback(selectedTag.CurrentValue.Tag_Id));
            tagConfirmBtn.onClick.AddListener(() => mainPanel.SetActive(false));
        }
    }

    private void GetRandomThreeTags()
    {
        var staffDataManger = ServiceLocater.Get<IStaffDataManager>();
        int allTagCount = staffDataManger.TagList.Count;
        var indexes = NumberExtractor.GetUniqueRandomNumbers(0, allTagCount, 3);
        for (int i = 0; i < indexes.Length; i++)
            tagPanels[i].Render(staffDataManger.TagList[indexes[i]], this);
    }
    
    private void Reroll()
    {
        GetRandomThreeTags();
        _cost += incrementCost;  
        rerollTxt.text = _presetRerollText();
    }

    private void RenderStaffData(StaffEntity staffEntity)
    {
        tagPanelTargetStaffRender.Render(staffEntity);
    }
}