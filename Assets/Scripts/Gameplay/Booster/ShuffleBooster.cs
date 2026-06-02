using UnityEngine;

public class ShuffleBooster : Booster
{
    public override BoosterType GetBoosterType() => BoosterType.Shuffle;
    public override void Excute(System.Action onComplete = null)
    {
        GameEventBus.Publish(new RequestExecuteBoosterEvent 
        { 
            boosterType = BoosterType.Shuffle,
            onComplete = (success) => 
            {
                if (success)
                {
                    CoreServices.Get<DataManager>().UseBooster((int)BoosterType.Shuffle);
                    GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.Shuffle});
                }
                onComplete?.Invoke();
            }
        });
    }
}
