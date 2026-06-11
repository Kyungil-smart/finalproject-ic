using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// 장르&테마_읽기, https://docs.google.com/spreadsheets/d/1IuTAkokLxpxIBwdIwTID_xZ0_3VCJKKEjA1GAd2Q_ec/edit?gid=1694145024#gid=1694145024 칼럼 명칭 그대로 가져옴
[Serializable]
public class GenreThemeRow
{
    public int GT_ID;
    public int Type;
    public int GT_Name_ID;
}

[CreateAssetMenu(fileName = "GenreThemeTypeDataSO", menuName = "Data/GenreThemeTypeDataSO")]
public class GenreThemeTypeDataSO : ScriptableObject
{
    public List<GenreThemeRow> genreThemeList = new List<GenreThemeRow>();

    // Type을 기준으로 특정 종류만 골라내는 기능
    public List<GenreThemeRow> GetGenreThemeListByType(int targetType)
    {
        return genreThemeList.Where(row => row.Type == targetType).ToList();
    }
}
