using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedUI : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button addMoveButton;
    [SerializeField] private Button backMainMenuButton;
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
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
