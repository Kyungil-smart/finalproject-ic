using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IReviewManager
{
    public List<ReviewResult> CheckRequirements();
    public UniTask<Sprite> RandomUserImgLoad();
}
