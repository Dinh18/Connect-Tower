using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private LoadingImage loadingImage;
    [SerializeField] private RefillHeartPopup refillHeartPopup;

    private Stack<IMenu> popupStack = new Stack<IMenu>();
    // [SerializeField] private ShopPanel shopPanel;
    private GameManager gameManager;
    
    public void Setup(GameManager gameManager)
    {
        this.gameManager = gameManager;

        mainMenu.Setup(this);
        setting.Setup(this);
        shop.Setup(this);
        ingame.Setup(this);
        endGameUI.Setup(this);
        loadingImage.Setup(this);

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
                if(gameManager.GetPrevState() == GameManager.GameState.None)
                {
                    StartCoroutine(ShowLoadingImage(3f));
                }
               
                break;
            case GameManager.GameState.Win:
                endGameUI.ShowLevelCompletedPanel();
                // StartCoroutine(DelayShowLevelCompletedPanel());
                break;
            case GameManager.GameState.Lose:
                endGameUI.ShowLevelFailedPanel();
                break;
            case GameManager.GameState.Playing:
                if(gameManager.GetPrevState() != GameManager.GameState.Pause && gameManager.GetPrevState() != GameManager.GameState.Lose)
                {
                    ClearPopupStack();
                    ingame.Show();
                    StartCoroutine(ShowLoadingImage(1f));
                }
                break;
            case GameManager.GameState.Pause:
                // setting.Show();
                break;
            case GameManager.GameState.Resume:
                break;

        }
    }
    public IEnumerator DelayShowLevelCompletedPanel()
    {
        yield return new WaitForSeconds(0.5f);
        endGameUI.ShowLevelCompletedPanel();
    }
    public void OnClickBackHome()
    {
        ClearPopupStack();
        gameManager.ChangeState(GameManager.GameState.MainMenu);
        if(gameManager.GetPrevState() == GameManager.GameState.Pause)
        {
            if(gameManager.GetMaxMoves() > gameManager.GetMoves())
            {
                gameManager.UseHeart();
            }
        }
        if(gameManager.GetPrevState() == GameManager.GameState.Lose)
        {
            gameManager.UseHeart();
        }
    }
    public void OnClickAddMoveToContinue()
    {
        int addMoveCost = 900;
        GameConfigSO config = Resources.Load<GameConfigSO>("GameConfig");
        if (config != null)
        {
            addMoveCost = config.addMoveCost;
        }

        if(DataManager.Instance.playerData.totalCoins >= addMoveCost)
        {
            endGameUI.Hide();
            gameManager.AddMove(5);
            gameManager.ChangeState(GameManager.GameState.Pause);
            gameManager.ChangeState(GameManager.GameState.Playing);
            DataManager.Instance.UseCoins(addMoveCost);
        }
        else
        {
            OpenShop();
        }   
    }
    public void OnClickTryAgain()
    {
        gameManager.UseHeart();
        Debug.Log("heart " + DataManager.Instance.playerData.heart);
        if(DataManager.Instance.playerData.heart > 0)
        {    
            gameManager.ChangeState(GameManager.GameState.Playing);
        }
        else
        {
            ClearPopupStack();
            gameManager.ChangeState(GameManager.GameState.MainMenu);
            mainMenu.OpenRefillHeart();
        }
    }
    public void OpenSetting()
    {
        // bool isIngame =(gameManager.GetCurrState() == GameManager.GameState.Playing || 
        //              gameManager.GetCurrState() == GameManager.GameState.Pause);

        if(isCurrentlyInGame() && TutorialManager.Instance.IsTutorialActive()) return;

        PushPopupToFront(setting, setting.transform);

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
        if(isCurrentlyInGame() && TutorialManager.Instance.IsTutorialActive()) return;
        PushPopupToFront(shop, shop.transform);
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
        if(gameManager.GetCurrState() == GameManager.GameState.MainMenu) return;
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
        PushPopupToFront(addBoosterUI, addBoosterUI.transform);
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

    public IEnumerator ShowLoadingImage(float time)
    {
        PushPopupToFront(loadingImage, loadingImage.transform, false);

        yield return new WaitForSeconds(time);

        PopPopup();
    }

    public void PushPopupToFront(IMenu popup, Transform goPopup, bool playAnim = true)
    {
        if(popupStack.Count > 0 && popupStack.Peek() == popup) return;
        // if(TutorialManager.Instance.IsTutorialActive()) return;
        if(popupStack.Count > 0)
        {
            IMenu topPopup = popupStack.Peek();
            topPopup.Hide();
        }

        popup.Show();
        popupStack.Push(popup);
        if(playAnim)
        {
            goPopup.localScale = Vector3.zero;
            goPopup.DOScale(Vector3.one, 0.3f)
                   .SetEase(Ease.OutBack)
                   .SetUpdate(true);
        }
    }

    public void PopPopup()
    {
        if(popupStack.Count > 0)
        {
            IMenu currPopup = popupStack.Pop();
            // goPopup.DOScale(Vector3.zero, 0.3f);
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
