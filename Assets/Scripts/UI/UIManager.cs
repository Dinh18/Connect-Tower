using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private MainMenuUIManager mainMenu;
    [SerializeField] private InGameUIManager ingame;
    [SerializeField] private EndGameUI endGameUI;
    [SerializeField] private SettingPopup setting;
    [SerializeField] private ShopPanel shop;
    private Stack<IMenu> popupStack = new Stack<IMenu>();
    // [SerializeField] private ShopPanel shopPanel;
    public Button settingButton;
    public Button continueButton;
    private GameManager gameManager;
    
    public void Setup(GameManager gameManager)
    {
        this.gameManager = gameManager;

        mainMenu.Setup(this);
        setting.Setup(this);
        shop.Setup(this);
        ingame.Setup(this);
        endGameUI.Setup(this);

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
                if(gameManager.GetPrevState() == GameManager.GameState.Win)
                {
                    int coinWin = LevelLoader.Instance.GetCurrentLevelReward();
                    mainMenu.AddCoin(coinWin);
                }
               
                break;
            case GameManager.GameState.Win:
                endGameUI.ShowLevelCompletedPanel();
                break;
            case GameManager.GameState.Lose:
                endGameUI.ShowLevelFailedPanel();
                break;
            case GameManager.GameState.Playing:
                if(gameManager.GetPrevState() != GameManager.GameState.Pause)
                {
                    ClearPopupStack();
                    ingame.Show();
                }
                break;
            case GameManager.GameState.Pause:
                // setting.Show();
                break;

        }
    }
    public void OnClickBackHome()
    {
        ClearPopupStack();
        gameManager.ChangeState(GameManager.GameState.MainMenu);
    }
    public void OpenSetting()
    {
        // bool isIngame =(gameManager.GetCurrState() == GameManager.GameState.Playing || 
        //              gameManager.GetCurrState() == GameManager.GameState.Pause);

        // if(isCurrentlyInGame() && TutorialManager.Instance.IsTutorialActive()) return;

        PushPopupToFront(setting);

        if(gameManager.GetCurrState() == GameManager.GameState.Playing)
        {
            gameManager.ChangeState(GameManager.GameState.Pause);
        }
    }

    public void CloseSetting()
    {
        PopPopup();
        if(popupStack.Count == 0 && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public void OpenShop(bool inMainMenu = false)
    {
        PushPopupToFront(shop);
        if(inMainMenu)
        {
            shop.HideCloseButton();
        }
        else
        {
            shop.ShowCloseButton();
            GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        }
    }

    public void CloseShop()
    {
        PopPopup();
        if(popupStack.Count == 0 && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public void OpenAddBooster(AddBoosterUI addBoosterUI, BoosterButton caller, string header, string coins, Constants.BoosterType type, bool isFirstTime)
    {
        // if(isFirstTime)
        // {
        //     buyButton.gameObject.SetActive(false);
        //     claimButton.gameObject.SetActive(true);
        // }
        // else
        // {
        //     buyButton.gameObject.SetActive(true);
        //     claimButton.gameObject.SetActive(false);
        // }
        addBoosterUI.SetConfig(caller,header, coins, type, isFirstTime);
        PushPopupToFront(addBoosterUI);
        if(gameManager.GetCurrState() == GameManager.GameState.Playing)
        {
            gameManager.ChangeState(GameManager.GameState.Pause);
        }
    }

    public void CloseAddBooster()
    {
        PopPopup();
        if(popupStack.Count <= 0 && gameManager.GetCurrState() == GameManager.GameState.Pause)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
    }

    public void OnClickAddBooster(BoosterButton boosterButton)
    {
        if(boosterButton.GetBooster().GetPrice() > DataManager.Instance.playerData.totalCoins)
        {
            OpenShop();
        }
        else
        {
            CloseAddBooster();
            boosterButton.PlayAddEffect();
            boosterButton.GetBooster().AddBooster(3);
        }
    }

    public void PushPopupToFront(IMenu popup)
    {
        if(popupStack.Count > 0 && popupStack.Peek() == popup) return;
        if(popupStack.Count > 0)
        {
            IMenu topPopup = popupStack.Peek();
            topPopup.Hide();
        }

        popup.Show();
        popupStack.Push(popup);
    }

    public void PopPopup()
    {
        if(popupStack.Count > 0)
        {
            IMenu currPopup = popupStack.Pop();
            currPopup.Hide();
        }

        if(popupStack.Count > 0)
        {
            IMenu prevPopup = popupStack.Peek();
            prevPopup.Show();
        }
    }

    private void ClearPopupStack()
    {
        foreach(IMenu popup in popupStack)
        {
            popup.Hide();
        }
        popupStack.Clear();
    }

    public bool isCurrentlyInGame()
    {
        return (gameManager.GetCurrState() == GameManager.GameState.Playing || 
                gameManager.GetCurrState() == GameManager.GameState.Pause);
    }

    
    
}
