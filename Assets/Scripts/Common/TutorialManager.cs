using UnityEngine;

public class TutorialManager : Manager, ITutorialManager
{
    private int tutorialUIIndex = 0;


    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register() => ServiceLocater.Register<ITutorialManager>(this);
    protected override void Unregister() => ServiceLocater.Unregister<ITutorialManager>(this);

    // 튜토리얼 시작
    public void Start()
    {
        Debug.Log($"[TutorialManager] : Tutorial Start {ServiceLocater.Get<IGameManager>().IsFinishTutorial}");
        if (!ServiceLocater.Get<IGameManager>().IsFinishTutorial)
        {
            StartTutorial();
        }
    }


    // 튜토리얼 기능 시작, 추후 튜토리얼 버튼이 생길 수도 있으니 기능 따로 빼놓음
    public void StartTutorial()
    {
        tutorialUIIndex = 0;

        TutorialUIRenderData renderData = new TutorialUIRenderData
        {
            onGoNextCallback = GoToNext,
            onGoBackCallback = GoToPrevious,
            onTutorialCompleteCallback = CompleteTutorial
        };

        ServiceLocater.Get<IUIRouter>().NavigateTo(UIType.TutorialUI, renderData);
    }

    // 이전으로 돌아가기
    public void GoToPrevious()
    {
        if (tutorialUIIndex > 0) tutorialUIIndex--;
    }

    // 다음으로 가기
    public void GoToNext()
    {
        tutorialUIIndex++;
    }

    // 튜토리얼 끝나면 안 나오도록 처리
    private void CompleteTutorial()
    {
        ServiceLocater.Get<IGameManager>().SetBoolTutorial(true);
        Debug.Log($"[TutorialManager] : Tutorial Finished {ServiceLocater.Get<IGameManager>().IsFinishTutorial}");
    }
}
