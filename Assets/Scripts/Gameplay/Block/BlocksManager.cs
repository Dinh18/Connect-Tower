using System.Collections.Generic;
using UnityEngine;

public class BlocksManager : MonoBehaviour
{
    private Dictionary<int, List<BlockController>> blocksByTopicID = new Dictionary<int, List<BlockController>>();
    public Dictionary<int, List<BlockController>> GetAllBlocks() => blocksByTopicID;
    private List<GameObject> blockPool;
    public void PoolBlock(int numsBlock)
    {
        blockPool = new List<GameObject>();
        GameObject blockObj = Resources.Load<GameObject>(Constants.BLOCK_TEST_1_PATH);
        for(int i = 0; i < numsBlock; i++)
        {
            GameObject block = Instantiate(blockObj, this.transform.position,Quaternion.identity, this.transform);
            block.SetActive(false);
        }
    }
    public void BlocksGenerate(List<SlotSetupData> slotSetups, List<SlotController> slots)
    {
        foreach(Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject blockObj = Resources.Load<GameObject>(Constants.BLOCK_TEST_1_PATH);
        
        for(int i = 0; i < slots.Count; i++)
        {
            for(int j = slotSetups[i].blocks.Count - 1; j >= 0; j--)
            {
                GameObject block = GameObject.Instantiate(blockObj, 
                                                        slots[i].stackAnchor.position + new Vector3(0, Constants.BLOCK_HEIGHT, 0) * (slotSetups[i].blocks.Count - 1 - j), 
                                                        Quaternion.identity);
                block.transform.SetParent(this.transform);

                BlockController b = block.GetComponent<BlockController>();
                ItemImageBlock itemImageBlock = block.GetComponentInChildren<ItemImageBlock>();
                
                itemImageBlock.AddImage(slotSetups[i].blocks[j].blockTopic.blocksSprite[0]);
                b.Setup(slotSetups[i].blocks[j].blockTopic.blockColor, 
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
