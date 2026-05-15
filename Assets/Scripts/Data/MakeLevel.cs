using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class MakeLevel : MonoBehaviour
{
    [Header("Level data")]
    public int Level = 0;
    public int moves = 10;
    [HideInInspector] public int row1;
    [HideInInspector] public int row2;
    [HideInInspector] public int totalTopics;
    
    // [HideInInspector] public Dictionary<int, int> amountBlockOfTopic = new Dictionary<int, int>();
    public List<BlockTopic> topics;
    [HideInInspector]public List<int> amountBlockOfTopic;
    [Header("Slot Settings")]
    public GameObject slotPrefab;
    private List<Transform> stackHolders = new List<Transform>();
    [HideInInspector] public List<SlotSetupData> slots = new List<SlotSetupData>();
    [Header("Block Settings")]
    public Transform blockHolder;
    public GameObject blockPrefab;
    // Editor Only
    public int indexTopicSelected = 0;

    private List<SlotController> slotControllers = new List<SlotController>();

    // [ContextMenu("Generate Slots In Scene ")]
    public void SettingSlots()
    {
        int totalSlots = row1 + row2;

        while (slots.Count > totalSlots) slots.RemoveAt(slots.Count - 1);
        while (slots.Count < totalSlots) slots.Add(new SlotSetupData());

        while(topics.Count > totalTopics)
        {
            topics.RemoveAt(topics.Count - 1);
            amountBlockOfTopic.RemoveAt(amountBlockOfTopic.Count - 1);
        } 
        while(topics.Count < totalTopics)
        {
            topics.Add(new BlockTopic());
            amountBlockOfTopic.Add(0);
        } 

        indexTopicSelected = 0;
    }
    public void SyncPositionsFromScene()
    {
        if (slotControllers == null || slotControllers.Count != slots.Count) return;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slotControllers[i] != null)
                slots[i].position = slotControllers[i].transform.localPosition;
        }
    }

    public void UpdateSlotsInEditor()
    {
        if (slotPrefab == null) return;
        SyncPositionsFromScene();
        SettingSlots(); // Ensure data matches rows

        slotControllers = new List<SlotController>();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        stackHolders.Clear();

        float startX_Row1 = -(row1 - 1) * Constants.SLOT_WIDTH / 2f; 
        int j = 0;
        for (int i = 0; i < row1; i++)
        {
            if (j >= slots.Count) break;
            GameObject newSlot = Instantiate(slotPrefab, transform);
            newSlot.name = "Slot_0_" + i;
            
            if (slots[j].position == Vector3.zero) {
                slots[j].position = new Vector3(startX_Row1 + (i * Constants.SLOT_WIDTH), 0, 0);
            }
            newSlot.transform.localPosition = slots[j].position;

            newSlot.GetComponent<SlotController>().Setup(slots[j].slotType, 0,slots[j].questionTopic ? slots[j].questionTopic : null);
            slotControllers.Add(newSlot.GetComponent<SlotController>());
            stackHolders.Add(newSlot.gameObject.GetComponentInChildren<StackHolder>().transform);
            j++;
        }
     
        float startX_Row2 = -(row2 - 1) * Constants.SLOT_WIDTH / 2f;
        for(int i = 0; i < row2; i++)
        {
            if (j >= slots.Count) break;
            GameObject newSlot = Instantiate(slotPrefab, transform);
            newSlot.name = "Slot_1_" + i;

            if (slots[j].position == Vector3.zero) {
                slots[j].position = new Vector3(startX_Row2 + (i * Constants.SLOT_WIDTH), Constants.SLOT_HEIGHT, 0);
            }
            newSlot.transform.localPosition = slots[j].position;

            newSlot.GetComponent<SlotController>().Setup(slots[j].slotType, 1,slots[j].questionTopic ? slots[j].questionTopic : null);
            slotControllers.Add(newSlot.GetComponent<SlotController>());
            stackHolders.Add(newSlot.gameObject.GetComponentInChildren<StackHolder>().transform);
            j++;
        }
    }    
    public void GenerateBlocks()
    {
        if (blockPrefab == null) return;


            for (int i = blockHolder.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(blockHolder.GetChild(i).gameObject);
            }

        int loopCount = Mathf.Min(slots.Count, slotControllers.Count);
        for (int i = 0; i < loopCount; i++)
        {
            SlotSetupData setup = slots[i];
            for (int j = setup.blocks.Count-1; j >= 0; j--)
            {
                BlockSetupData chosenTopic = setup.blocks[j];
                if (chosenTopic == null || chosenTopic.blockTopic == null) continue;

                GameObject newBlock = Instantiate(blockPrefab, blockHolder);

                BlocksManager blocksManager = blockHolder.GetComponent<BlocksManager>();
                
                Sprite spriteToUse = null;
                if (chosenTopic.blockTopic.blocksSprite != null && chosenTopic.blockTopic.blocksSprite.Count > 0)
                {
                    int safeIndex = chosenTopic.indexSprite % chosenTopic.blockTopic.blocksSprite.Count;
                    spriteToUse = chosenTopic.blockTopic.blocksSprite[safeIndex];
                }
                
                newBlock.GetComponent<BlockController>().Setup(blocksManager, chosenTopic.blockTopic.blockColor, chosenTopic.blockTopic, chosenTopic.typeBlock, spriteToUse, slotControllers[i]);

                newBlock.transform.position = slotControllers[i].stackAnchor.position + new Vector3(0, Constants.BLOCK_HEIGHT, 0) * (setup.blocks.Count - 1 - j);
            }
        }

    }
    public void AddBlockToSlot(int slotIndex, BlockTopic blockTopic, int typeBlock)
    {
        if(slots[slotIndex].blocks.Count >= 4)
        {
            Debug.Log("Slot " + slotIndex + " đã có đủ 4 block, không thể thêm nữa");
            return;
        }
        if(amountBlockOfTopic[indexTopicSelected] >= 4)
        {
            Debug.Log("Topic " + blockTopic.topicID + " đã có đủ 4 block, không thể thêm nữa");
            return;
        }
        BlockSetupData newBlockSetup = new BlockSetupData();
        newBlockSetup.blockTopic = blockTopic;
        newBlockSetup.typeBlock = (BlockController.BlockType)typeBlock;
        newBlockSetup.indexSprite = amountBlockOfTopic[indexTopicSelected];
        slots[slotIndex].blocks.Add(newBlockSetup);
        amountBlockOfTopic[indexTopicSelected]++;
    }
    public void RemoveBlockFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        if (slot.blocks.Count == 0) return;

        var block = slot.blocks[slot.blocks.Count - 1];
        for(int i = 0; i < topics.Count; i++)
        {
            if(topics[i].topicID == block.blockTopic.topicID)
            {
                indexTopicSelected = i;
                break;
            }
        }
        slot.blocks.RemoveAt(slot.blocks.Count - 1);
        amountBlockOfTopic[indexTopicSelected]--;
    }
    public void Reset()
    {
        row1 = 0;
        row2 = 0;
        totalTopics = 0;
        topics.Clear();
        slots.Clear();
        amountBlockOfTopic.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        for (int i = blockHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(blockHolder.GetChild(i).gameObject);
        }
    }
    public void SaveLevelData()
    {
        for(int i = 0; i < amountBlockOfTopic.Count; i++)
        {
            if(amountBlockOfTopic[i] < 4)
            {
                Debug.Log("Topic " + topics[i].topicID + " has less than 4 blocks. Please fix it before saving.");
                return;
            }
        }
        LevelDataSO newLevelData = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevelData.level = Level;
        newLevelData.moves = moves;
        newLevelData.row1 = row1;
        newLevelData.row2 = row2;
        newLevelData.numsTopic = topics.Count;
        newLevelData.slots = new List<SlotSetupData>();

        SyncPositionsFromScene();
        foreach(SlotSetupData slot in slots)
        {
            SlotSetupData slotData = new SlotSetupData();
            slotData.slotType = slot.slotType;
            slotData.questionTopic = slot.questionTopic;
            slotData.position = slot.position;
            slotData.blocks = new List<BlockSetupData>();
            foreach(BlockSetupData block in slot.blocks)
            {
                BlockSetupData blockData = new BlockSetupData();
                blockData.blockTopic = block.blockTopic;
                blockData.typeBlock = block.typeBlock;
                blockData.indexSprite = block.indexSprite;
                slotData.blocks.Add(blockData);
            }
            newLevelData.slots.Add(slotData);
        }
        // Save the LevelDataSO to a file or asset
        string assetPath = $"Assets/Resources/Levels/Level_{Level}.asset";
        
        #if UNITY_EDITOR
        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.Refresh();
        Debug.Log("Level saved to " + assetPath);
        #endif
    }

    public void AutoFillTopics()
    {
        int totalBlocks = 0;
        foreach (var slot in slots) {
            totalBlocks += slot.blocks.Count;
        }

        if (totalBlocks == 0) {
            Debug.LogError("No blocks in the level to auto-fill!");
            return;
        }

        if (totalBlocks % 4 != 0) {
            Debug.LogError($"Total blocks ({totalBlocks}) is not a multiple of 4! Please add or remove blocks.");
            return;
        }

        int requiredTopics = totalBlocks / 4;
        
        if (topics.Count < requiredTopics) {
            Debug.LogError($"Not enough topics! Need {requiredTopics} topics, but have {topics.Count}.");
            return;
        }

        for (int i = 0; i < amountBlockOfTopic.Count; i++) amountBlockOfTopic[i] = 0;

        List<int> topicPool = new List<int>();
        for (int i = 0; i < requiredTopics; i++) {
            for (int j = 0; j < 4; j++) {
                topicPool.Add(i);
            }
        }

        for (int i = 0; i < topicPool.Count; i++) {
            int temp = topicPool[i];
            int randomIndex = Random.Range(i, topicPool.Count);
            topicPool[i] = topicPool[randomIndex];
            topicPool[randomIndex] = temp;
        }

        int poolIndex = 0;
        foreach (var slot in slots) {
            for (int b = 0; b < slot.blocks.Count; b++) {
                int topicIdx = topicPool[poolIndex];
                slot.blocks[b].blockTopic = topics[topicIdx];
                slot.blocks[b].indexSprite = amountBlockOfTopic[topicIdx];
                amountBlockOfTopic[topicIdx]++;
                poolIndex++;
            }
        }

        Debug.Log("Auto-filled topics successfully!");
    }

    public void LoadLevel(LevelDataSO levelData)
    {
        if (levelData == null) return;

        Reset();

        this.Level = levelData.level;
        this.moves = levelData.moves;
        this.row1 = levelData.row1;
        this.row2 = levelData.row2;

        HashSet<BlockTopic> uniqueTopics = new HashSet<BlockTopic>();
        foreach(var s in levelData.slots) {
            foreach(var b in s.blocks) {
                if (b.blockTopic != null) uniqueTopics.Add(b.blockTopic);
            }
            if (s.questionTopic != null) uniqueTopics.Add(s.questionTopic);
        }

        this.topics = new List<BlockTopic>(uniqueTopics);
        this.totalTopics = this.topics.Count;

        this.amountBlockOfTopic = new List<int>();
        for(int i = 0; i < this.topics.Count; i++) this.amountBlockOfTopic.Add(0);

        this.slots = new List<SlotSetupData>();
        foreach(var s in levelData.slots) {
            SlotSetupData newSlot = new SlotSetupData();
            newSlot.slotType = s.slotType;
            newSlot.questionTopic = s.questionTopic;
            newSlot.position = s.position;
            
            foreach(var b in s.blocks) {
                BlockSetupData nb = new BlockSetupData();
                nb.typeBlock = b.typeBlock;
                nb.blockTopic = b.blockTopic;
                nb.indexSprite = b.indexSprite;
                newSlot.blocks.Add(nb);
                
                int tIdx = this.topics.IndexOf(b.blockTopic);
                if (tIdx >= 0) this.amountBlockOfTopic[tIdx]++;
            }
            this.slots.Add(newSlot);
        }

        UpdateSlotsInEditor();
        GenerateBlocks();
        Debug.Log("Level loaded successfully!");
    }
}

