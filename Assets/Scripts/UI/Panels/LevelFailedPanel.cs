using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedPanel : Panel
{
    // private UIManager uIManager; // Loại bỏ phụ thuộc
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button addMoveButton;
    [SerializeField] private Button backMainMenuButton;
    [SerializeField] private Transform blockLeft, blockRight, titleText;

    void OnEnable()
    {
        backMainMenuButton.onClick.AddListener(OnClickBackHome);
        tryAgainButton.onClick.AddListener(OnClickTryAgain);
        addMoveButton.onClick.AddListener(OnClickAddMove);
    }

    void OnDisable()
    {
        backMainMenuButton.onClick.RemoveAllListeners();
        tryAgainButton.onClick.RemoveAllListeners();
        addMoveButton.onClick.RemoveAllListeners();
    }

    public override void Show()
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
    private void OnClickTryAgain()
    {
        CoreServices.Get<GameManager>().RestartLevel();
    }
    
    private void OnClickAddMove()
    {
        CoreServices.Get<GameManager>().AddMoveToContinue(5);
    }
    public override void Hide() => gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
