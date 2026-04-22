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
        GameEventBus.OnRequestUI += HandleUIRequest;
        GameEventBus.OnRequestClosePopup += PopPopup;
        GameEventBus.OnGameStateChanged += UpdateUI;
        GameEventBus.OnRequestAddBooster += OpenAddBooster;
        GameEventBus.OnClickAddBooster += OnClickAddBooster;
        GameEventBus.OnRequestBackHome += OnClickBackHome;
        GameEventBus.OnRequestTryAgain += OnClickTryAgain;
        GameEventBus.OnRequestAddMoveToContinue += OnClickAddMoveToContinue;
    }

    void OnDisable()
    {
        GameEventBus.OnRequestUI -= HandleUIRequest;
        GameEventBus.OnRequestClosePopup -= PopPopup;
        GameEventBus.OnGameStateChanged -= UpdateUI;
        GameEventBus.OnRequestAddBooster -= OpenAddBooster;
        GameEventBus.OnClickAddBooster -= OnClickAddBooster;
        GameEventBus.OnRequestBackHome -= OnClickBackHome;
        GameEventBus.OnRequestTryAgain -= OnClickTryAgain;
        GameEventBus.OnRequestAddMoveToContinue -= OnClickAddMoveToContinue;
    }

    private void HandleUIRequest(GameEventBus.UIType type)
    {
        switch (type)
        {
            case GameEventBus.UIType.Settings: OpenSetting(); break;
            case GameEventBus.UIType.Shop: OpenShop(false); break;
            case GameEventBus.UIType.ShopFromMainMenu: OpenShop(true); break;
            case GameEventBus.UIType.RefillHeart: mainMenu.OpenRefillHeart(); break;
            case GameEventBus.UIType.EndGameWin: endGameUI.ShowLevelCompletedPanel(); break;
            case GameEventBus.UIType.EndGameLose: endGameUI.ShowLevelFailedPanel(); break;
        }
    }

    public void UpdateUI(GameManager.GameState gameState)
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

    public void OpenSetting()
    {
        if(isCurrentlyInGame() && TutorialManager.Instance.IsTutorialActive()) return;
        PushPopupToFront(setting, setting.transform);
        if(gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Playing)
            gameManager.ChangeState(GameManager.GameState.Pause);
    }

    public void OpenShop(bool inMainMenu = false)
    {
        if(isCurrentlyInGame() && TutorialManager.Instance.IsTutorialActive()) return;
        PushPopupToFront(shop, shop.transform);
        if(inMainMenu) shop.HideCloseButton();
        else
        {
            shop.ShowCloseButton();
            if(gameManager != null) gameManager.ChangeState(GameManager.GameState.Pause);
        }
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
            popupStack.Pop().Hide();
            if(gameManager.GetCurrState() == GameManager.GameState.Pause) gameManager.ChangeState(GameManager.GameState.Playing);
            
        } 
        if(popupStack.Count > 0) popupStack.Peek().Show();
    }

    private void ClearPopupStack()
    {
        foreach(IMenu popup in popupStack) popup.Hide();
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
        gameManager.UseHeart();
        if(dataManager.GetHearts() > 0) gameManager.ChangeState(GameManager.GameState.Playing);
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

    public void OpenAddBooster(AddBoosterUI addBoosterUI, BoosterButton caller, string header, string coins, Constants.BoosterType type, bool isFirstTime)
    {
        addBoosterUI.SetConfig(caller, header, coins, type, isFirstTime);
        PushPopupToFront(addBoosterUI, addBoosterUI.transform);
        if(gameManager != null && gameManager.GetCurrState() == GameManager.GameState.Playing)
            gameManager.ChangeState(GameManager.GameState.Pause);
    }

    public void OnClickAddBooster(BoosterButton boosterButton)
    {
        if(boosterButton.GetBooster().GetPrice() > dataManager.GetTotalCoins()) OpenShop();
        else { PopPopup(); boosterButton.PlayAddEffect(); boosterButton.GetBooster().AddBooster(3); }
    }
}
