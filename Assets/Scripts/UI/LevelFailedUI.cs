using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedUI : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button addMoveButton;
    [SerializeField] private Button backMainMenuButton;
    [SerializeField] private Transform blockLeft, blockRight, titleText;
    // public static event Action<bool> OnLoadLevel;
    void OnEnable()
    {
        backMainMenuButton.onClick.AddListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.AddListener(uIManager.OnClickTryAgain);
        addMoveButton.onClick.AddListener(uIManager.OnClickAddMoveToContinue);
    }
    void OnDisable()
    {
        backMainMenuButton.onClick.RemoveListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.RemoveListener(uIManager.OnClickTryAgain);
        addMoveButton.onClick.RemoveListener(uIManager.OnClickAddMoveToContinue);
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
        blockLeft.localScale = Vector3.zero;
        blockRight.localScale = Vector3.zero;
        titleText.localScale = Vector3.zero;
        blockLeft.DOScale(1,0.5f).SetEase(Ease.OutBack);
        blockRight.DOScale(1,0.5f).SetEase(Ease.OutBack);
        titleText.DOScale(1,0.5f).SetEase(Ease.OutBack);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
