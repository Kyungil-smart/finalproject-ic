using Cysharp.Threading.Tasks;

// 직원을 고용하고 불러오는 서비스 인터페이스
public interface IStaffHireService
{
    UniTask HireStaffAsync(int count, int playerLevel);
    UniTaskVoid LoadStaffAsync();
}