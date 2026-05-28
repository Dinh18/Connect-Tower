using UnityEngine;

public class HintBooster : Booster
{
    [SerializeField] private FloatingNotifier floatingNotifier;
    public override BoosterType GetBoosterType() => BoosterType.Hint;

    public override void Excute()
    {
        GameEventBus.Publish(new RequestExecuteBoosterEvent 
        { 
            boosterType = BoosterType.Hint,
            onComplete = (success) => 
            {
                if(success)
                {
                    CoreServices.Get<DataManager>().UseBooster((int)BoosterType.Hint);
                    GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.HintBooster});
                    Debug.Log("Thuc hien Hint thanh cong");
                } 
                else 
                {
                    floatingNotifier.ShowWarning("All blocks have been revealed!");
                }
            }
        });
    }

    
}
