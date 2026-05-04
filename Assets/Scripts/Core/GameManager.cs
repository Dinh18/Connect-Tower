using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { None, MainMenu, Playing, Pause, Win, Lose, Resume }
    
    private GameState currState = GameState.None;
    private GameState prevState = GameState.None;
    private int moves;
    private int maxMoves;
    private bool isInfiniteMovesActive = false;

    // Dependencies injected via Init
    private CameraController cameraController;
    private SlotsManager slotsManager;
    private HeartManager heartManager;
    private LevelLoader levelLoader;

    public static event Action<int> OnChangeMoves;

    public void Init(SlotsManager slots, HeartManager heart, CameraController cam, LevelLoader loader)
    {
        this.slotsManager = slots;
        this.heartManager = heart;
        this.cameraController = cam;
        this.levelLoader = loader;

        CoreServices.Register<GameManager>(this);
        Application.targetFrameRate = 60;
    }

    public void StartGame()
    {
        ChangeState(GameState.MainMenu);
    }

    void OnEnable()
    { 
        SlotController.OnMoveFisnished += Move;
        GameEventBus.Subscribe<StartBorderFlashEvent>(StartInfiniteMovesBooster);
        GameEventBus.Subscribe<StopBorderFlashEvent>(StopInfiniteMovesBooster);
    }
    void OnDisable()
    {
        SlotController.OnMoveFisnished -= Move;
        GameEventBus.UnSubscribe<StartBorderFlashEvent>(StartInfiniteMovesBooster);
        GameEventBus.UnSubscribe<StopBorderFlashEvent>(StopInfiniteMovesBooster);
    }

    public GameState GetCurrState() => currState;
    public GameState GetPrevState() => prevState;
    public int GetMaxMoves() => maxMoves;
    public int GetMoves() => moves;

    public void SetupLevel(int maxMoves)
    {
        this.moves = maxMoves;
        this.maxMoves = maxMoves;
        OnChangeMoves?.Invoke(moves);
        GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
        cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
    }

    public void Move(bool isMoving)
    {
        if(isInfiniteMovesActive) return;
        if(!isMoving)
        {
            moves--;
            OnChangeMoves?.Invoke(moves);
            GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
            
            if(moves <= 0 && !slotsManager.GetLevelComleted())
                ChangeState(GameState.Lose);
        }
    }

    private void StartInfiniteMovesBooster(StartBorderFlashEvent startBorderFlash)
    {
        if(startBorderFlash.borderType == BorderType.Ice)
        {
            isInfiniteMovesActive = true;
        }
    }

    private void StopInfiniteMovesBooster(StopBorderFlashEvent stopBorderFlash)
    {
        isInfiniteMovesActive = false;
    }

    public void UseHeart() => heartManager.UseHeart();
    public void AddMove(int moves)
    {
        this.moves += moves;
        OnChangeMoves?.Invoke(this.moves);
        // GameEventBus.OnMovesUpdated?.Invoke(this.moves);
        GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
    }
    public void RestartLevel()
    {
        SetupLevel(maxMoves);
        levelLoader.LoadLevel();
        ChangeState(GameState.Playing);
    }

    public void AddMoveToContinue(int extraMoves)
    {
        AddMove(extraMoves);
        ChangeState(GameState.Playing);
    }
    
    public void ChangeState(GameState newState)
    {
        if(currState == newState) return;
        
        prevState = currState;
        currState = newState;

        if(currState == GameState.Playing && prevState != GameState.Pause && prevState != GameState.Lose)
        {
            levelLoader.LoadLevel();
        }

        GameEventBus.Publish<GameStateChangedEvent>(new GameStateChangedEvent { newState = currState });
        Debug.Log($"[GameManager] State Changed: {prevState} -> {currState}");
    }
}
