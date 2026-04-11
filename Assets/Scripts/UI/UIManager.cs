using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private EndGameUI endGameUI;
    [SerializeField] private MainMenuUIManager mainMenu;
    [SerializeField] private InGameUIManager ingame;
    [SerializeField] private SettingPopup setting;
    [SerializeField] private ShopPanel shop;

    // [SerializeField] private ShopPanel shopPanel;
    public Button settingButton;
    public Button continueButton;
    private GameManager gameManager;
    public void Setup(GameManager gameManager)
    {
        this.gameManager = gameManager;

        setting.Setup(this);
        shop.Setup(this);
        mainMenu.Setup(this);
        ingame.Setup(this);

    }

    // public void ChangeWin(int m)
    // {
        
    // }

    public void UpdateUI(GameManager.GameState gameState)
    {
        endGameUI.Hide();
        mainMenu.Hide();
        if(gameState == GameManager.GameState.MainMenu) ingame.Hide();
        switch(gameState)
        {
            case GameManager.GameState.MainMenu:
                mainMenu.Show();
                if(gameManager.GetCurrState() == GameManager.GameState.Win)
                {
                    int coinWin;
                    if(LevelLoader.Instance.gameDifficult == LevelLoader.GameDifficult.Easy) coinWin = 40;
                    else if(LevelLoader.Instance.gameDifficult == LevelLoader.GameDifficult.Hard) coinWin = 80;
                    else coinWin = 120;
                    mainMenu.AddCoin(coinWin);
                }
               
                break;
            case GameManager.GameState.Win:
                StartCoroutine(endGameUI.ShowLevelCompletedPanel());
                break;
            case GameManager.GameState.Lose:
                endGameUI.ShowLevelFailedPanel();
                break;
            case GameManager.GameState.Playing:
                ingame.Show();
                break;
            case GameManager.GameState.Pause:
                // setting.Show();
                break;

        }
    }
    public void OnClickBackHome()
    {
        gameManager.ChangeState(GameManager.GameState.MainMenu);
        setting.Hide();
    }
    public void OpenSetting()
    {
        setting.Show();
    }

    public void OpenShop(bool inMainMenu = false)
    {
        shop.Show();
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
        shop.Hide();
        
    }
    
}
