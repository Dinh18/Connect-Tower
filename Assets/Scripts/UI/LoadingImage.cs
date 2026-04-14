using UnityEngine;

public class LoadingImage : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

}
