public enum StaffHireResult
{
    Success,  // 고용 성공
    Available,  // 고용 가능 여부 판단
    Full,  // 이미 모든 TO 꽉참
    NoRecruiter,  // Recruit List 에 없음
    NotEnoughMoney  // 돈 부족
}