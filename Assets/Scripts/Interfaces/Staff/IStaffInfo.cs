// 스태프 기본 정보 출력을 위한 인터페이스

using UnityEngine;

public interface IStaffInfo
{
    public GameObject GetGameObject();
    public GameObject GetAvatar();
    public int GetStaffID();
    public string GetFullName();
    public JobType GetJob();
    public StaffGrade GetGrade();
    public int GetSalary();
    public DiscType GetDiscType();
    
    // 외부에서는 최종 스탯만 가져가게 설계 (나머지는 나중에 추가)
    public int GetTotalCareer();
    
    public int GetConcentration();
    public int GetCreativity();

    public int GetCommunication(); 
    public int GetDesign();
    public int GetDevelopment();
    public int GetArt();
    void DisplayInfo();
}

// 나중에 스태프 정보 업데이트 인터페이스도 추가하기.  Set~..() 

