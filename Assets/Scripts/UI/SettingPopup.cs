using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backHomeButton;
    void OnEnable()
    {
        backHomeButton.onClick.AddListener(uIManager.OnClickBackHome);
        closeButton.onClick.AddListener(OnClickClose);
    }
    void OnDisable()
    {
        backHomeButton.onClick.RemoveListener(uIManager.OnClickBackHome);
        closeButton.onClick.RemoveListener(Hide);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);

    }

    private void OnClickClose()
    {
        Hide();
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
    }
}
