using UnityEngine;

public enum BoosterType
{
    AddMove = 0,
    Shuffle = 1,
    Hint = 2,
    Undo = 3,
}

public class Booster : MonoBehaviour
{
    protected BoosterType boosterType;
    public string GetName()
    {
        return CoreServices.Get<DataManager>().GetBooster((int)GetBoosterType()).name;
    }
    public virtual BoosterType GetBoosterType() => boosterType;
    public int GetPrice() => CoreServices.Get<DataManager>().GetBooster((int)GetBoosterType()).price;
    public virtual void Excute(System.Action onComplete = null){}
    public void AddBooster(int amount) => CoreServices.Get<DataManager>().AddBooster((int)GetBoosterType(), amount);
    public int GetNumsBooster() => CoreServices.Get<DataManager>().GetNumsBooster((int)GetBoosterType());
    public int GetUnlockLevel() => CoreServices.Get<DataManager>().GetBooster((int)GetBoosterType()).unlockedLevel;

}
