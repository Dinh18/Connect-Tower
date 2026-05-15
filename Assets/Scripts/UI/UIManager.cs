using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private MainMenuUIPanel mainMenu;
    [SerializeField] private InGamePanel ingame;
    [SerializeField] private EndGamePanel endGameUI;
    [SerializeField] private ShopPanel shop;
    [SerializeField] private LoadingImage loadingImage;
    [SerializeField] private List<Popup> allPopups;
    
    private Stack<Popup> popupStack = new Stack<Popup>();
    private GameManager gameManager;
    private DataManager dataManager;

    public void Init(GameManager gameM, DataManager dataM)
    {
        this.gameManager = gameM;
        this.dataManager = dataM;

        CoreServices.Register<UIManager>(this);

        // Setup sub-panels
        mainMenu.Setup(this);
        shop.Setup(this);
        ingame.Setup(this);
        endGameUI.Setup(this);
        loadingImage.Setup(this);
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<RequestOpenPanelEvent>(HandleOpenPanelRequest);
        GameEventBus.Subscribe<GameStateChangedEvent>(UpdateUI);
        GameEventBus.Subscribe<RequestOpenPopupEvent>(HandleOpenPopupRequest);
        GameEventBus.Subscribe<RequestClosePopupEvent>(HandleClosePopupRequest);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestOpenPanelEvent>(HandleOpenPanelRequest);
        GameEventBus.UnSubscribe<GameStateChangedEvent>(UpdateUI);
        GameEventBus.UnSubscribe<RequestOpenPopupEvent>(HandleOpenPopupRequest);
        GameEventBus.UnSubscribe<RequestClosePopupEvent>(HandleClosePopupRequest);

    }
    
    #region UI Tổng
    public void UpdateUI(GameStateChangedEvent gameStateChangedEvent)
    {
        GameManager.GameState gameState = gameStateChangedEvent.newState;
        {
            endGameUI.Hide();
            mainMenu.Hide();
            if(gameState == GameManager.GameState.MainMenu) ingame.Hide();
            
            switch(gameState)
            {
                case GameManager.GameState.MainMenu:
                    mainMenu.Show();
                    if(gameManager != null && gameManager.GetPrevState() == GameManager.GameState.Win)
                        mainMenu.AddCoin(CoreServices.Get<LevelLoader>().GetCurrentLevelReward());
                    
                    if(gameManager != null && gameManager.GetPrevState() == GameManager.GameState.None)
                        StartCoroutine(ShowLoadingImage(3f));
                    break;
                case GameManager.GameState.Win: endGameUI.ShowLevelCompletedPanel(); break;
                case GameManager.GameState.Lose: endGameUI.ShowLevelFailedPanel(); break;
                case GameManager.GameState.Playing:
                    if(gameManager != null && gameManager.GetPrevState() != GameManager.GameState.Pause && gameManager.GetPrevState() != GameManager.GameState.Lose)
                    {
                        ClearPopupStack();
                        ingame.Show();
                        StartCoroutine(ShowLoadingImage(1f));
                    }
                    break;
            }
        }
    }
    #endregion

    #region Xử lý popup

    public Popup GetPopup(PopupType popupType)
    {
        foreach(var popup in allPopups)
        {
            if(popup.popupType == popupType) return popup;
        }
        return null;
    }

    private void HandleOpenPopupRequest(RequestOpenPopupEvent requestOpenPopup)
    {
        Popup popup = GetPopup(requestOpenPopup.targetPopup);
        if(popup != null)
        {
            PushPopupToFront(popup, popup.gameObject.transform);
            
            if(requestOpenPopup.targetPopup == PopupType.Setting || requestOpenPopup.targetPopup == PopupType.QuitLevel)
            {
                if(gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Playing)
                    gameManager.ChangeState(GameManager.GameState.Pause);
            }
        }
    }
    private void HandleClosePopupRequest(RequestClosePopupEvent requestClosePopup)
    {
        PopPopup();
    }

    public void OpenPopup(PopupType popupType)
    {
        Popup popup = GetPopup(popupType);
        if (popup != null)
        {
            PushPopupToFront(popup, popup.gameObject.transform);
        }
    }

    public void PushPopupToFront(Popup popup, Transform goPopup, bool playAnim = true)
    {
        if(popupStack.Count > 0 && popupStack.Peek() == popup) return;
        if(popupStack.Count > 0) popupStack.Peek().Hide();

        popup.Show();
        popupStack.Push(popup);
        if(playAnim)
        {
            goPopup.localScale = Vector3.zero;
            goPopup.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void PopPopup()
    {
        if(popupStack.Count > 0)
        {
            Popup popup = popupStack.Pop();
            GameObject goPopup = popup.gameObject;
            goPopup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                popup.Hide();
                if(popupStack.Count == 0)
                {
                    if(gameManager.GetCurrState() == GameManager.GameState.Pause) gameManager.ChangeState(GameManager.GameState.Playing);
                }
                else
                {
                    if(popupStack.Peek().dimImage != null)
                    {
                        popupStack.Peek().dimImage.SetActive(true);
                    }
                }
            });
            
        } 
        if(popupStack.Count > 0)
        {
            Popup popup = popupStack.Peek();
            GameObject goPopup = popup.gameObject;
            goPopup.transform.localScale = Vector3.zero;
            popup.Show();
            goPopup.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        } 
    }

    private void ClearPopupStack()
    {
        while(popupStack.Count > 0)
        {
            PopPopup();
        }
        popupStack.Clear();
    }
    #endregion

    #region Xử lý Panel
    private void HandleOpenPanelRequest(RequestOpenPanelEvent requestOpenPanelEvent)
    {
        switch (requestOpenPanelEvent.targetPanel)
        {
            case PanelType.Shop: OpenShop(!isCurrentlyInGame()); break;
            case PanelType.EndGameWin: endGameUI.ShowLevelCompletedPanel(); break;
            case PanelType.EndGameLose: endGameUI.ShowLevelFailedPanel(); break;
        }
    }

    public void OpenShop(bool inMainMenu = false)
    {
        shop.Show();
        if(inMainMenu) shop.HideCloseButton();
        else
        {
            shop.ShowCloseButton();
            if(gameManager != null) gameManager.ChangeState(GameManager.GameState.Pause);
        }
    }
    public void CloseShop()
    {
        shop.Hide();
        if(gameManager == null || gameManager.GetCurrState() == GameManager.GameState.MainMenu) return;
        if(gameManager.GetPrevState() == GameManager.GameState.Lose)
        {
            gameManager.ChangeState(GameManager.GameState.Lose);
            return;
        }
        if(popupStack.Count == 0 && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public bool isCurrentlyInGame()
    {
        if (gameManager == null) gameManager = CoreServices.Get<GameManager>();
        return (gameManager != null && (gameManager.GetCurrState() == GameManager.GameState.Playing || 
                gameManager.GetCurrState() == GameManager.GameState.Pause));
    }

    public void OnClickBackHome()
    {
        ClearPopupStack();
        gameManager.ChangeState(GameManager.GameState.MainMenu);
    }

    public void OnClickTryAgain()
    {
        CoreServices.Get<HeartManager>().UseHeart();
        if(dataManager.GetHearts() > 0)
        {
            
            CoreServices.Get<GameManager>().RestartLevel();
            ClearPopupStack();
        } 
        else { ClearPopupStack(); gameManager.ChangeState(GameManager.GameState.MainMenu); mainMenu.OpenRefillHeart(); }
    }

    public void OnClickAddMoveToContinue()
    {
        int cost = Resources.Load<GameConfigSO>("GameConfig")?.addMoveCost ?? 900;
        if(dataManager.GetTotalCoins() >= cost)
        {
            endGameUI.Hide();
            gameManager.AddMove(5);
            gameManager.ChangeState(GameManager.GameState.Playing);
            dataManager.UseCoins(cost);
        }
        else OpenShop();
    }

    public IEnumerator ShowLoadingImage(float time)
    {
        PushPopupToFront(loadingImage, loadingImage.transform, false);
        yield return new WaitForSeconds(time);
        PopPopup();
    }
    #endregion
}
