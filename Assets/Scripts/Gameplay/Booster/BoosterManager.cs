using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BoosterManager : MonoBehaviour
{
    [Header("Booster References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    // [SerializeField] private Transform gridRoot;
    [SerializeField] private AddMoveBooster addMove;
    [SerializeField] private HintBooster hint;
    [SerializeField] private ShuffleBooster suffle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        addMove.Setup(this);
        hint.Setup(this);
        suffle.Setup(this);
        visitedBlock = new Dictionary<int, List<BlockController>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ShuffleBlock()
    {
        List<BlockController> diffcultBLocks = new List<BlockController>();
        Dictionary<int, List<BlockController>> sameBlocks = new Dictionary<int, List<BlockController>>();

        foreach(SlotController slot in slotsManager.GetAllSlots())
        {
            if(slot.isFinished || !slot.isRevealed || slot.slotType == SlotController.SlotType.Ice) continue;
            
            slot.MoveToShuffle(slotsManager.gameObject.transform,diffcultBLocks,sameBlocks);
        }

        yield return new WaitForSeconds(1f);
        

        List<SlotController> randomSlots = new List<SlotController>(slotsManager.GetAllSlots());

        ShuffleList(randomSlots);

        ShuffleList(diffcultBLocks);

        // List<BlockController> easyBlocks = new List<BlockController>();

        foreach(var kvp in sameBlocks) 
        {
            diffcultBLocks.AddRange(kvp.Value);
        }

        int index = 0;

        while(index < diffcultBLocks.Count)
        {
            for(int i = 0; i < randomSlots.Count; i++)
            {
                if(index >= diffcultBLocks.Count) break;

                if(randomSlots[i].blocks.Count >= 4 || !randomSlots[i].isRevealed || randomSlots[i].slotType == SlotController.SlotType.Ice) continue;

                BlockController block = diffcultBLocks[index];

                randomSlots[i].MoveToSlot(block);

                index++;
            }
        }
    }   
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    // Hint Booster
    private Dictionary<int, List<BlockController>> visitedBlock = new Dictionary<int, List<BlockController>>();
    [Header("Hint References")]
    [SerializeField] private GameObject hintImage1;
    [SerializeField] private GameObject hintImage2;
    public bool SearchedBlocks()
    {
        // Setup dữ liệu để bắt đầu tìm kiếm.
        Dictionary<int, List<BlockController>> blocksByTopicID = new Dictionary<int, List<BlockController>>();

        List<SlotController> randomSlot = new List<SlotController>();

        foreach(SlotController slot in slotsManager.GetAllSlots())
        {
            if(slot.isFinished || !slot.isRevealed) continue;
            randomSlot.Add(slot);
        }

        ShuffleList(randomSlot);

        foreach(SlotController slot in randomSlot)
        {
            foreach(BlockController block in slot.blocks)
            {
                if(!block.isRevealed) continue;

                if(visitedBlock.ContainsKey(block.GetTopicID()))
                {
                    if(visitedBlock[block.GetTopicID()].Contains(block)) continue;
                }

                if(blocksByTopicID.ContainsKey(block.GetTopicID())) blocksByTopicID[block.GetTopicID()].Add(block);
                else
                {
                    List<BlockController> blocks = new List<BlockController>();
                    blocks.Add(block);
                    blocksByTopicID.Add(block.GetTopicID(), blocks);
                }
            }
        }
        // Tìm kiếm các cặp block giống nhau.
        foreach((int topicID, List<BlockController> blocks) in blocksByTopicID)
        {
            if(blocks.Count < 2) continue;
            
            StartCoroutine(HintCoroutine(0.5f, hintImage1, hintImage2, blocks[0], blocks[1]));

            if(visitedBlock.ContainsKey(topicID))
            {
                visitedBlock[topicID].Add(blocks[0]);
                visitedBlock[topicID].Add(blocks[1]);
            }
            else
            {
                List<BlockController> blockControllers = new List<BlockController>();
                blockControllers.Add(blocks[0]);
                blockControllers.Add(blocks[1]);
                visitedBlock.Add(topicID, blockControllers);
            }
            return true;
        }
        // Tìm block đơn lẻ mà topic của nó đã được tìm kiếm trước đó khi không có cặp block nào
        foreach((int topicID, List<BlockController> blocks) in blocksByTopicID)
        {
            if(visitedBlock.ContainsKey(topicID))
            {
                visitedBlock[topicID].Add(blocks[0]);
                
                StartCoroutine(HintCoroutine(0.5f,hintImage1, hintImage2, blocks[0]));
                return true;
            }
        }

        return false;
        
    }
    public IEnumerator HintCoroutine(float time, GameObject hintImage1, GameObject hintImage2, BlockController block1, BlockController block2 = null)
    {
        hintImage1.transform.position = mainCamera.WorldToScreenPoint(block1.transform.position);
        hintImage1.SetActive(true);
        if(block2 != null)
        {
            hintImage2.transform.position = mainCamera.WorldToScreenPoint(block2.transform.position);
            hintImage2.SetActive(true);
        } 

        yield return new WaitForSeconds(time);

        block1.SetColorOutLine();
        if(block2 != null) block2.SetColorOutLine();

        hintImage1.SetActive(false);
        hintImage2.SetActive(false);
    }
}
