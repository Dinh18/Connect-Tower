using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;  
using UnityEngine;


public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject sharedDimImage;
    [SerializeField] private List<Menu> allMenu;
    [SerializeField] private List<UIView> allPopups;
    [SerializeField] private Dictionary<Type,UIView> uiDictionary = new Dictionary<Type,UIView>();
    private Stack<UIView> uiStack = new Stack<UIView>();
    private GameManager gameManager;
    public void Init(GameManager gameM)
    {
        this.gameManager = gameM;

        CoreServices.Register(this);
        
        foreach (var popup in allPopups)
        {
            RegisterUI(popup);
        }
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<GameStateChangedEvent>(UpdateUI);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<GameStateChangedEvent>(UpdateUI);
    }

    private void RegisterUI(UIView ui)
    {
        if (ui != null)
        {
            // Tự động nhận diện class (Ví dụ: ShopPanel) và đưa vào từ điển
            Type type = ui.GetType(); 
            if (!uiDictionary.ContainsKey(type))
            {
                uiDictionary.Add(type, ui);
            }
        }
    }

    public T GetUI<T>() where T : UIView
    {
        Type uiType = typeof(T);
        if (uiDictionary.ContainsKey(uiType))
        {
            return uiDictionary[uiType] as T;
        }

        return null;
    }
    
    #region Xử lý Menu
    public void UpdateUI(GameStateChangedEvent gameStateChangedEvent)
    {
        GameManager.GameState gameState = gameStateChangedEvent.newState;
        {
            // endGameUI.Hide();
            // mainMenu.Hide();
            // if(gameState == GameManager.GameState.MainMenu) ingame.Hide();

            foreach(Menu menu in allMenu)
            {
                if (menu is InGameMenu && (gameState == GameManager.GameState.Lose || gameState == GameManager.GameState.Pause || gameState == GameManager.GameState.Playing))
                {
                    continue;
                }
                menu.Hide();
            }

            ClearUIStack();

            if(sharedDimImage != null) sharedDimImage.SetActive(false);
            
            switch(gameState)
            {
                case GameManager.GameState.MainMenu:
                    // mainMenu.Show();
                    ShowMenu<MainMenu>();
                    GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Idle});
                    if(CoreServices.Get<DataManager>().GetHearts() <= 0)
                    {
                        Debug.Log("Het tim");
                        GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Sleep});
                    }
                    else if(gameManager != null && gameManager.GetPrevState() == GameManager.GameState.Win)
                    {
                        GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Excited});
                    }
                    else if(gameManager != null && gameManager.GetPrevState() == GameManager.GameState.Lose)
                    {
                        GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Crying});
                    }
                    
                    if(gameManager != null && gameManager.GetPrevState() == GameManager.GameState.None)
                        StartCoroutine(ShowLoadingImage(3f,() => GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Waving})));
                    break;
                case GameManager.GameState.Win: 
                        // endGameUI.ShowLevelCompletedPanel(); 
                        GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Excited});
                        ShowMenu<EndGameMenu>();
                        break;
                case GameManager.GameState.Lose:
                        // endGameUI.ShowLevelFailedPanel();
                        ShowMenu<EndGameMenu>();
                        break;
                case GameManager.GameState.Playing:
                    if(gameManager != null && gameManager.GetPrevState() != GameManager.GameState.Pause)
                    {
                        ClearUIStack();
                        ShowMenu<InGameMenu>();
                        if(gameManager.GetPrevState() == GameManager.GameState.MainMenu)
                        {
                            StartCoroutine(ShowLoadingImage(1f,null));
                            InGameMenu inGameMenu = GetMenu<InGameMenu>() as InGameMenu;
                            if (inGameMenu != null) inGameMenu.Setup();
                        }
                        else if (gameManager.isRestarting)
                        {
                            GameEventBus.Publish(new LoadingFinished());
                            InGameMenu inGameMenu = GetMenu<InGameMenu>() as InGameMenu;
                            if (inGameMenu != null) inGameMenu.Setup();
                        }
                    }
                    break;
            }
        }
    }
    private void ShowMenu<T>() where T : Menu
    {
        foreach(var menu in allMenu)
        {
            if(menu.GetType() == typeof(T))
            {
                menu.Show();
                return;
            }
        }
    }

    private Menu GetMenu<T>() where T : Menu
    {
        foreach(var menu in allMenu)
        {
            if(menu.GetType() == typeof(T))
            {
                menu.Show();
                return menu;
            }
        }

        Debug.LogWarning("Không tìm thất main menu panel");
        return null;
    }
    #endregion

    #region Xử lý popup

    public T ShowUI<T>() where T : UIView
    {
        Type uiType = typeof(T);

        if(uiDictionary.ContainsKey(uiType))
        {
            T uiInstance = uiDictionary[uiType] as T;

            if (uiStack.Count > 0 && uiStack.Peek() == uiInstance)
            {
                return uiInstance;
            }

            if(uiStack.Count > 0)
            {
                uiStack.Peek().Hide();
            }

            uiStack.Push(uiInstance);
            uiInstance.Show();
            UpdateDimImageState();

            return uiInstance;
        }

        Debug.LogError($"Không tìm thấy UI có kiểu: {uiType}");
        return null;
    }

    

    public void PopUI()
    {
        if(uiStack.Count > 0)
        {
            UIView currentUI = uiStack.Pop();
            currentUI.Hide();
        }
        if(uiStack.Count > 0)
        {
            uiStack.Peek().Show();
        }
        UpdateDimImageState();
    }

    public void UpdateDimImageState()
    {
        if (sharedDimImage != null)
        {
            sharedDimImage.SetActive(uiStack.Count > 0);
        }
    }

    private void ClearUIStack()
    {
        while(uiStack.Count > 0)
        {
            PopUI();
        }
        uiStack.Clear();
    }
    #endregion

    #region Xử lý Panel

    public void OpenShop()
    {
        if (gameManager == null) gameManager = CoreServices.Get<GameManager>();

        if (gameManager != null && gameManager.GetCurrState() != GameManager.GameState.MainMenu)
        {
            ShopPanel shop = ShowUI<ShopPanel>();
            if (shop != null)
            {
                shop.ShowCloseButton(true);
            }
        }
        else
        {
            MainMenu mainMenu = GetMenu<MainMenu>() as MainMenu;
            PopUI();
            if (mainMenu != null)
            {
                mainMenu.GoToShop();
            }
        }
    }
    public void CloseShop()
    {
        if (gameManager == null) gameManager = CoreServices.Get<GameManager>();

        if (gameManager != null && gameManager.GetCurrState() == GameManager.GameState.MainMenu)
        {
            MainMenu mainMenu = GetMenu<MainMenu>() as MainMenu;
            if (mainMenu != null)
            {
                mainMenu.OnHomeButtonClicked();
            }
            return;
        }

        if (uiStack.Count > 0 && uiStack.Peek() is ShopPanel)
        {
            PopUI();
        }
        else
        {
            ShopPanel shop = GetUI<ShopPanel>();
            if (shop != null) shop.Hide(); // Fallback an toàn
        }

        if (gameManager == null) return;
        
        if (gameManager.GetPrevState() == GameManager.GameState.Lose)
        {
            gameManager.ChangeState(GameManager.GameState.Lose);
            return;
        }
        
        if (uiStack.Count == 0 && gameManager.GetCurrState() == GameManager.GameState.Pause)
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
        ClearUIStack();
        gameManager.ChangeState(GameManager.GameState.MainMenu);
    }

    public void OnClickTryAgain()
    {
        CoreServices.Get<HeartManager>().UseHeart();
        if(CoreServices.Get<DataManager>().GetHearts() > 0)
        {
            
            CoreServices.Get<GameManager>().RestartLevel();
            ClearUIStack();
        } 
        else { ClearUIStack(); gameManager.ChangeState(GameManager.GameState.MainMenu); ShowUI<RefillHeartPopup>(); }
    }

    public void OnClickAddMoveToContinue()
    {
        int cost = Resources.Load<GameConfigSO>("GameConfig")?.addMoveCost ?? 900;
        if(CoreServices.Get<DataManager>().GetTotalCoins() >= cost)
        {
            // endGameUI.Hide();
            gameManager.AddMove(5);
            gameManager.ChangeState(GameManager.GameState.Playing);
            CoreServices.Get<DataManager>().UseCoins(cost);
        }
        else OpenShop();
    }

    public IEnumerator ShowLoadingImage(float time, Action action)
    {
        ShowUI<LoadingImage>();
        yield return new WaitForSeconds(time);
        PopUI();
        action?.Invoke();
    }
    #endregion
}
