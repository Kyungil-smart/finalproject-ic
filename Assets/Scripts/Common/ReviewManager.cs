using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReviewManager : Manager, IReviewManager, IReadyStatus
{
    [SerializeField] private UserReviewDataSO reviewTasks;


    // TODO : SO 를 ghseet 에서 받아와야함 (현재 gsheet 구버젼이라 갱신되면 확인 필요)
    [SerializeField] private string gSheetId;
    [SerializeField] private string gid;

    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;


    private void OnEnable() => Register();
    private void OnDisable() => Unregister();

    protected override void Register()
    {
        ServiceLocater.Register<IReviewManager>(this);
        ServiceLocater.Register<IEventRouter>(new EventRouter());
    }

    protected override void Unregister()
    {
        ServiceLocater.Unregister<IReviewManager>(this);
        ServiceLocater.Unregister<IEventRouter>(new EventRouter());
    }

    protected override void Init()
    {
        Debug.Log("[ReviewManager] Initializing...");
        DownloadData();
    }

    private async UniTaskVoid DownloadData()
    {
        // TODO : 현재 gsheet 구버젼이라, 기획에서 gsheet 수정된 이후 주석 해제 후 테스트 필요
        /*
        if (!Utils.Environment.isDevelopment) return;
        _readyStatus["ReviewData"] = false;

        GSheetManager gsManager = new GSheetManager(gSheetId, gid);
        await Utils.TaskAsync.WaitUntilOrThrowAsync(() => gsManager.IsDownload);
        var dataList = gsManager.GetData();
        reviewTasks.userReviewList.Clear();
        foreach (var data in dataList)
        {
            reviewTasks.userReviewList.Add(new UserReviewRow()
            {
                mediaId = int.Parse(data["MediaId"]),
                userId = int.Parse(data["UserId"]),
                genreId = int.Parse(data["GenreId"]),
                themeId = int.Parse(data["ThemeId"]),
                positiveCommentId = int.Parse(data["PositiveCommentId"]),
                negativeCommentId = int.Parse(data["NegativeCommentId"]),
                reqType = Enum.Parse<RequireReviewType>(data["ReqType"].ToString()),
                reqValue = int.Parse(data["ReqValue"]),
            });
        }
        _readyStatus["ReviewData"] = true;
        */
    }

    // 평점 계산하기
    public void CheckRequirements()
    {
        // and 조건으로 장르랑 테마 확인해서 리스트화

        // 리스트에서 단순 랜덤으로 최대 3개 뽑기(기획 쪽에서 데이터는 꼭 3개가 나오도록 맞춘다고 함)

        // 점수 이상인지 이하인지 확인

        // T12에 전달하기

        
    }
}
