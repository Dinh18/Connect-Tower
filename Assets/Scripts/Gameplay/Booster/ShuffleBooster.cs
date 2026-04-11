using UnityEngine;

public class ShuffleBooster : MonoBehaviour, IBooster
{
    private BoosterManager boosterManager;
    private string boosterName = "Shuffle";
    private int price = 800;
    private Constants.BoosterType boosterType = Constants.BoosterType.Shuffle;
    public void AddBooster(int amount)
    {
        DataManager.Instance.AddBooster((int)Constants.BoosterType.Shuffle,amount, price);
    }
    public void Excute()
    {
        DataManager.Instance.UseBooster((int)Constants.BoosterType.Shuffle);
        if(boosterManager == null) Debug.Log("boosterManager is null");
        AudioManager.Instance.PlayShuffleAudio();
        StartCoroutine(boosterManager.ShuffleBlock());
    }

    public int GetNumsBooster()
    {
        return DataManager.Instance.GetAmountOfBoosterByID((int)Constants.BoosterType.Shuffle);
    }

    public void Setup(BoosterManager boosterManager)
    {
        this.boosterManager = boosterManager;   
    }

    public int GetPrice()
    {
        return price;
    }

    public string GetName()
    {
        return boosterName;
    }

    public Constants.BoosterType GetBoosterType()
    {
        return boosterType;
    }
}
