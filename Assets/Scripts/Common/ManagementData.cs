using UnityEngine;

public class ManagementData
{
    public int Money { get; private set; }
    public int Heart { get; private set; }
    
    public void AddMoney(int money) => Money += money;
    public void AddHeart(int heart) => Heart += heart;
}
