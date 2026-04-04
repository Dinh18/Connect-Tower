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
    public int moves;
    private GameState state;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private CameraController cameraController;
    public static event Action<int> OnChangeMoves;
    void Awake()
    {
        Instance = this;   
    }

    void Start()
    {
        uIManager.Setup(this);
        cameraController.Setup();
        levelLoader.Setup(this, slotsManager, blocksManager);
        ChangeState(GameState.MainMenu);
    }

    public SlotsManager GetSlotsManager()
    {
        return slotsManager;
    }
    public void SetupLevel(int maxMoves)
    {
        moves = maxMoves;
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
        }
    }
    public void AddMove(int moves)
    {
        this.moves += moves;
        OnChangeMoves?.Invoke(this.moves);
    }
    public GameState GetCurrState() => state;
    public void ChangeState(GameState gameState)
    {
        if(gameState == GameState.Playing)
        {
            levelLoader.LoadLevel();
        }
        this.state = gameState;
        uIManager.UpdateUI(gameState);
        Debug.Log("CurrState: " + this.state);
    }
}
