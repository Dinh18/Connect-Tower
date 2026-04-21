using System;
using UnityEngine;

public static class GameEventBus
{
    // --- GAME STATE EVENTS ---
    public static Action<GameManager.GameState> OnGameStateChanged;
    public static Action<int> OnMovesUpdated;
    public static Action OnLevelCompleted;
    public static Action OnLevelFailed;

    // --- UI NAVIGATION EVENTS ---
    public enum UIType
    {
        MainMenu,
        InGame,
        Settings,
        Shop,
        ShopFromMainMenu,
        AddBooster,
        RefillHeart,
        EndGameWin,
        EndGameLose
    }

    public static Action<UIType> OnRequestUI;
    public static Action OnRequestClosePopup;
    public static Action<int> OnRequestAddMove;
    
    // --- SPECIAL UI ACTIONS ---
    public static Action OnRequestBackHome;
    public static Action OnRequestTryAgain;
    public static Action OnRequestAddMoveToContinue;

    // --- BOOSTER EVENTS ---
    public static Action<AddBoosterUI, BoosterButton, string, string, Constants.BoosterType, bool> OnRequestAddBooster;
    public static Action<BoosterButton> OnClickAddBooster;
}
