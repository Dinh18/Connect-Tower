using System.Collections;
using UnityEngine;

public class AddMoveBooster : Booster
{
    // [SerializeField] private int amount;
    public override BoosterType GetBoosterType() => BoosterType.AddMove;
    public override void Excute(System.Action onComplete = null)
    {
        CoreServices.Get<DataManager>().UseBooster((int)BoosterType.AddMove);
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.AddMove});
        StartCoroutine(StartInfiniteMovesCoroutine(10f));
        onComplete?.Invoke();
    }

    private IEnumerator StartInfiniteMovesCoroutine(float time)
    {
        GameEventBus.Publish(new StartBorderFlashEvent { borderType = BorderType.Ice, flashSpeed = 1f, flashTime = time });
        yield return new WaitForSeconds(time);
        GameEventBus.Publish(new StopBorderFlashEvent());
    }
}
