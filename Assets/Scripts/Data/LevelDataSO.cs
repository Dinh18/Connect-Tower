using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "Level_", menuName = "Scriptable Objects/Create Level")]
public class LevelDataSO : ScriptableObject
{
    public int level;
    public int numsTopic;
    public int difficult;
    public int moves;
    public int row1;
    public int row2;
    public List<SlotSetupData> slots = new List<SlotSetupData>();
    
}

[System.Serializable]
public class SlotSetupData
{
    // public List<BlockTopic> blocks= new List<BlockTopic>();
    public SlotController.SlotType slotType = SlotController.SlotType.Normal;
    // [ShowIf("slotType", SlotController.SlotType.Hide)]
    public BlockTopic questionTopic;
    public List<BlockSetupData> blocks= new List<BlockSetupData>();

}
[System.Serializable]
public class BlockSetupData
{
    public BlockController.BlockType typeBlock;
    public BlockTopic blockTopic;
    public int indexSprite;
}
