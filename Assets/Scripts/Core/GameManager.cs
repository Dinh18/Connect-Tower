using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        MainMenu,
        Playing, 
        Pause, 
        Win, 
        Lose, 
        Resume }
    
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
    public bool Moved()
    {
        return moves < maxMoves;
    }
    public void SetupLevel(int maxMoves)
    {
        this.moves = maxMoves;
        this.maxMoves = maxMoves;
        OnChangeMoves?.Invoke(moves);
        GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
        cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
    }

    private Coroutine noMovesCoroutine;
    private bool isWaitingForNoMoves = false;

    public void Move(bool isMoving)
    {
        if(isInfiniteMovesActive) return;
        if(!isMoving)
        {
            moves--;
            OnChangeMoves?.Invoke(moves);
            GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
            
            if(moves <= 0 && !slotsManager.GetLevelComleted())
            {
                ChangeState(GameState.Lose);
                // CoreServices.Get<DataManager>().ResetWinStreak();
            }
            else if (!slotsManager.GetLevelComleted())
            {
                // Nếu đang chờ 5s, bỏ qua việc kiểm tra board
                if (isWaitingForNoMoves) return;

                if (!slotsManager.HasAvailableMoves())
                {
                    Debug.Log("Không còn nước đi hợp lệ nào! Bắt đầu đếm ngược 5s.");
                    isWaitingForNoMoves = true;
                    noMovesCoroutine = StartCoroutine(WaitAndTriggerNoMoves());
                }
            }
        }
    }

    private System.Collections.IEnumerator WaitAndTriggerNoMoves()
    {
        yield return new WaitForSeconds(5f);

        // Sau 5s, kiểm tra xem game còn đang chơi không và chưa qua màn
        if (currState == GameState.Playing && !slotsManager.GetLevelComleted())
        {
            // Kiểm tra lại lần nữa phòng trường hợp người chơi dùng booster làm thay đổi board
            if (!slotsManager.HasAvailableMoves())
            {
                Debug.Log("Hết 5s, vẫn không có nước đi! Bắn sự kiện.");
                GameEventBus.Publish(new NoMovesAvailableEvent());
                // ChangeState(GameState.Lose);
            }
        }
        
        isWaitingForNoMoves = false;
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

        if (noMovesCoroutine != null)
        {
            StopCoroutine(noMovesCoroutine);
            noMovesCoroutine = null;
        }
        isWaitingForNoMoves = false;
        
        // Bỏ qua việc xét Thắng/Thua nếu đang dùng cọ vẽ trong Level Editor
        if (RuntimeLevelEditorManager.Instance != null && RuntimeLevelEditorManager.Instance.isEditMode)
        {
            if (newState == GameState.Win || newState == GameState.Lose)
            {
                return;
            }
        }

        // Tự động quay lại Editor khi chơi thử hoàn thành (không hiện bảng Win/Lose)
        if (LevelLoader.isPlaytestingTempLevel && (newState == GameState.Win || newState == GameState.Lose))
        {
            if (RuntimeLevelEditorManager.Instance != null)
            {
                RuntimeLevelEditorManager.Instance.RestoreFromPlaytest();
            }
            return;
        }

        prevState = currState;
        currState = newState;

        if(currState == GameState.Playing && prevState != GameState.Pause && prevState != GameState.Lose)
        {
            levelLoader.LoadLevel();
            CoreServices.Get<GamePlayController>().ResetSelection();
            CoreServices.Get<GamePlayController>().ResetUndoStack();
        }

        GameEventBus.Publish<GameStateChangedEvent>(new GameStateChangedEvent { newState = currState });
        Debug.Log($"[GameManager] State Changed: {prevState} -> {currState}");
    }
}
