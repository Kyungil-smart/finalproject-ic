using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T12StaffRenderer : MonoBehaviour
{
    [SerializeField] private Image staffFaceImg;
    [SerializeField] private TextMeshProUGUI staffNameTxt;
    
    public void Render(Sprite staffSprite, string staffName, int staffLevel)
    {
        staffFaceImg.sprite = staffSprite;
        staffNameTxt.text = staffName;
    }
}