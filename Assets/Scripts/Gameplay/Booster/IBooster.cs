using UnityEngine;

public interface IBooster
{
    public string GetName();
    public Constants.BoosterType GetBoosterType();
    public int GetPrice();
    public void Setup(BoosterManager boosterManager);
    public void Excute();
    public void AddBooster(int amount); 
    public int GetNumsBooster();
    public int GetUnlockLevel();
}
