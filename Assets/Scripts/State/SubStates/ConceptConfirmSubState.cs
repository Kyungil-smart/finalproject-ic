using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ConceptConfirmSubState : MonoBehaviour, IProcessState
{
    [Header("이 상태의 SO")]
    [field: SerializeField] public ProcessStateSO CurrentStateDataSO { get; private set; }   // 현재 상태

    public event Action<IProcessState> OnStateFinished;

    public void Enter()
    {
        Debug.Log($"[ConceptConfirmSubState] : {CurrentStateDataSO.StateName} Sub 상태 진입");
        // TODO : 3. 게임 장르/테마 선정 시작
    }

    public async UniTask Execute()
    {
        SelectTheme();
    }

    public void Exit()
    {
        OnStateFinished?.Invoke(this);
        // TODO : 3. 게임 장르/테마 선정 종료
    }

    public void SelectTheme()
    {
        Debug.Log($"[ConceptConfirmSubState] : 테마 및 장르 결정 시작");
        //TODO : 제작할 게임의 테마 및 장르 결정
    }
}