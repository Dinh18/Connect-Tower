using UnityEngine;

public class AddMoveBooster : MonoBehaviour ,IBooster
{
    [SerializeField] private int amount;
    private BoosterManager boosterManager;
    private string boosterName = "Extra Moves";
    private Constants.BoosterType boosterType = Constants.BoosterType.AddMove;
    private int price = 900;
    private int unlockLevel = 1;
    
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
        DataManager.Instance.AddBooster((int)Constants.BoosterType.AddMove,amount, price);
    }

    public void Excute()
    {
        CoreServices.Get<GameManager>().AddMove(amount);
        DataManager.Instance.UseBooster((int)Constants.BoosterType.AddMove);
        AudioManager.Instance.PlayAddMoveAudio();
        Debug.Log("Da them luot di chuyen");
    }

    public int GetNumsBooster()
    {
        return DataManager.Instance.GetAmountOfBoosterByID((int)Constants.BoosterType.AddMove);
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
