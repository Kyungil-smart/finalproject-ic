using System;
using System.Collections.Generic;
using UnityEngine;

public class T03TrendGenreThemeSelectRenderData : UIRenderData
{
    public List<NameTag> genres;
    public List<NameTag> themes;
    public Action<(NameTag genre, NameTag theme)> onSelectCallback ;
}

public class T03TrendGenreThemeResultRenderData : UIRenderData
{
    public NameTag genre;
    public NameTag theme;
    public Action goBackCallback;
    public Action goNextCallback;
}