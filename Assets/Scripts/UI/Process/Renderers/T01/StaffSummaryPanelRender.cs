using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffSummaryPanelRender : MonoBehaviour
{
    [Header("Render Data")]
    [SerializeField] private Image faceImage;
    [SerializeField] private TextMeshProUGUI staffIdText;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI hiredText;
    [SerializeField] private TextMeshProUGUI jobTxt;
    [SerializeField] private TextMeshProUGUI gradeTxt;
    [SerializeField] private TextMeshProUGUI tag1Txt;
    [SerializeField] private TextMeshProUGUI tag2Txt;
    
    [Header("Toggle")]
    [SerializeField] private Toggle panelToggle;
    [SerializeField] private Image imageToggle;
    [SerializeField] private Button detailButton;
    [SerializeField] private Color trueColor = new Color(0.7f, 0.9f, 0.7f, 0.5f);  // 켜졌을 때 (예: 연녹색)
    [SerializeField] private Color falseColor = new Color(1.0f, 1.0f, 1.0f, 0.0f); // 꺼졌을 때 (예: 흰색)
    [SerializeField] private AudioClip btnClip;

    [Header("Detail")] 
    [SerializeField] private StaffDatailUIRenderer detailPanel;
    
    public Observable<(bool isOn, int index)> OnItemSelecte => onItemSelected;
    private readonly Subject<(bool isOn, int index)> onItemSelected = new();
    private int _itemIndex;
    private bool _isRendering;
    
    public void SetUp(int index) => _itemIndex = index;
    
    public void OnClickSelect() => onItemSelected.OnNext((panelToggle.isOn, _itemIndex));
    
    private string StaffId(int id) => $"ID: {id}";
    private string Hired(bool hired) => hired ? "" : "New";

    private StaffViewData _data;
    private bool _selectable;
    
    private void Start()
    {
        if (panelToggle)
        {
            panelToggle.onValueChanged
                .AsObservable()
                .Subscribe(isOn =>
                {
                    if (_isRendering) return;
                    ServiceLocater.Get<ISoundManager>().PlaySfx(btnClip);
                    if (_selectable) imageToggle.color = isOn ? trueColor : falseColor;
                    onItemSelected.OnNext((isOn, _itemIndex));
                })
                .AddTo(this);
        }
        detailButton?.onClick.RemoveAllListeners();
        detailButton?.onClick.AddListener(ViewDetail);
    }

    public void Render(StaffViewData data, bool hired, bool selected, bool selectable)
    {
        _isRendering = true;
        _data = data;
        faceImage.sprite = data.Thumbnail;
        staffIdText.text = StaffId(data.Staff_ID);
        nameTxt.text = data.Staff_Name;
        hiredText.text = Hired(hired);
        jobTxt.text = data.Job_Name;
        gradeTxt.text = data.Grade;
        tag1Txt.text = null;  // ToDo. Tag 개발 완료시 추가
        tag2Txt.text = null;
        if (panelToggle)
        {
            panelToggle.interactable = selectable;
            _selectable = selectable;
            panelToggle.isOn = selected;
            imageToggle.color = selected ? trueColor : falseColor;
        }
        _isRendering = false;
    }
    
    public void RevertSelection()
    {
        _isRendering = true;
        panelToggle.isOn = false;
        imageToggle.color = falseColor;
        _isRendering = false;
    }

    private void OnDestroy()
    {
        onItemSelected.OnCompleted();
    }

    private void ViewDetail()
    {
        detailPanel.Render(_data);
        detailPanel.gameObject.SetActive(true);
    }
}
