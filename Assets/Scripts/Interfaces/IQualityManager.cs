using Cysharp.Threading.Tasks;

public interface IQualityManager
{
    public QualityCalculate Calculator { get; }
    public UniTask ShowAchieveResult();
}