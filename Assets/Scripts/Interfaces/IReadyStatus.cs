using System.Collections.Generic;

public interface IReadyStatus
{   // 데이터 로딩을 먼저 끝내야 하는 것들은 일괄적으로 이거를 달도록 하자.
    public Dictionary<string, bool> ReadyStatus { get; }
}