using System.Collections.Generic;
using Unity.Android.Gradle;
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
    // public static LevelLoader Instance;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private GameManager gameManager;
    private LevelDataSO[] levelDatas;
    // [SerializeField] DataLevel dataTest;
    public List<SlotController> slots;
    public GameDifficult gameDifficult;

    public void Setup(GameManager gameManager, SlotsManager slotsManager, BlocksManager blocksManager)
    {
        this.gameManager = gameManager;
        this.slotsManager = slotsManager;
        this.blocksManager = blocksManager;
        levelDatas = Resources.LoadAll<LevelDataSO>(Constants.LEVELS_PATH);
        // LoadLevel(DataManager.Instance.playerData.currentLevel);
        slotsManager.Setup(this);
    }

    public void LevelUp()
    {
        if(DataManager.Instance.playerData.currentLevel < levelDatas.Length)
        {
            DataManager.Instance.LevelUp(gameDifficult);
        }
    }
    public void LoadLevel()
    {

        LevelDataSO levelData = levelDatas[DataManager.Instance.playerData.currentLevel];
        int row1 = levelData.row1;
        int row2 = levelData.row2;
        List<SlotSetupData> slotSetup = levelData.slots;
        int numsTopic = levelData.numsTopic;
        gameDifficult = (GameDifficult)levelData.difficult;

        slots = new List<SlotController>();

        slotsManager.SlotsGenerate(row1, row2, slots, slotSetup, numsTopic);

        blocksManager.BlocksGenerate(slotSetup, slots);

        gameManager.SetupLevel(levelData.moves);
        
        foreach(SlotController slot in slots)
        {
            slot.SetupIceSlot();
        }
    }

    
}
