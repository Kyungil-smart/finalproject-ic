using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class UserReviewRow
{
    public int mediaId;
    public int userId;
    public int genreId;
    public int themeId;
    public int positiveCommentId;
    public int negativeCommentId;
    public RequireReviewType reqType;
    public int reqValue;
}


// 유저 리뷰 읽기
// https://docs.google.com/spreadsheets/d/18pW0HvqUVgkDqUf6f7a9AZl7ZWxIZpGgOkGL5sexvbw/edit?gid=335899092#gid=335899092 칼럼 명 참고

[CreateAssetMenu(fileName = "UserReviewDataSO", menuName = "Data/UserReviewDataSO")]
public class UserReviewDataSO : ScriptableObject
{
    public List<UserReviewRow> userReviewList = new List<UserReviewRow>();
}
