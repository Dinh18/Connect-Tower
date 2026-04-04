using UnityEngine;

public class ShuffleBooster : MonoBehaviour, IBooster
{
    private BoosterManager boosterManager;
    public void AddBooster(int amount)
    {
        DataManager.Instance.AddBooster((int)Constants.BoosterType.Shuffle,amount);
    }
    public void Excute()
    {
        if(DataManager.Instance.GetAmountOfBoosterByID((int)Constants.BoosterType.Shuffle) <= 0) return;
        DataManager.Instance.UseBooster((int)Constants.BoosterType.Shuffle);
        if(boosterManager == null) Debug.Log("boosterManager is null");
        StartCoroutine(boosterManager.ShuffleBlock());
    }

    public void Setup(BoosterManager boosterManager)
    {
        this.boosterManager = boosterManager;   
    }
}
