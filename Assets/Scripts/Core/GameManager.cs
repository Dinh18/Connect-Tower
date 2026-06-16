using System;
using System.Collections;
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
    public bool isRestarting = false;
    private int pendingMoves = 0;

    public int GetPendingMoves() => pendingMoves;
    public void IncrementPendingMoves() => pendingMoves++;
    public bool IsInfiniteMovesActive() => isInfiniteMovesActive;

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
    public int GetCurrentMoves() => moves;
    public bool Moved()
    {
        return moves < maxMoves;
    }
    public void SetupLevel(int maxMoves)
    {
        this.moves = maxMoves;
        this.maxMoves = maxMoves;
        this.pendingMoves = 0;
        OnChangeMoves?.Invoke(moves);
        GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
        CoreServices.Get<DataManager>().ResetSessionUndo();
        // cameraController.FitCamera(slotsManager.row1, slotsManager.row2);
        // CameraFitter.FitBoardOrtho(Camera.main, 2, )
    }

    private Coroutine noMovesCoroutine;
    private bool isWaitingForNoMoves = false;
    private bool isWaitingForLowMoves = false;

    public void Move(bool isMoving)
    {
        if (pendingMoves > 0) pendingMoves--;
        GameEventBus.Publish(new MoveFinished());
        if(isInfiniteMovesActive) return;
        if(!isMoving)
        {
            moves--;
            OnChangeMoves?.Invoke(moves);
            GameEventBus.Publish(new MovesUpdatedEvent { currentMoves = this.moves });
            
            if(moves <= 0 && !slotsManager.GetLevelComleted())
            {
                // ChangeState(GameState.Lose);
                CoreServices.Get<UIManager>().ShowUI<OutOfMovePopup>();
                // CoreServices.Get<DataManager>().ResetWinStreak();
            }
            else if (!slotsManager.GetLevelComleted())
            {
                // Nếu đang chờ 5s, bỏ qua việc kiểm tra board
                if (!slotsManager.HasAvailableMoves() && !isWaitingForNoMoves)
                {
                    Debug.Log("Không còn nước đi hợp lệ nào! Bắt đầu đếm ngược 5s.");
                    isWaitingForNoMoves = true;
                    noMovesCoroutine = StartCoroutine(WaitAndTriggerNoMoves());
                }

                if(moves <= 2 && !isWaitingForLowMoves)
                {
                    Debug.Log("Sắp hết lượt di chuyển! Bắt đầu đếm ngược 5s.");
                    isWaitingForLowMoves = true;
                    StartCoroutine(WaitAndTriggerLowMoves());
                }
            }
        }
    }

    private IEnumerator WaitAndTriggerLowMoves()
    {
        if(CoreServices.Get<DataManager>().IsFirstTimeUserBooster((int)BoosterType.AddMove))
        {
            yield return new WaitForSeconds(0.5f); // Đợi 0.5s trước khi bắn sự kiện để đảm bảo mọi thứ đã ổn định sau nước đi cuối cùng
            GameEventBus.Publish(new LowMovesEvent());
            GameEventBus.Publish(new RequestUnlockBoosterEvent { boosterType = BoosterType.AddMove });
            CoreServices.Get<DataManager>().UnlockBooster((int)BoosterType.AddMove);
            // yield break; // Không cần đợi nếu người chơi chưa từng dùng booster Extra Moves
        }
        yield return new WaitForSeconds(2f); // Đợi 2s để tránh spam sự kiện khi người chơi nhanh tay
        GameEventBus.Publish(new LowMovesEvent());
        isWaitingForLowMoves = false;
        
    }

    private IEnumerator WaitAndTriggerNoMoves()
    {
        if(CoreServices.Get<DataManager>().IsFirstTimeUserBooster((int)BoosterType.Shuffle) || CoreServices.Get<DataManager>().IsFirstTimeUserBooster((int)BoosterType.Undo))
        {
            yield return new WaitForSeconds(1.5f); // Đợi 1s trước khi bắn sự kiện để đảm bảo mọi thứ đã ổn định sau nước đi cuối cùng
            GameEventBus.Publish(new NoMovesAvailableEvent());
            if(CoreServices.Get<DataManager>().IsFirstTimeUserBooster((int)BoosterType.Undo))
            {
                GameEventBus.Publish(new RequestUnlockBoosterEvent { boosterType = BoosterType.Undo });
                CoreServices.Get<DataManager>().UnlockBooster((int)BoosterType.Undo);
            }
            else if(CoreServices.Get<DataManager>().IsFirstTimeUserBooster((int)BoosterType.Shuffle))
            {
                GameEventBus.Publish(new RequestUnlockBoosterEvent { boosterType = BoosterType.Shuffle });
                CoreServices.Get<DataManager>().UnlockBooster((int)BoosterType.Shuffle);
            }
        }
        yield return new WaitForSeconds(3f);

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
        isRestarting = true;
        SetupLevel(maxMoves);
        levelLoader.LoadLevel();
        CoreServices.Get<GamePlayController>().ResetSelection();
        CoreServices.Get<GamePlayController>().ResetUndoStack();
        GameEventBus.Publish(new BoardStateChangedEvent());
        ChangeState(GameState.Playing);
        isRestarting = false;
        
    }

    public bool AddMoveToContinue(int extraMoves)
    {
        if(CoreServices.Get<DataManager>().GetTotalCoins() >= 900)
        {
            AddMove(extraMoves);
            CoreServices.Get<DataManager>().UseCoins(900);
            ChangeState(GameState.Playing);
            return true;
        }
        return false;
        
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

        if (currState == GameState.MainMenu)
        {
            if (levelLoader != null)
            {
                levelLoader.ClearLevel();
            }
        }

        if(currState == GameState.Playing && prevState != GameState.Pause && prevState != GameState.Lose)
        {
            levelLoader.LoadLevel();
            CoreServices.Get<GamePlayController>().ResetSelection();
            CoreServices.Get<GamePlayController>().ResetUndoStack();
        }

        if(currState == GameState.Playing && prevState == GameState.MainMenu)
        {
            DataManager dataManager = CoreServices.Get<DataManager>();
            foreach(BoosterDataSO booster in dataManager.GetAllBoosters())
            {
                if(dataManager.IsFirstTimeUserBooster(int.Parse(booster.id)))
                {
                    StartCoroutine(DelayedTutorial(2f, dataManager, booster));
                    break;
                }
            }
        }
        GameEventBus.Publish(new GameStateChangedEvent { newState = currState });
        Debug.Log($"[GameManager] State Changed: {prevState} -> {currState}");
    }

    public IEnumerator DelayedTutorial(float delay, DataManager dataManager, BoosterDataSO booster)
    {
        if(booster.unlockedLevel == dataManager.GetCurrentLevel() && int.Parse(booster.id) != (int)BoosterType.Hint) yield break;
        yield return new WaitForSeconds(delay);
        GameEventBus.Publish(new RequestUnlockBoosterEvent { boosterType = (BoosterType)int.Parse(booster.id) });
        dataManager.UnlockBooster(int.Parse(booster.id));
    }
}
