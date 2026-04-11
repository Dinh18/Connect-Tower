using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum GameState
    {
        MainMenu,
        Playing,
        Pause,
        Win,
        Lose
    }
    private int moves;
    private int maxMoves;
    private GameState state;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private HeartManager heartManager;
    [SerializeField] private CameraController cameraController;
    public static event Action<int> OnChangeMoves;
    void Awake()
    {
        Instance = this;  
         
    }

    void Start()
    {
        cameraController.Setup();
        levelLoader.Setup(this, slotsManager, blocksManager);
        uIManager.Setup(this);
        ChangeState(GameState.MainMenu);
    }

    public SlotsManager GetSlotsManager()
    {
        return slotsManager;
    }

    public GameState GetCurrState() => state;
    public int GetMaxMoves() => maxMoves;
    public int GetMoves() => moves;
    public void SetupLevel(int maxMoves)
    {
        moves = maxMoves;
        this.maxMoves = maxMoves;
        OnChangeMoves?.Invoke(moves);
        cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
    }
    public void Move()
    {
        moves--;
        OnChangeMoves?.Invoke(moves);
        if(moves <= 0)
        {
            ChangeState(GameState.Lose);
            // DataManager.Instance.UseHeart();
            heartManager.UseHeart();
        }
    }
    public void AddMove(int moves)
    {
        this.moves += moves;
        OnChangeMoves?.Invoke(this.moves);
    }
    
    public void ChangeState(GameState gameState)
    {
        if(gameState == GameState.Playing)
        {
            levelLoader.LoadLevel();
        }
        uIManager.UpdateUI(gameState);
        this.state = gameState;
        Debug.Log("CurrState: " + this.state);
    }
}
