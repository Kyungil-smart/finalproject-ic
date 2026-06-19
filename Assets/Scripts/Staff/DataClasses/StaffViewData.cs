using System.Collections.Generic;
using UnityEngine;

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
    public JobType Job_Type;
    public Sprite Thumbnail;

    // 핵심 성향
    public string Grade;            
    public string DISC_Type;        
    // 나중에 Fixed_Tag 추가

    // 상태 및 성장 정보 (RuntimeData 기반)
    public string Current_State;
    public int Level;
    public float Exp;
    
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

    public List<string> All_Tags = new ();
}