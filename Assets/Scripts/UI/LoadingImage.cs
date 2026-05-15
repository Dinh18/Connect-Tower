using UnityEngine;

public class LoadingImage : Popup
{
    private UIManager uIManager;
    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public override void Show()
    {
        this.gameObject.SetActive(true);
    }
}
