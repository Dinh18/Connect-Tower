using UnityEngine;
using UnityEngine.UI;

public class LevelFailedUI : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button backMainMenuButton;
    void OnEnable()
    {
        backMainMenuButton.onClick.AddListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.AddListener(OnClickTryAgain);
    }
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

    private void OnClickTryAgain()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }
}
