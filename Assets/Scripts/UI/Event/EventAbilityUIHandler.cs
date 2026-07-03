using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

[Serializable]
public class AbilityUI
{
    public GameObject plus;
    public GameObject minus;
}

public class EventAbilityUIHandler : MonoBehaviour
{
    [SerializeField] private TwoSelectScrollBarController twoTsController;
    [SerializeField] private OneSelectScrollBarController oneTsController;
    [SerializeField] private AbilityUI artUI;
    [SerializeField] private AbilityUI designUI;
    [SerializeField] private AbilityUI devUI;
    [SerializeField] private AbilityUI totalUI;
    [SerializeField] private AbilityUI costUI;

    [Header("민감도 > +,- 등장 시기")] 
    [SerializeField] [Range(0f, 0.5f)] private float single;
    [SerializeField] [Range(0f, 0.5f)] private float doubleLeft;
    [SerializeField] [Range(0.5f, 1f)] private float doubleRight;

    private EventEffectData _dataA;
    private EventEffectData _dataB;
    private readonly CompositeDisposable _disposables = new();
    
    public void SetData(List<EventEffectData> effectDataList)
    {
        if (effectDataList.Count == 1)
        {
            _dataB = effectDataList[0];
            oneTsController.scrollValue
                .Subscribe(value =>
                {
                    if (_dataA == null && _dataB == null) return;
                    // 1개짜리
                    if (value > single) Render(_dataB);
                    else Reset();
                }).AddTo(_disposables);
        }
        else
        {
            _dataA = effectDataList[0];
            _dataB = effectDataList[1];
            twoTsController.scrollValue
                .Subscribe(value =>
                {
                    if (_dataA == null && _dataB == null) return;
                    if (value < doubleLeft) Render(_dataA);
                    else if (value > doubleRight) Render(_dataB);
                    else Reset();
                }).AddTo(_disposables);
        }
    }

    private void Reset()
    {
        artUI.plus.SetActive(false);
        artUI.minus.SetActive(false);
        designUI.plus.SetActive(false);
        designUI.minus.SetActive(false);
        devUI.plus.SetActive(false);
        devUI.minus.SetActive(false);
        totalUI.plus.SetActive(false);
        totalUI.minus.SetActive(false);
        costUI.plus.SetActive(false);
        costUI.minus.SetActive(false);
    }
    
    private void Render(EventEffectData data)
    {
        artUI.plus.SetActive(data.target == "Art_Quality" && (data.value > 0 || data.ratio > 1));
        artUI.minus.SetActive(data.target == "Art_Quality" && data.value < 0);
        designUI.plus.SetActive(data.target == "Design_Quality" && (data.value > 0 || data.ratio > 1));
        designUI.minus.SetActive(data.target == "Design_Quality" && data.value < 0);
        devUI.plus.SetActive(data.target == "Dev_Quality" && (data.value > 0 || data.ratio > 1));
        devUI.minus.SetActive(data.target == "Dev_Quality" && data.value < 0);
        totalUI.plus.SetActive(data.target == "Total_Quality" && (data.value > 0 || data.ratio > 1));
        totalUI.minus.SetActive(data.target == "Total_Quality" && data.value < 0);
        costUI.plus.SetActive(data.target == "Money" && (data.value > 0 || data.ratio > 1));
        costUI.minus.SetActive(data.target == "Money" && data.value < 0);
    }
}
