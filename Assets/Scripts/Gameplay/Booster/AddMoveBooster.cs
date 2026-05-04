using System.Collections;
using UnityEngine;

public class AddMoveBooster : MonoBehaviour ,IBooster
{
    [SerializeField] private int amount;
    private BoosterManager boosterManager;
    private string boosterName = "Extra Moves";
    private Constants.BoosterType boosterType = Constants.BoosterType.AddMove;
    private int price = 900;
    private int unlockLevel = 1;
    private DataManager dataManager;
    
    void Start()
    {
        dataManager = CoreServices.Get<DataManager>();
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.RegisterBooster(this);
        }
    }

    public Constants.BoosterType GetBoosterType() => boosterType;
    public void AddBooster(int amount)
    {
        dataManager.AddBooster((int)Constants.BoosterType.AddMove,amount, price);
    }

    public void Excute()
    {
        dataManager.UseBooster((int)Constants.BoosterType.AddMove);
        AudioManager.Instance.PlayAddMoveAudio();
        StartCoroutine(StartInfiniteMovesCoroutine(10f));
    }

    private IEnumerator StartInfiniteMovesCoroutine(float time)
    {
        GameEventBus.Publish(new StartBorderFlashEvent { borderType = BorderType.Ice, flashSpeed = 1f, flashTime = time });
        yield return new WaitForSeconds(time);
        GameEventBus.Publish(new StopBorderFlashEvent());
    }

    public int GetNumsBooster()
    {
        return dataManager.GetAmountOfBoosterByID((int)Constants.BoosterType.AddMove);
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
