using System;
using System.Collections.Generic;
using UnityEngine;

public class ReviewData
{
    // public Sprite iconImg;
    public Sprite profileImg;
    public bool isPositive;
    public int nickNameId;
    public int commentId;
}

public class T12ReviewUIRenderData : UIRenderData
{
    public List<ReviewData> reviews;
    public Action btCallback;
}