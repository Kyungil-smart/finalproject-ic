using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubProcessStateMachine : MonoBehaviour
{


    [Header("전체 서브 상태 목록")]
    // IProcessState 는 인스펙터 창에 나오지 않아서 GameObject로 받아서 변환
    [SerializeField] private List<GameObject> _subStateObjects = new List<GameObject>();    

    // 실제 코드에서 사용할 인터페이스 리스트
    private List<IProcessState> _subStates = new List<IProcessState>();


    [Header("현재 실행 중인 서브 상태 데이터 (SO)")]
    [field: SerializeField] public ProcessStateSO CurrentSubState { get; private set; }


    [Header("현재 활성화된 서브 상태 컴포넌트 (구독 대상)")]
    [SerializeField] private IProcessState _subStateObject;

    [Header("발동할 서브 상태 큐 (전달받은 목록)")]
    [SerializeField] private List<ProcessStateSO> _subStateSOList = new List<ProcessStateSO>();

    // 모든 서브 상태가 종료되었음을 메인 상태 머신에 통보
    public event Action OnAllSubStatesFinished;

    // 큐에서 실행 중인 서브 상태의 인덱스
    private int _currentSubStateIndex = 0;



    private void Awake()
    {
        // 게임 시작 시 실제 인터페이스 리스트로 변환하여 사용
        foreach (var obj in _subStateObjects)
        {
            if (obj != null)
            {
                _subStates.Add(obj.GetComponent<IProcessState>());
            }
        }
    }


    // 에디터 상태에서 리스트에 추가된 오브젝트가 IProcessState 컴포넌트를 가지고 있는지 검증
    private void OnValidate()
    {
        if (_subStateObjects == null) return;

        for (int i = _subStateObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = _subStateObjects[i];

            // 빈 슬롯은 통과
            if (obj == null) continue;

            // IProcessState 컴포넌트가 있는지 검사
            if (obj.GetComponent<IProcessState>() == null)
            {
                Debug.LogWarning($"[{obj.name}] 오브젝트에 IProcessState를 구현한 컴포넌트가 없음");
                _subStateObjects.RemoveAt(i); // 유효하지 않은 오브젝트는 리스트에서 삭제
            }
        }
    }


    // 메인 상태 머신에서 서브 상태 리스트를 전달 -> 실행은 다른 곳에서
    public void ChangeSubStateList(List<ProcessStateSO> subStates)
    {
        _subStateSOList = subStates;
        _currentSubStateIndex = 0; // 인덱스 초기화

        if(_subStateSOList.Count == 0)
        {
            OnAllSubStatesFinished?.Invoke();
        }
    }


    // 실제 서브 상태 변경을 담당하는 함수
    public void RunSubState()
    {
        // 기존에 실행 중이던 서브 상태 컴포넌트가 있다면 구독 해제 및 종료 처리
        if (_subStateObject != null)
        {
            _subStateObject.OnStateFinished -= HandleSubStateFinished;
            // _subStateObject.Exit();
        }

        // 현재 서브 상태 정보 갱신
        CurrentSubState = _subStateSOList[_currentSubStateIndex];

        // 캐싱된 전체 서브 상태 컴포넌트 중, 인스펙터에 매칭된 SO와 같은 컴포넌트 찾기
        _subStateObject = _subStates.Find(state => state.CurrentStateDataSO == CurrentSubState);

        /* // 이름으로 매칭하는 방식 (버그가 있어서 사용 안함)
        _subStateObject = _subStates.Find(state =>
            state.GetType().Name.Contains(newState.StateName));
        */

        if (_subStateObject != null)
        {
            // 찾은 자식 오브젝트 스크립트 기능 발동 및 이벤트 구독
            _subStateObject.OnStateFinished += HandleSubStateFinished; // 완료 이벤트 구독
            _subStateObject.ChangeMyState(CurrentSubState);                  // 데이터 갱신
            _subStateObject.Enter();                                  // 기능 발동 -> 매니저가 발동하도록 수정
            _subStateObject.Execute();
            _subStateObject.Exit();
            // -> 추후 


            /*
            if (newState != null && newState.HasEvent)
            {
                // ServiceLocater를 통해 등록된 EventManager의 인터페이스를 가져옴
                IEventManager eventManager = ServiceLocater.Get<IEventManager>();

                if (eventManager != null)
                {
                    // 외부 요인 이벤트를 비동기(UniTaskVoid)로 발생
                    // 본 개발 절차가 시작되기 직전 타이밍에 고정적으로 실행
                    eventManager.OccurEvent(newState.RelatedEventType);

                    Debug.Log($"[SubStateMachine] : 이벤트 발동 완료 {newState.RelatedEventType} (상태: {newState.StateName})");
                }
                else
                {
                    Debug.LogWarning("[SubStateMachine] : EventManager를 ServiceLocater에서 찾을 수 없음");
                }
            }
            */
        }
        else
        {
            // 만약 대응하는 컴포넌트를 찾지 못했다면 경고를 띄우고 패스
            Debug.LogError($"[ProcessSubStateMachine] : {CurrentSubState.StateName} 데이터에 대응하는 자식 컴포넌트를 찾을 수 없음");
            MoveToNextSubState();
        }
    }

    // 발동했던 자식 오브젝트 기능이 끝나 OnStateFinished 이벤트를 보내왔을 때 실행되는 핸들러
    private void HandleSubStateFinished(IProcessState finishedState)
    {
        // 이벤트 구독 해제
        finishedState.OnStateFinished -= HandleSubStateFinished;

        // 다음 인덱스 반복 처리를 위한 함수 호출
        MoveToNextSubState();
    }

    // 반복 및 종료 처리를 담당하는 함수
    private void MoveToNextSubState()
    {
        _currentSubStateIndex++; // 인덱스 증가

        // 만약 리스트에 다음 서브 상태가 더 남아있다면 다시 ChangeSubState 발동
        if (_currentSubStateIndex < _subStateSOList.Count)
        {
            RunSubState();
        }
        // 더 이상 서브 상태가 없다면 (모두 끝났다면)
        else
        {
            // Debug.Log("[ProcessSubStateMachine] : 모든 서브 상태 루프 완료");
            _subStateObject = null;
            CurrentSubState = null;

            OnAllSubStatesFinished?.Invoke();
        }
    }


}