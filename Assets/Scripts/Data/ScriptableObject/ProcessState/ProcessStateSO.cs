using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DevStateSO", menuName = "Scriptable Objects/DevStateSO")]
public class ProcessStateSO : ScriptableObject
{
    // 기본 정보, 이전 상태, 다음 상태, 실행 상태들 있어야 함
    [Header("상태 ID")] public int StateID;
    [Header("상태 Enum")] public GameDevProcName gameDevProcName;
    [Header("상태 명칭")] public int stateNameId;

    [Header("이전 단계")] public ProcessStateSO prevState;
    [Header("다음 단계")] public ProcessStateSO nextState;

    [Header("Related Image")] public string imageID;
    
    [Header("활용되는 이벤트 타입")] public List<EventType> eventType;

    // 12 절차별 발생된 이벤트를 초기화하기 위함
    [Header("첫 절차만 True")] public bool resetEvent;
}
