using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private MainMenuUIManager mainMenu;
    [SerializeField] private InGameUIManager ingame;
    [SerializeField] private EndGameUI endGameUI;
    [SerializeField] private SettingPopup setting;
    [SerializeField] private ShopPanel shop;
    [SerializeField] private LoadingImage loadingImage;
    [SerializeField] private RefillHeartPopup refillHeartPopup;
    [SerializeField] private QuitLevelPopup quitLevelPopup;
    
    private Stack<IMenu> popupStack = new Stack<IMenu>();
    private GameManager gameManager;
    private DataManager dataManager;

    public void Init(GameManager gameM, DataManager dataM)
    {
        this.gameManager = gameM;
        this.dataManager = dataM;

        CoreServices.Register<UIManager>(this);

        // Setup sub-panels
        mainMenu.Setup(this);
        setting.Setup(this);
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

    private void HandleOpenPanelRequest(RequestOpenPanelEvent requestOpenPanelEvent)
    {
        switch (requestOpenPanelEvent.targetPanel)
        {
            case PanelType.Shop: OpenShop(false); break;
            case PanelType.ShopFromMainMenu: OpenShop(true); break;
            case PanelType.EndGameWin: endGameUI.ShowLevelCompletedPanel(); break;
            case PanelType.EndGameLose: endGameUI.ShowLevelFailedPanel(); break;
        }
    }

    private void HandleOpenPopupRequest(RequestOpenPopupEvent requestOpenPopup)
    {
        switch(requestOpenPopup.targetPopup)
        {
            case PopupType.RefillHeart: mainMenu.OpenRefillHeart(); break;
            case PopupType.Setting: OpenSetting(); break;
            case PopupType.QuitLevel: OpenQuitLevelPopup(); break;
        }
    }

    private void HandleClosePopupRequest(RequestClosePopupEvent requestClosePopup)
    {
        PopPopup();
    }


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
                        // GameEventBus.Publish(new LoadingFinished{});
                    }
                    break;
            }
        }
    }


    public void OpenSetting()
    {
        PushPopupToFront(setting, setting.transform);
        if(gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Playing)
            gameManager.ChangeState(GameManager.GameState.Pause);
    }

    public void OpenShop(bool inMainMenu = false)
    {
        PushPopupToFront(shop, shop.transform);
        if(inMainMenu) shop.HideCloseButton();
        else
        {
            shop.ShowCloseButton();
            if(gameManager != null) gameManager.ChangeState(GameManager.GameState.Pause);
        }
    }

    public void OpenQuitLevelPopup()
    {
        PushPopupToFront(quitLevelPopup, quitLevelPopup.transform);
        gameManager.ChangeState(GameManager.GameState.Pause);
    }

    public void CloseSetting()
    {
        PopPopup();
        if(popupStack.Count == 0 && gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public void CloseShop()
    {
        PopPopup();
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

    public void CloseAddBooster()
    {
        PopPopup();
        if(popupStack.Count <= 0 && gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public void PushPopupToFront(IMenu popup, Transform goPopup, bool playAnim = true)
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
            IMenu popup = popupStack.Pop();
            GameObject goPopup = popup.GetGameObject();
            goPopup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                popup.Hide();
                if(gameManager.GetCurrState() == GameManager.GameState.Pause) gameManager.ChangeState(GameManager.GameState.Playing);
            });
            
        } 
        if(popupStack.Count > 0)
        {
            IMenu popup = popupStack.Peek();
            GameObject goPopup = popup.GetGameObject();
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

    // public void OpenAddBooster(RequestOpenBoosterPopupEvent requestOpenBoosterPopupEvent)
    // {
    //     PushPopupToFront(addBoosterUI, addBoosterUI.transform);
    //     if(gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Playing)
    //         gameManager.ChangeState(GameManager.GameState.Pause);
    // }
}
