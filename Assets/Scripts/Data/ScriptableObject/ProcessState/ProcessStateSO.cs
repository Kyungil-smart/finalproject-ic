using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct SubStateEventData
{
    public EventType eventType;
    public int occurRate;   // 이벤트 발생 확률 (0~100)
}


[CreateAssetMenu(fileName = "DevStateSO", menuName = "Scriptable Objects/DevStateSO")]
public class ProcessStateSO : ScriptableObject
{
    // 기본 정보, 이전 상태, 다음 상태, 실행 상태들 있어야 함
    [Header("상태 ID")]
    [field: SerializeField] public int StateID;

    [Header("상태 명칭")]
    [field: SerializeField] public string StateName;

    [Header("서브 상태 여부")]
    [field: SerializeField] public bool IsSubState;

    [Header("이전 단계")]
    public ProcessStateSO prevState;

    [Header("다음 단계")]
    public ProcessStateSO nextState;

    [Header("하위 단계")]
    public List<ProcessStateSO> subStates;    // 기획상 게임 중간에 삽입되지 않을 예정이니 list 대신 배열 사용


    // 추가로 필요한 정보
    [field: SerializeField] public bool IsLastSubStateFinished; // 게임 종료 확인([field: SerializeField]는 데이터 확인 위해서 임시로 넣음)


    [Header("이벤트 설정")]
    [SerializeField] private bool _hasEvent; // 이 상태가 시작될 때 이벤트를 실행할지 여부
    [SerializeField] private List<SubStateEventData> _eventData; // 실행할 이벤트의 데이터

    // 외부에서 읽을 수 있도록 프로퍼티 제공 (OOP 캡슐화)
    public bool HasEvent => _hasEvent;
    public List<SubStateEventData> RelatedEventData => _eventData;
}
