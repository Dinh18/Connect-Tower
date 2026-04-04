using UnityEngine;

public class ShopPanel : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

}
