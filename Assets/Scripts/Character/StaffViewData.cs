using System.Collections.Generic;

/// <summary>
/// View 표시를 위한 데이터. InitData + RuntimeData를 View하기 좋게 바꾼 데이터 . 
/// </summary>
public class StaffViewData
{
    // 기본 정보
    public int Staff_ID;
    public string Staff_Name;
    public bool Staff_Gender;       
    public string Job_Name;         
    public int Avatar_ID;

    // 핵심 성향
    public string Grade;            
    public string DISC_Type;        

    // 상태 및 성장 정보 (RuntimeData 기반)
    public string Current_State;
    public int Current_Level;
    public int Current_Exp;
    
    // 돈
    public int Salary;
    public int Hire_Cost;

    // Base + Added 합산 결과
    public int Final_Career;
    public int Final_Common_Concentration;
    public int Final_Common_Creativity;
    public int Final_Common_Communication;
    public int Final_Job_Planning;
    public int Final_Job_Development;
    public int Final_Job_Art;

    // 보유 태그 목록 (Fixed_Tag + Added_Tags 통합) -> 나중에 Fixed_Tag 1개 int, Added_Tags 여러 개 List<int>로 나누도록 변경.
    public List<int> All_Tags = new List<int>();
}