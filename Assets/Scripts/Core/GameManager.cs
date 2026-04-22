using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { None, MainMenu, Playing, Pause, Win, Lose, Resume }
    
    private GameState currState = GameState.None;
    private GameState prevState = GameState.None;
    private int moves;
    private int maxMoves;

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

    void OnEnable() { SlotController.OnMoveFisnished += Move; }
    void OnDisable() { SlotController.OnMoveFisnished -= Move; }

    public GameState GetCurrState() => currState;
    public GameState GetPrevState() => prevState;
    public int GetMaxMoves() => maxMoves;
    public int GetMoves() => moves;

    public void SetupLevel(int maxMoves)
    {
        this.moves = maxMoves;
        this.maxMoves = maxMoves;
        OnChangeMoves?.Invoke(moves);
        GameEventBus.OnMovesUpdated?.Invoke(moves);
        cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
    }

    public void Move(bool isMoving)
    {
        if(!isMoving)
        {
            moves--;
            OnChangeMoves?.Invoke(moves);
            GameEventBus.OnMovesUpdated?.Invoke(moves);
            
            if(moves <= 0 && !slotsManager.GetLevelComleted())
                ChangeState(GameState.Lose);
        }
    }

    public void UseHeart() => heartManager.UseHeart();
    public void AddMove(int moves)
    {
        this.moves += moves;
        OnChangeMoves?.Invoke(this.moves);
        GameEventBus.OnMovesUpdated?.Invoke(this.moves);
    }
    
    public void ChangeState(GameState newState)
    {
        if(currState == newState) return;
        
        prevState = currState;
        currState = newState;

        if(currState == GameState.Playing && prevState != GameState.Pause)
        {
            levelLoader.LoadLevel();
        }

        GameEventBus.OnGameStateChanged?.Invoke(currState);
        Debug.Log($"[GameManager] State Changed: {prevState} -> {currState}");
    }
}
