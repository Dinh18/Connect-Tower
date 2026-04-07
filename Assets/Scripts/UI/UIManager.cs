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

        mainMenu.Setup(this);
        ingame.Setup(this);

        setting.Setup(this);
        shop.Setup(this);
    }

    public void UpdateUI(GameManager.GameState gameState)
    {
        endGameUI.Hide();
        mainMenu.Hide();
        if(gameState == GameManager.GameState.MainMenu) ingame.Hide();
        switch(gameState)
        {
            case GameManager.GameState.MainMenu:
                if(gameManager.GetCurrState() == GameManager.GameState.Win)
                {
                    mainMenu.AddCoin();
                }
                mainMenu.Show();
                break;
            case GameManager.GameState.Win:
                // ingame.Show();
                endGameUI.ShowLevelCompletedPanel();
                break;
            case GameManager.GameState.Lose:
                // ingame.Show();
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

    public void OpenShop()
    {
        shop.Show();
    }

    public void CloseShop()
    {
        shop.Hide();
    }
    
}
