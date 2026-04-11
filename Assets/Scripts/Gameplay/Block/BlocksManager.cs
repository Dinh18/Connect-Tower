using System.Collections.Generic;
using UnityEngine;

public class BlocksManager : MonoBehaviour
{
    private Dictionary<int, List<BlockController>> blocksByTopicID = new Dictionary<int, List<BlockController>>();
    public Dictionary<int, List<BlockController>> GetAllBlocks() => blocksByTopicID;
    private Stack<GameObject> blockPool = new Stack<GameObject>();
    private GameObject blockObj;
    private Dictionary<string, Material> blockMaterialCache = new Dictionary<string, Material>();
    void Awake()
    {
        blockObj = Resources.Load<GameObject>(Constants.BLOCK_TEST_1_PATH);
    }
    public void PoolBlock(int numsBlock)
    {
        for(int i = 0; i < numsBlock; i++)
        {
            GameObject block = Instantiate(blockObj, this.transform);
            block.SetActive(false);
            blockPool.Push(block);
        }
    }

    public Material GetMaterial(string materialPath)
    {
        if(!blockMaterialCache.ContainsKey(materialPath))
        {
            Material material = Resources.Load<Material>(materialPath);
            blockMaterialCache[materialPath] = material;
        }
        return blockMaterialCache[materialPath];
    }
    public void BlocksGenerate(List<SlotSetupData> slotSetups, List<SlotController> slots)
    {
        blocksByTopicID.Clear();
        foreach(Transform child in this.transform)
        {
            if(child.gameObject.activeSelf)
            {    
                child.gameObject.SetActive(false);
                blockPool.Push(child.gameObject);
            }
        }
        
        for(int i = 0; i < slots.Count; i++)
        {
            for(int j = slotSetups[i].blocks.Count - 1; j >= 0; j--)
            {
                GameObject block;
                if(blockPool.Count <= 0)
                {
                    block = Instantiate(blockObj, this.transform);
                }
                else
                {
                    block = blockPool.Pop();
                } 
                block.SetActive(true);
                block.transform.position = slots[i].stackAnchor.position + new Vector3(0, Constants.BLOCK_HEIGHT, 0) * (slotSetups[i].blocks.Count - 1 - j);

                BlockController b = block.GetComponent<BlockController>();
                ItemImageBlock itemImageBlock = block.GetComponentInChildren<ItemImageBlock>();
                
                itemImageBlock.AddImage(slotSetups[i].blocks[j].blockTopic.blocksSprite[0]);
                b.Setup(this,slotSetups[i].blocks[j].blockTopic.blockColor, 
                    slotSetups[i].blocks[j].blockTopic, 
                    slotSetups[i].blocks[j].typeBlock,
                    slotSetups[i].blocks[j].blockTopic.blocksSprite[slotSetups[i].blocks[j].indexSprite],
                    slots[i]);
                if(blocksByTopicID.ContainsKey(b.GetTopicID()))
                {
                    blocksByTopicID[b.GetTopicID()].Add(b);
                }
                else
                {
                    List<BlockController> blockControllers = new List<BlockController>();
                    blockControllers.Add(b);
                    blocksByTopicID.Add(b.GetTopicID(), blockControllers);
                }
                // if(block.GetComponent<Block>()) Debug.Log("Block is null");
                slots[i].blocks.Push(b);
            }
        }
    }
}
