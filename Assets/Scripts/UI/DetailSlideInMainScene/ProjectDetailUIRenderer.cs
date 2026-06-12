using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectDetailUIRenderer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI genre;
    [SerializeField] private TextMeshProUGUI theme;
    [SerializeField] private TextMeshProUGUI grade;
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private TextMeshProUGUI income;
    [SerializeField] private TextMeshProUGUI rewards;
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
        title.text = data.title;
        genre.text = data.genre;
        theme.text = data.theme;
        grade.text = data.grade;
        cost.text = data.cost.ToString();
        income.text = data.income.ToString();
        rewards.text = data.rewards;
        
        _callback =  callback;
    }
}