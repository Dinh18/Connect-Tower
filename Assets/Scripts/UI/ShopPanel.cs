using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text coinCountText;
    void OnEnable()
    {
        closeButton.onClick.AddListener(uIManager.CloseShop);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(uIManager.CloseShop);
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
        coinCountText.text = DataManager.Instance.playerData.totalCoins.ToString();
    }

    public void ShowCloseButton()
    {
        closeButton.gameObject.SetActive(true);
    }
    public void HideCloseButton()
    {
        closeButton.gameObject.SetActive(false);
        
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
