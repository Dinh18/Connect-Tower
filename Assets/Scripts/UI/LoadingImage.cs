using UnityEngine;

public class LoadingImage : Popup
{
    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public override void Show()
    {
        this.gameObject.SetActive(true);
    }
}
