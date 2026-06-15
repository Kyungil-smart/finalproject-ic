using System;
using System.Collections.Generic;
/// <summary>
/// StaffData: 스태프의 기본정보를 담음.
/// StaffInitData: 시작시 StaffDataFactory에 의해서 랜덤값들이 초기화 되고 게임 진행할때 값이 변경되지 않는 데이터들 모음.  
/// StaffRuntimeData: 런타임에서 바뀔수 있는 데이터들
/// 초기화가 필요하며 Runtime에 바뀔수 있는 데이터는 Init에서 Base_Data로 초기화 하고 Runtime에서 변경되는 값은 Added_Data로 저장 후
/// 두값을 더해서(Or 곱해서) Data의 값을 계산
///
/// 
/// </summary>

[Serializable]
public class StaffInitData
{
    // 기본 정보
    public int Staff_ID; //시트에서 읽음.
    public string Staff_Name; // 시트에서 읽음
    public bool Staff_Gender; // 시트에서 읽음
    public JobType Job; // 시트에서 읽음
    public int Level = 1;
    public float Exp = 0;
    
    // 아바타 (아바타 ID값)
    public int Avatar_ID; // 성별, 직군에 따라 직업 범위내의 ID 랜덤 결정

    // 핵심 성향
    public StaffGrade Grade; // 시트에서 읽은 확률 표에 따라 랜덤 결정.
    public DiscType DISC_Type; // 랜덤 결정.

    // 능력치: 레벨에 의해 기본 결정 -> 태그, 등급에 적힌 능력치 배율or(+값) 적용하여 초기 능력치 결정  
    // 가챠로 뽑혔을 때의 베이스 스탯 및 경력 (StaffInitData의 Base와 StaffRuntimeData의 Added를 더하여 현재 스탯값계산)
    public int Base_Career; 
    public int Base_Common_Concentration;
    public int Base_Common_Creativity;
    public int Base_Common_Communication;
    public int Base_Job_Planning;
    public int Base_Job_Development;
    public int Base_Job_Art;

    // 비용 (초기 스탯 기반으로 계산)
    public int Salary;
    public int Hire_Cost;
}

[Serializable]
public class StaffRuntimeData
{
    public StaffState Current_State = StaffState.Idle; // 프로젝트에 투입되었는지.
    
    // 인게임 플레이로 추가 획득한 스탯 및 경력
    public int Added_Career = 0;
    public int Added_Common_Concentration = 0;
    public int Added_Common_Creativity = 0;
    public int Added_Common_Communication = 0;
    public int Added_Job_Design = 0;
    public int Added_Job_Development = 0;
    public int Added_Job_Art = 0;

    // 인게임에서 획득하는 추가 태그들 (나중에 구체적인 기획 나오면 수정)
    // 나중에 Fixed_Tag 1개 int, Added_Tags 여러 개 List<int>로 나누도록 변경.
    public List<TagRow> Added_Tags = new(); 
}