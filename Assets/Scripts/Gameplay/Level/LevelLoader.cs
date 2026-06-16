using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public enum GameDifficult { Easy = 0, Hard = 1, VeryHard = 2 }

    private SlotsManager slotsManager;
    private BlocksManager blocksManager;
    private GameManager gameManager;
    private DataManager dataManager;
    private LevelDataSO[] levelDatas;
    
    public List<SlotController> slots;
    public GameDifficult gameDifficult;
    private int numsTopic;

    public static bool isPlaytestingTempLevel = false;
    public static LevelDataSO playtestLevelData = null;

    public GameMode gameMode;

    public LevelDataSO GetCurrentLevelDataSO()
    {
        return levelDatas[CoreServices.Get<DataManager>().GetCurrentLevel()];
    }

    public void Init(SlotsManager slotsM, BlocksManager blocksM, GameManager gameM, DataManager dataM)
    {
        this.slotsManager = slotsM;
        this.blocksManager = blocksM;
        this.gameManager = gameM;
        this.dataManager = dataM;

        CoreServices.Register<LevelLoader>(this);
        
        // Pre-load resources
        levelDatas = Resources.LoadAll<LevelDataSO>(Constants.LEVELS_PATH);
        System.Array.Sort(levelDatas, (a, b) => a.level.CompareTo(b.level));
        blocksManager.PoolBlock(40);
        slotsManager.PoolSlot(10);
    }

    public int GetNumsLevel() => levelDatas.Length;
    public int GetDifficultLevel(int lvl) => levelDatas[lvl].difficult;
    public SlotController GetSlotByIndex(int index) => slots[index];
    public int GetNumsTopic() => numsTopic;

    public void LevelUp()
    {
        dataManager.LevelUp(gameDifficult, levelDatas.Length - 1); 
    }

    public void LoadLevel()
    {
        LevelDataSO levelData = isPlaytestingTempLevel && playtestLevelData != null ? playtestLevelData : levelDatas[dataManager.GetCurrentLevel()];
        numsTopic = levelData.numsTopic;
        gameDifficult = (GameDifficult)levelData.difficult;
        gameMode = levelData.gameMode;

        slots = new List<SlotController>();
        slotsManager.SlotsGenerate(levelData.row1, levelData.row2, slots, levelData.slots, numsTopic);
        blocksManager.BlocksGenerate(levelData.slots, slots);
        gameManager.SetupLevel(levelData.moves);
        CoreServices.Get<HeatManager>().Setup(slots);
        
        foreach(SlotController slot in slots) slot.SetupIceSlot();
        
        GameEventBus.Publish(new LevelLoadedEvent { levelIndex = isPlaytestingTempLevel ? -1 : dataManager.GetCurrentLevel() });
    }

    public void ClearLevel()
    {
        if (blocksManager != null) blocksManager.ClearBlocks();
        if (slotsManager != null) slotsManager.ClearSlots();
        if (slots != null) slots.Clear();
    }

    public int GetCurrentLevelReward()
    {
        return CoreServices.Get<DataManager>().CoinReward(gameDifficult);
    }
}
