using System;

public class TutorialUIRenderData : UIRenderData
{
    public Action onGoBackCallback;
    public Action onGoNextCallback;

    public Action onTutorialCompleteCallback;   // 튜토리얼 끝났을 때 매니저의 세이브/후속 처리를 실행할 콜백
}
