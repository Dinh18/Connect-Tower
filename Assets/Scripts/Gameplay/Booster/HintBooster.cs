using UnityEngine;

public class HintBooster : MonoBehaviour, IBooster
{
    private BoosterManager boosterManager;
    public void AddBooster(int amount)
    {
        DataManager.Instance.AddBooster((int) Constants.BoosterType.Hint,amount);
    }

    public void Excute()
    {
        if(DataManager.Instance.GetAmountOfBoosterByID((int)Constants.BoosterType.Hint) <= 0) return;
        DataManager.Instance.UseBooster((int)Constants.BoosterType.Hint);
        bool searched = boosterManager.SearchedBlocks();
        if(searched)
        {
            AudioManager.Instance.PlayHintBoosterAudio();
            Debug.Log("Thuc hien Hint thanh cong");
        } 
        else Debug.Log("Hint khong thanh cong");
    }

    public void Setup(BoosterManager boosterManager)
    {
        this.boosterManager = boosterManager;
    }
}
