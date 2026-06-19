using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class T0203UIController : MonoBehaviour, IUIRender
{
    [Header("Common UI Components")] 
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TextLoader title;
    
    [Header("T02 UI Components")]
    [SerializeField] private GameObject middleSession01;
    [SerializeField] private GameObject tailSession01;
    [SerializeField] private Button confirmBtn;

    [Header("T03-1 UI Components")] 
    [SerializeField] private GameObject middleSession02;
    [SerializeField] private GameObject tailSession02;
    [SerializeField] private DataRolePicker genrePicker;
    [SerializeField] private DataRolePicker themePicker;
    [SerializeField] private Button selectBtn;
    
    [Header("T03-2 UI Components")]
    [SerializeField] private GameObject middleSession03;
    [SerializeField] private GameObject tailSession03;
    [SerializeField] private TextMeshProUGUI genre;
    [SerializeField] private TextMeshProUGUI theme;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;

    private T03TrendGenreThemeSelectRenderData _t031Cache;
    
    private void OnEnable()
    {
        ServiceLocater.Get<IUIRouter>().RegisterUIRender(UIType.MarketGenreThemeUI, this);
    }

    private void OnDisable()
    {
        ServiceLocater.Get<IUIRouter>().UnregisterUIRender(UIType.MarketGenreThemeUI);
    }
    
    public void Render(UIRenderData data)
    {
        Debug.Log("[T0203UIController] Render");
        mainPanel.SetActive(true);
        if (data is T02TrendGenreThemeRenderData renderT2Data)
        {
            Debug.Log("[T0203UIController] Render - T02");
            title.Text = "트랜드 조사 결과"; // ToDo. 추후 Text ID 로 변경 예정
            
            confirmBtn.onClick.RemoveAllListeners();
            confirmBtn.onClick.AddListener(() => middleSession01.SetActive(false));
            confirmBtn.onClick.AddListener(() => tailSession01.SetActive(false));
            confirmBtn.onClick.AddListener(() => mainPanel.SetActive(false));
        } 
        else if (data is T03TrendGenreThemeSelectRenderData renderT31Data)
        {
            Debug.Log("[T0203UIController] Render - T03");
            title.Text = "게임 장르 & 테마 선정"; // ToDO. 추후 Text ID 로 변경 예정
            middleSession02.SetActive(true);
            tailSession02.SetActive(true);
            
            _t031Cache = renderT31Data;
            genrePicker.SetDataList(renderT31Data.genres);
            themePicker.SetDataList(renderT31Data.themes);
            
            selectBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.AddListener(ConnectSelectGenreAndThemes);
            selectBtn.onClick.AddListener(() => middleSession02.SetActive(false));
            selectBtn.onClick.AddListener(() => tailSession02.SetActive(false));
            selectBtn.onClick.AddListener(() => mainPanel.SetActive(false));
        }
        else if (data is T03TrendGenreThemeResultRenderData renderT32Data)
        {
            title.Text = "게임 장르 & 테마 결과"; // ToDO. 추후 Text ID 로 변경 예정
            middleSession03.SetActive(true);
            tailSession03.SetActive(true);
            
            previousBtn.onClick.RemoveAllListeners();
            previousBtn.onClick.AddListener(() => renderT32Data.goBackCallback?.Invoke());
            previousBtn.onClick.AddListener(() => middleSession03.SetActive(false));
            previousBtn.onClick.AddListener(() => tailSession03.SetActive(false));
            previousBtn.onClick.AddListener(() => mainPanel.SetActive(false));

            genre.text = renderT32Data.genre.name;
            theme.text = renderT32Data.theme.name;
            
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(() => renderT32Data.goNextCallback?.Invoke());
            nextBtn.onClick.AddListener(() => middleSession03.SetActive(false));
            nextBtn.onClick.AddListener(() => tailSession03.SetActive(false));
            nextBtn.onClick.AddListener(() => mainPanel.SetActive(false));
        }
    }

    private void ConnectSelectGenreAndThemes()
    {
        genrePicker.GetCenterData();
        _t031Cache.onSelectCallback?.Invoke((genrePicker.GetCenterData(), themePicker.GetCenterData()));
    }
}