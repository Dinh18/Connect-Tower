using UnityEngine;

public class LevelFailedUI : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
