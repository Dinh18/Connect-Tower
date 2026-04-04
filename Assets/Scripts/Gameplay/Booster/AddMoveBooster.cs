using UnityEngine;

public class AddMoveBooster : MonoBehaviour ,IBooster
{
    [SerializeField] private int amount;
    private BoosterManager boosterManager;
    public void AddBooster(int amount)
    {
        DataManager.Instance.AddBooster((int)Constants.BoosterType.AddMove,amount);
    }

    public void Excute()
    {
        if(DataManager.Instance.GetAmountOfBoosterByID((int)Constants.BoosterType.AddMove) <= 0) return;
        GameManager.Instance.AddMove(amount);
        DataManager.Instance.UseBooster((int)Constants.BoosterType.AddMove);
        Debug.Log("Da them luot di chuyen");
    }

    public void Setup(BoosterManager boosterManager)
    {
        this.boosterManager = boosterManager;
    }
}
