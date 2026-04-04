using UnityEngine;

public interface IBooster
{
    public void Setup(BoosterManager boosterManager);
    public void Excute();
    public void AddBooster(int amount); 
}
