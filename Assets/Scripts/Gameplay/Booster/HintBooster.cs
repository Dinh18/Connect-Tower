using UnityEngine;

public class HintBooster : MonoBehaviour, IBooster
{
    private BoosterManager boosterManager;
    private Constants.BoosterType boosterType = Constants.BoosterType.Hint;
    private string boosterName = "Hint";
    private int price = 900;
    private int unlockLevel = 2;
    
    void Start()
    {
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.RegisterBooster(this);
        }
    }

    public Constants.BoosterType GetBoosterType() => boosterType;

    public void AddBooster(int amount)
    {
        CoreServices.Get<DataManager>().AddBooster((int) Constants.BoosterType.Hint,amount, price);
    }

    public void Excute()
    {
        CoreServices.Get<DataManager>().UseBooster((int)Constants.BoosterType.Hint);
        bool searched = boosterManager.SearchedBlocks();
        if(searched)
        {
            AudioManager.Instance.PlayHintBoosterAudio();
            Debug.Log("Thuc hien Hint thanh cong");
        } 
        else Debug.Log("Hint khong thanh cong");
    }

    public int GetNumsBooster()
    {
        return CoreServices.Get<DataManager>().GetAmountOfBoosterByID((int)Constants.BoosterType.Hint);
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

    public int GetUnlockLevel()
    {
        return unlockLevel;
    }
}
