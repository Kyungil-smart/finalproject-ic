// 스태프 기본 정보 출력을 위한 인터페이스
public interface IStaffInfo
{
    int GetStaffID();
    string GetFullName();
    JobType GetJob();
    StaffGrade GetGrade();
    int GetSalary();
    DiscType GetDiscType();
    
    // 외부에서는 최종 스탯만 가져가게 설계 (나머지는 나중에 추가)
    int GetTotalCareer();
    
    int GetTotalConcentration();
    int GetTotalCreativity();

    int GetTotalCommunication(); 
    int GetPlanning();
    int GetDevelopment();
    int GetArt();
    
    
    
    void DisplayInfo();
}

// 나중에 스태프 정보 업데이트 인터페이스도 추가하기.  Set~..() 

