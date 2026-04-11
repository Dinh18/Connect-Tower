using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    void OnEnable()
    {
        closeButton.onClick.AddListener(Hide);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(Hide);
    }
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

    public void ShowCloseButton()
    {
        closeButton.gameObject.SetActive(true);
    }
    public void HideCloseButton()
    {
        closeButton.gameObject.SetActive(false);
        
    }

}
