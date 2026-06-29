using System.Collections.Generic;
using UnityEngine;

public interface IReviewManager
{
    public List<ReviewResult> CheckRequirements();
}
