using UnityEngine;

public class ShuffleBooster : MonoBehaviour, IBooster
{
    private BoosterManager boosterManager;
    private string boosterName = "Shuffle";
    private int price = 800;
    private int unlockLevel = 3;
    private Constants.BoosterType boosterType = Constants.BoosterType.Shuffle;
    
    void Start()
    {
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.RegisterBooster(this);
        }
    }

    public void AddBooster(int amount)
    {
        CoreServices.Get<DataManager>().AddBooster((int)Constants.BoosterType.Shuffle,amount, price);
    }
    public void Excute()
    {
        CoreServices.Get<DataManager>().UseBooster((int)Constants.BoosterType.Shuffle);
        if(boosterManager == null) Debug.Log("boosterManager is null");
        AudioManager.Instance.PlayShuffleAudio();
        boosterManager.ShuffleBlock();
    }

    public int GetNumsBooster()
    {
        return CoreServices.Get<DataManager>().GetAmountOfBoosterByID((int)Constants.BoosterType.Shuffle);
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

    public int GetUnlockLevel()
    {
        return unlockLevel;
    }
}
