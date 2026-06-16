using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T12StaffRenderer : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI staffNameTxt;
    [SerializeField] private TextMeshProUGUI staffLevelTxt;

    public void Render(GameDevProcName procName, string staffName, int staffLevel)
    {
        // ToDo. procName 별로 image 교체
        staffNameTxt.text = staffName;
        staffLevelTxt.text = $"Lv. {staffLevel:D2}";
    }
}