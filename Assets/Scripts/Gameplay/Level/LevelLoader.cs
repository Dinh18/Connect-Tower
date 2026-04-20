using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
/*
- số lượng slot hàng 1, hàng 2
- độ khó
- số loại block,
- các block ở mỗi slot
*/
public class LevelLoader : MonoBehaviour
{
    public enum GameDifficult
    {
        Easy = 0,
        Hard = 1,
        VeryHard = 2
    }
    public static LevelLoader Instance;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private GameManager gameManager;
    // private int coinWin;
    private LevelDataSO[] levelDatas;
    // [SerializeField] DataLevel dataTest;
    public List<SlotController> slots;
    public GameDifficult gameDifficult;
    private int numsTopic;

    public int GetNumsTopic() => numsTopic;

    void Awake()
    {
        Instance = this;
    }

    public int GetNumsLevel() => levelDatas.Length;

    public int GetDifficultLevel(int lvl) => levelDatas[lvl].difficult;

    public SlotController GetSlotByIndex(int index)
    {
        return slots[index];
    }

    public void Setup(GameManager gameManager, SlotsManager slotsManager, BlocksManager blocksManager)
    {
        this.gameManager = gameManager;
        this.slotsManager = slotsManager;
        this.blocksManager = blocksManager;
        levelDatas = Resources.LoadAll<LevelDataSO>(Constants.LEVELS_PATH);
        slotsManager.Setup(this);
        blocksManager.PoolBlock(40);
        slotsManager.PoolSlot(10);
    }

    public void LevelUp()
    {
        DataManager.Instance.LevelUp(gameDifficult, levelDatas.Length - 1); 
    }
    public void LoadLevel()
    {

        LevelDataSO levelData = levelDatas[DataManager.Instance.playerData.currentLevel];
        int row1 = levelData.row1;
        int row2 = levelData.row2;
        List<SlotSetupData> slotSetup = levelData.slots;
        numsTopic = levelData.numsTopic;
        gameDifficult = (GameDifficult)levelData.difficult;

        slots = new List<SlotController>();

        slotsManager.SlotsGenerate(row1, row2, slots, slotSetup, numsTopic);

        blocksManager.BlocksGenerate(slotSetup, slots);

        gameManager.SetupLevel(levelData.moves);
        
        foreach(SlotController slot in slots)
        {
            slot.SetupIceSlot();
        }

        if(DataManager.Instance.playerData.currentLevel == 0)
        {
            TutorialManager.Instance.SetupFirstTimeTutorial();
            TutorialManager.Instance.StartFirstTimeTutorial();
        }

        foreach(var mechanic in DataManager.Instance.playerData.mechanics)
        {
            if(DataManager.Instance.IsFirstTimePlayMechanic(mechanic.id))
            {
                TutorialManager.Instance.StartMechanicTutorial(mechanic.id);
            }
        }



    }

    public int GetCurrentLevelReward()
    {
        GameConfigSO config = Resources.Load<GameConfigSO>("GameConfig");
        if (config == null) 
        {
            Debug.LogWarning("Không tìm thấy GameConfig trong thư mục Resources!");
            return 40; // Default fallback
        }

        if (gameDifficult == GameDifficult.Easy) return config.coinRewardEasy;
        if (gameDifficult == GameDifficult.Hard) return config.coinRewardHard;
        return config.coinRewardSuperHard;
    }

    
}
