using System;
using System.Collections.Generic;
using UnityEngine;
public enum PanelType
{
    MainMenu,
    InGame,
    Shop,
    ShopFromMainMenu,
    EndGameWin,
    EndGameLose
}

public enum PopupType
{
    RefillHeart,
    Booster,
    Setting
}
public enum BorderType
{
    Warning,
    Ice
}
public interface IGameEvent{}
public static class GameEventBus
{
    private static readonly Dictionary<Type, Delegate> _eventListeners = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);
        if(_eventListeners.ContainsKey(eventType))
        {
            _eventListeners[eventType] = Delegate.Combine(_eventListeners[eventType], listener);
        }
        else
        {
            _eventListeners[eventType] = listener;
        }
    }

    public static void UnSubscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);
        var currentDelegate = _eventListeners[eventType];
        var newDelegate = Delegate.Remove(currentDelegate, listener);
        if(newDelegate == null)
        {
            _eventListeners.Remove(eventType);
        }
        else
        {
            _eventListeners[eventType] = newDelegate;
        }
    }

    public static void Publish<T>(T gameEvent) where T : IGameEvent
    {
        Type eventType = typeof(T);
        if(_eventListeners.ContainsKey(eventType))
        {
            if(_eventListeners[eventType] is Action<T> action)
            {
                action.Invoke(gameEvent);
            }
        }
    }

    public static void ClearAllListeners()
    {
        _eventListeners.Clear();
    }
}
public struct StartBorderFlashEvent : IGameEvent
{
    public BorderType borderType;
    public float flashSpeed;
    public float flashTime;
}

public struct StopBorderFlashEvent : IGameEvent
{
    
}

public struct GameStateChangedEvent : IGameEvent
{
    public GameManager.GameState newState;
}

public struct MovesUpdatedEvent : IGameEvent
{
    public int currentMoves;
}

public struct RequestOpenPanelEvent : IGameEvent
{
    public PanelType targetPanel;
}

public struct RequestOpenPopupEvent : IGameEvent
{
    public PopupType targetPopup;
}

public struct RequestCloseBoosterPopupEvent : IGameEvent
{
    
}

public struct RequestOpenBoosterPopupEvent : IGameEvent
{
    public Constants.BoosterType type;
}
public struct LevelLoadedEvent : IGameEvent
{
    public int levelIndex;
}

public struct FinishedSlotsUpdatedEvent : IGameEvent
{
    public int finishedSlots;
    public int totalSlots;
}

public struct CoinsUpdatedEvent : IGameEvent
{
    public int totalCoins;
}

public struct AddBoosterEvent : IGameEvent
{
    public BoosterButton boosterButton;
}

public struct HeartUpdatedEvent : IGameEvent
{
    public int heartCount;
}

public struct BoosterCountUpdatedEvent : IGameEvent
{
    public int boosterId;
    public int count;
}

public struct LevelUpdatedEvent : IGameEvent
{
    public int newLevel;
}
