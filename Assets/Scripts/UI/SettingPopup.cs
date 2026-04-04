using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backHomeButton;
    void Awake()
    {
        backHomeButton.onClick.AddListener(uIManager.OnClickBackHome);
        closeButton.onClick.AddListener(Hide);
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
}
