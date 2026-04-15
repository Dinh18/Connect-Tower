using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum GameState
    {
        None,
        MainMenu,
        Playing,
        Pause,
        Win,
        Lose,
        Resume
    }
    private int moves;
    private int maxMoves;
    private GameState currState;
    private GameState prevState = GameState.None;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private HeartManager heartManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private InputManager inputManager;
    public static event Action<int> OnChangeMoves;
    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        cameraController.Setup();
        levelLoader.Setup(this, slotsManager, blocksManager);
        uIManager.Setup(this);
        // prevState = GameState.None;
        currState = GameState.None;
        ChangeState(GameState.MainMenu);
        SlotController.OnMoveFisnished+=Move;
    }

    public SlotsManager GetSlotsManager()
    {
        return slotsManager;
    }

    public GameState GetCurrState() => currState;
    public GameState GetPrevState() => prevState;
    public int GetMaxMoves() => maxMoves;
    public int GetMoves() => moves;
    public void SetupLevel(int maxMoves)
    {
        moves = maxMoves;
        this.maxMoves = maxMoves;
        OnChangeMoves?.Invoke(moves);
        cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
    }
    public void Move(bool isMoving)
    {
        if(!isMoving)
        {
            moves--;
            OnChangeMoves?.Invoke(moves);
            if(moves <= 0)
            {
                ChangeState(GameState.Lose);
                // DataManager.Instance.UseHeart();
                // heartManager.UseHeart();
            }
        }
    }
    public void UseHeart()
    {
        heartManager.UseHeart();
    }
    public void AddMove(int moves)
    {
        this.moves += moves;
        OnChangeMoves?.Invoke(this.moves);
    }
    
    public void ChangeState(GameState newState)
    {
        if(currState == newState) return;
        
        prevState = currState;
        currState = newState;

        if(currState == GameState.Playing)
        {
            if(prevState != GameState.Pause)
            {
                levelLoader.LoadLevel();
                inputManager.Setup();
            }
        }

        uIManager.UpdateUI(currState);
        Debug.Log($"State Changed: {prevState} -> {currState}");
    }
}
