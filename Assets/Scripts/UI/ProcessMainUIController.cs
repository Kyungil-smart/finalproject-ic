using DataDispatcher;
using TMPro;
using UnityEngine;
using R3;
using Channel = DataDispatcher.Channel;

public class ProcessMainUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI projectName;
    [SerializeField] private GameObject prevStepSession;
    [SerializeField] private TextLoader prevStepName;    
    [SerializeField] private TextLoader curStepName;
    [SerializeField] private GameObject nextStepSession;
    [SerializeField] private TextLoader nextStepName;

    private IGameManager _gameManager;
    private IProjectManager _projectManager;
    private IPostManager _postManager;
    private readonly CompositeDisposable _disposables = new();
    
    private void Start()
    {
        _projectManager = ServiceLocater.Get<IProjectManager>();
        _gameManager = ServiceLocater.Get<IGameManager>();
        _postManager = ServiceLocater.Get<IPostManager>();
        
        projectName.text = $"Project: {_projectManager?.GetProjectData()?.name}";
        UpdateDateUI();
        UpdateGoldUI();
        var data = _postManager.Request<bool, StateViewData>(Channel.ProcessUIUpdate, true);
        UpdateProcessData(data);
    }
    
    private void UpdateGoldUI()
    {
        _gameManager.Money
            .Subscribe(gold =>
            {
                moneyText.text = gold.ToString("N0");;
            }).AddTo(_disposables);
    }

    private void UpdateDateUI()
    {
        _gameManager.Date
            .DistinctUntilChanged()
            .Subscribe(date =>
            {
                dateText.text = date.ToString("yyyy") + "년";            
            }).AddTo(_disposables);
    }
    
    private void UpdateProcessData(StateViewData stateView)
    {
        if (stateView.prev.id <= 0)
        {
            prevStepName.TextId = 0;
            prevStepSession.SetActive(false);
        }
        else
        {
            prevStepName.TextId = stateView.prev.textId;
            prevStepSession.SetActive(true);
        }
        curStepName.TextId = stateView.current.textId;
        if (stateView.next.id <= 0)
        {
            nextStepName.TextId = 0;
            nextStepSession.SetActive(false);
        }
        else
        {
            nextStepName.TextId = stateView.next.textId;
            nextStepSession.SetActive(true);
        }  
    }
}