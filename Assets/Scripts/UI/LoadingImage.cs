using UnityEngine;

public class LoadingImage : Popup
{
    public override void Hide()
    {
        this.gameObject.SetActive(false);
        GameEventBus.Publish(new LoadingFinished());
    }

    public override void Show()
    {
        this.gameObject.SetActive(true);
    }
}
