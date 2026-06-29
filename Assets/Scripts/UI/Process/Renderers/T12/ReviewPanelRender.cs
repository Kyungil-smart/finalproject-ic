using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviewPanelRender : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    // [SerializeField] private Image iconImage;
    [SerializeField] private Image positiveIcon;
    [SerializeField] private Image negativeIcon;
    [SerializeField] private TextLoader nickName;
    [SerializeField] private TextLoader comment;

    public void Render(ReviewData data)
    {
        faceImage.sprite = data.profileImg;
        // iconImage.sprite = data.iconImg;
        nickName.TextId = data.nickNameId;
        comment.TextId = data.commentId;

        positiveIcon.gameObject.SetActive(data.isPositive);
        negativeIcon.gameObject.SetActive(!data.isPositive);        
    }
}