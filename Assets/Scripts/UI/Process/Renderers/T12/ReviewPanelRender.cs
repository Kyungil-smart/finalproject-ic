using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviewPanelRender : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextLoader nickName;
    [SerializeField] private TextLoader comment;

    public void Render(ReviewData data)
    {
        faceImage.sprite = data.profileImg;
        iconImage.sprite = data.iconImg;
        nickName.TextId = data.nickNameId;
        comment.TextId = data.commentId;
        // Todo. Image 변경 코드 추가(필요 없을수도 있음).
    }
}