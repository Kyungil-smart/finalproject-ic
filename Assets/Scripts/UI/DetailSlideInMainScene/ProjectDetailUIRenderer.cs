using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

[Serializable]
public class UserReviewShow
{
    public Image thumbUp;
    public Image thumbDown;
    public TextLoader userName;
    public TextLoader comments;
} 

public class ProjectDetailUIRenderer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI company;
    [SerializeField] private TextMeshProUGUI projectName;
    [SerializeField] private TextMeshProUGUI genre;
    [SerializeField] private TextMeshProUGUI theme;
    [SerializeField] private TextMeshProUGUI grade;
    [SerializeField] private TextMeshProUGUI rewards;
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private List<UserReviewShow> reviews;
    
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    
    [SerializeField] private Button closeBt;
    private Action _callback;

    private void OnEnable()
    {
        closeBt.onClick.AddListener(() => _callback?.Invoke());
    }

    private void OnDisable()
    {
        closeBt.onClick.RemoveAllListeners();
    }
    
    public void Render(ProjectDetailRenderData data, Action callback)
    {
        if (data.index == 0)
            rightArrow.SetActive(true);
        else if (data.index == ServiceLocater.Get<IGameManager>().GetProjectYear() - 2)
            leftArrow.SetActive(true);
        else
        {
            rightArrow.SetActive(true);
            leftArrow.SetActive(true);
        }

        company.text = ServiceLocater.Get<IGameManager>().PlayerName;
        projectName.text = data.projectName;
        genre.text = data.genre;
        theme.text = data.theme;
        grade.text = data.grade;
        rewards.text = data.rewards;
        cost.text = data.cost.ToString("N0");
        income.text = data.income.ToString("N0");
        if (data.reviews != null)
        {
            for (int i = 0; i < data.reviews.Count; i++)
            {
                reviews[i].userName.TextId = data.reviews[i].userReviewRow.userId;
                if (data.reviews[i].isPositiveComment)
                {
                    reviews[i].comments.TextId = data.reviews[i].userReviewRow.positiveCommentId;
                    reviews[i].thumbUp.gameObject.SetActive(true);
                }
                else
                {
                    reviews[i].comments.TextId = data.reviews[i].userReviewRow.negativeCommentId;
                    reviews[i].thumbDown.gameObject.SetActive(true);
                }
            }    
        }
        _callback = callback;
    }
}