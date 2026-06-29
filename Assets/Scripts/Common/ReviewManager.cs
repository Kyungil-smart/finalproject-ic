using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AddressableAssets;

[Serializable]
public struct ReviewResult
{
    public UserReviewRow userReviewRow;
    public bool isPositiveComment;  // 긍정 리뷰인지 여부

    public ReviewResult(UserReviewRow row)
    {
        this.userReviewRow = row;
        this.isPositiveComment = false; // 초기값 설정
    }
}


public class ReviewManager : Manager, IReviewManager, IReadyStatus
{
    [SerializeField] private UserReviewDataSO reviewTasks;


    // TODO : SO 를 ghseet 에서 받아와야함 (현재 gsheet 구버젼이라 갱신되면 확인 필요)
    [SerializeField] private string gSheetId;
    [SerializeField] private string gid;

    private Dictionary<string, bool> _readyStatus = new();
    public Dictionary<string, bool> ReadyStatus => _readyStatus;

    private const string FACEIMG_LABEL = "Staff_Thumnail";

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
                reqType = System.Enum.Parse<RequireReviewType>(data["ReqType"].ToString()),
                reqValue = int.Parse(data["ReqValue"]),
            });
        }
        _readyStatus["ReviewData"] = true;
        */
    }

    // 대상 리뷰 골라내 넣는 기능
    public List<ReviewResult> CheckRequirements()
    {
        List<ReviewResult> tempResultList = new List<ReviewResult>();   // 대상이 되는 모든 리뷰 가져오기
        List<ReviewResult> resultList = new List<ReviewResult>();   // 대상이 되는 모든 리뷰 중 3개만 최종적으로 골라내기(단순 랜덤ㄴ)

        // 기존에 선택된 장르와 테마 가져오기
        NameTag currentGenre = ServiceLocater.Get<IProjectManager>().Genre;
        NameTag currentTheme = ServiceLocater.Get<IProjectManager>().Theme;
        int targetGenreId = currentGenre.id;
        int targetThemeId = currentTheme.id;

        // and 조건으로 장르랑 테마 확인해서 리스트화
        foreach (var item in reviewTasks.userReviewList)
        {
            if (item.genreId == targetGenreId && item.themeId == targetThemeId)
            {
                tempResultList.Add(new ReviewResult(item));
            }
        }

        // 리스트에서 단순 랜덤으로 최대 3개 뽑기(기획 쪽에서 데이터는 꼭 3개가 나오도록 맞춘다고 함)
        if (tempResultList.Count >= 3)
        {
            // Unity의 Random.value를 기준으로 리스트를 무작위 정렬(Shuffle)한 뒤 최상위 3개만 추출
            resultList = tempResultList
                .OrderBy(x => Random.value)
                .Take(3)
                .ToList();
        }
        else
        {
            resultList = new List<ReviewResult>(tempResultList);    // 데이터가 3개 미만일 경우 다 담기
        }

        // 점수 이상인지 이하인지 확인
        IProjectManager projectManager = ServiceLocater.Get<IProjectManager>();

        for (int i = 0; i < resultList.Count; i++)
        {
            // 리스트에서 원본 데이터 복사 (값 타입 구조체이므로 임시 변수에 대입)
            ReviewResult result = resultList[i];

            float currentStatValue = 0f;

            switch (result.userReviewRow.reqType)
            {
                case RequireReviewType.Design:
                    currentStatValue = projectManager.DesignQuality;
                    break;
                case RequireReviewType.Dev:
                    currentStatValue = projectManager.DevQuality;
                    break;
                case RequireReviewType.Art:
                    currentStatValue = projectManager.ArtQuality;
                    break;
                case RequireReviewType.Total:
                    currentStatValue = projectManager.TotalQuality;
                    break;
                case RequireReviewType.None:
                default:
                    currentStatValue = 0f;
                    break;
            }

            // 요구 수치 이상일 경우 isPositiveComment true로
            if (currentStatValue >= result.userReviewRow.reqValue)
            {
                result.isPositiveComment = true;
            }
            else
            {
                result.isPositiveComment = false;
            }

            resultList[i] = result;
        }

        // 외부에 전달하기
        return resultList;
    }

    // 어드레서블로 faceimg 로드
    public async UniTask<List<Sprite>> RandomUserImgLoad(int count)
    {
        var result = new List<Sprite>();
        
        var imgHandle = Addressables.LoadResourceLocationsAsync(FACEIMG_LABEL, typeof(Sprite));
        var location = await imgHandle.ToUniTask();
        
        var pickImg = location.OrderBy(x => Random.value).Take(count).ToList();
        
        Addressables.Release(imgHandle);

        foreach (var loc in pickImg)
        {
            var sprite = await Addressables.LoadAssetAsync<Sprite>(loc).ToUniTask();
            result.Add(sprite);
        }
        
        return result;
    }


    // TODO : SO 다운로드 받기 기능 추가 필요
    [ContextMenu("데이터 다운로드")]
    private void DataDownload()
    {
        DownloadData();
    }
}
