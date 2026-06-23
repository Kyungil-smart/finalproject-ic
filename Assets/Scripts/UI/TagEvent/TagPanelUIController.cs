using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TagPanelUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tagName;
    [SerializeField] private TextMeshProUGUI _tagDesc;
    [SerializeField] private TextMeshProUGUI _tagEffectAName;
    [SerializeField] private TextMeshProUGUI _tagEffectBName;
    [SerializeField] private Button _selectBtn;

    private TagSelectUIController _mainController;
    private TagRow _tagRow;
    
    private void OnEnable() => _selectBtn.onClick.AddListener(OnSelect);
    private void OnDisable() => _selectBtn.onClick.RemoveListener(OnSelect);
    
    public void Render(TagRow tag, TagSelectUIController mainController)
    {
        // _icon ; Addressable 이나 다른 거 이용...해서! icon 가져오기. matching 으로는 아무래도....
        _tagRow = tag;
        _tagName.text = tag.Tag_Name;
        _tagDesc.text = tag.Tag_Desc;
        RenderTagEffect(_tagEffectAName, tag.Tag_A_Effect_Name);
        RenderTagEffect(_tagEffectBName, tag.Tag_B_Effect_Name);
        _mainController = mainController;
    }

    private void OnSelect()
    {
        Debug.Log("[TagPanelUIController] Select !!");
        _mainController.selectedTag.Value = _tagRow;
    }

    private void RenderTagEffect(TextMeshProUGUI target, string name)
    {   // ToDo. 임시로 한글로 변환한 내용. 추후 어떻게 할지 논의 필요.
        target.gameObject.SetActive(true);
        switch (name)
        {
            case "Staff_None_Effect":
                target.gameObject.SetActive(false);
                break;
            case "Staff_Concentration":
                target.text = "집중력";
                break;
            case "Staff_Creativity":
                target.text = "창조력";
                break;
            case "Staff_Communication":
                target.text = "소통력";
                break;
            case "Staff_Design":
                target.text = "기획력";
                break;
            case "Staff_Dev":
                target.text = "개발력";
                break;
            case "Staff_Art":
                target.text = "미술성";
                break;
        }
    }
}