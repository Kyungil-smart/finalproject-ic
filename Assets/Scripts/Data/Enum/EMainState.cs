using UnityEngine;

public enum EMainState
{
    None = 0,    // 사실상 사용 안함, 1~12 숫자 맞추기 위해 사용
    StaffManagingState,
    MarketResearchState,
    ConceptState,
    DesignPreProductionState,
    ArtPreProductionState,
    DevPreProductionState,
    DesignFullProductionState,
    ArtFullProductionState,
    DevFullProductionState,
    QAState,
    MarketingState,
    ReleaseState
}
