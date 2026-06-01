using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedPanel : Panel
{
    // private UIManager uIManager; // Loại bỏ phụ thuộc
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button backMainMenuButton;

    void OnEnable()
    {
        backMainMenuButton.onClick.AddListener(OnClickBackHome);
        tryAgainButton.onClick.AddListener(OnClickTryAgain);
    }

    void OnDisable()
    {
        backMainMenuButton.onClick.RemoveAllListeners();
        tryAgainButton.onClick.RemoveAllListeners();
    }

    public override void Show()
    {
        gameObject.SetActive(true);
        if (transform.childCount > 1)
        {
            Transform panelHolder = transform.GetChild(1);
            panelHolder.DOKill();
            panelHolder.localScale = Vector3.zero;
            panelHolder.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
            
            Transform background = transform.GetChild(0);
            if (background.TryGetComponent<Image>(out Image bgImage))
            {
                bgImage.DOKill();
                Color c = bgImage.color;
                c.a = 0;
                bgImage.color = c;
                bgImage.DOFade(0.86f, 0.3f).SetUpdate(true);
            }
        }
        else
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }
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
    
    public override void Hide() 
    {
        if (transform.childCount > 1)
        {
            Transform panelHolder = transform.GetChild(1);
            panelHolder.DOKill();
            panelHolder.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
            
            Transform background = transform.GetChild(0);
            if (background.TryGetComponent<Image>(out Image bgImage))
            {
                bgImage.DOKill();
                bgImage.DOFade(0f, 0.3f).SetUpdate(true);
            }
        }
        else
        {
            transform.DOKill();
            transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
        }
    }
    public GameObject GetGameObject() => this.gameObject;
}
