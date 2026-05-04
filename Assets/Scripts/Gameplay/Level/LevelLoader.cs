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

    public void Init(SlotsManager slotsM, BlocksManager blocksM, GameManager gameM, DataManager dataM)
    {
        this.slotsManager = slotsM;
        this.blocksManager = blocksM;
        this.gameManager = gameM;
        this.dataManager = dataM;

        CoreServices.Register<LevelLoader>(this);
        
        // Pre-load resources
        levelDatas = Resources.LoadAll<LevelDataSO>(Constants.LEVELS_PATH);
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
        LevelDataSO levelData = levelDatas[dataManager.GetCurrentLevel()];
        numsTopic = levelData.numsTopic;
        gameDifficult = (GameDifficult)levelData.difficult;

        slots = new List<SlotController>();
        slotsManager.SlotsGenerate(levelData.row1, levelData.row2, slots, levelData.slots, numsTopic);
        blocksManager.BlocksGenerate(levelData.slots, slots);
        gameManager.SetupLevel(levelData.moves);
        
        foreach(SlotController slot in slots) slot.SetupIceSlot();
        
        GameEventBus.Publish(new LevelLoadedEvent { levelIndex = dataManager.GetCurrentLevel() });
    }

    public int GetCurrentLevelReward()
    {
        GameConfigSO config = Resources.Load<GameConfigSO>("GameConfig");
        if (config == null) return 40;

        if (gameDifficult == GameDifficult.Easy) return config.coinRewardEasy;
        if (gameDifficult == GameDifficult.Hard) return config.coinRewardHard;
        return config.coinRewardSuperHard;
    }
}
