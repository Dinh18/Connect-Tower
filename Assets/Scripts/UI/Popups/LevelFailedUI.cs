using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedUI : MonoBehaviour, IMenu
{
    // private UIManager uIManager; // Loại bỏ phụ thuộc
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button addMoveButton;
    [SerializeField] private Button backMainMenuButton;
    [SerializeField] private Transform blockLeft, blockRight, titleText;

    void OnEnable()
    {
        // backMainMenuButton.onClick.AddListener(() => GameEventBus.OnRequestBackHome?.Invoke());
        // tryAgainButton.onClick.AddListener(() => GameEventBus.OnRequestTryAgain?.Invoke());
        // addMoveButton.onClick.AddListener(() => GameEventBus.OnRequestAddMoveToContinue?.Invoke());
        backMainMenuButton.onClick.AddListener(OnClickBackHome);
        tryAgainButton.onClick.AddListener(() => CoreServices.Get<GameManager>().RestartLevel());
        addMoveButton.onClick.AddListener(() => CoreServices.Get<GameManager>().AddMoveToContinue(5));
    }

    void OnDisable()
    {
        backMainMenuButton.onClick.RemoveAllListeners();
        tryAgainButton.onClick.RemoveAllListeners();
        addMoveButton.onClick.RemoveAllListeners();
    }

    public void Setup(UIManager uIManager)
    {
        // Vẫn giữ để tương thích
    }

    public void Show()
    {
        gameObject.SetActive(true);
        blockLeft.localScale = Vector3.zero;
        blockRight.localScale = Vector3.zero;
        titleText.localScale = Vector3.zero;
        blockLeft.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        blockRight.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        titleText.DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }

    private void OnClickBackHome()
    {
        CoreServices.Get<GameManager>().UseHeart();
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }

    public void Hide() => gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
