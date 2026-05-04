using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using System.Linq;
using DG.Tweening;

public class BoosterManager : MonoBehaviour
{
    [Header("Booster References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private Transform centerPivot;
    
    // Biến Singleton để các Booster tự tìm đến
    public static BoosterManager Instance { get; private set; }

    private List<IBooster> activeBoosters = new List<IBooster>();
    private Dictionary<int, List<BlockController>> visitedBlock = new Dictionary<int, List<BlockController>>();

    void Awake()
    {
        Instance = this;
    }

    // Cơ chế Self-Registration (Tự điểm danh) - Không tốn 1 chút hiệu năng quét Scene nào!
    public void RegisterBooster(IBooster booster)
    {
        if (!activeBoosters.Contains(booster))
        {
            booster.Setup(this);
            activeBoosters.Add(booster);
        }
    }

    public void ShuffleBlock()
    {
        List<BlockController> diffcultBLocks = new List<BlockController>();
        Dictionary<int, List<BlockController>> sameBlocks = new Dictionary<int, List<BlockController>>();
        Sequence sequence = DOTween.Sequence();
        //----------------Đưa các block về 1 điểm----------------
        int moveIndex = 0;
        float moveDuration = 0.4f;
        foreach(SlotController slot in slotsManager.GetAllSlots())
        {
            if(slot.isFinished || !slot.isRevealed || slot.slotType == SlotController.SlotType.Ice) continue;
            
            List<BlockController> poppedBlocks = slot.MoveToShuffle(diffcultBLocks, sameBlocks);
            foreach(BlockController block in poppedBlocks)
            {
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 1.5f;
                Vector3 gatherPos = centerPivot.position + randomOffset;
                Vector3[] pathArr = new Vector3[] {slot.arcPeak.transform.position, gatherPos};
                
                block.transform.SetParent(centerPivot);
                block.transform.DOKill();
                sequence.Insert(moveIndex * 0.02f, block.transform.DOPath(pathArr, moveDuration, PathType.CatmullRom).SetEase(Ease.InBack));
                moveIndex++;
            }
        }

        //----------------Xoay tâm để tạo hiệu ứng đã xáo trộn----------------
        float spinDuration = 1f;
        float spinDelay = moveIndex * 0.02f + moveDuration + 0.1f;
        int rounds = 3;
        sequence.Insert(spinDelay,centerPivot.DORotate(new Vector3(0,0,360 * rounds),spinDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));

        //----------------Rải các block ra lại các slot----------------
        List<SlotController> randomSlots = new List<SlotController>(slotsManager.GetAllSlots());
        ShuffleList(randomSlots);
        ShuffleList(diffcultBLocks);

        foreach(var kvp in sameBlocks) 
        {
            diffcultBLocks.AddRange(kvp.Value);
        }

        int index = 0;
        int safeCounter = 0; // Luôn nhớ thêm cái này để chống treo game nhé
        Debug.Log($"Total blocks to shuffle: {diffcultBLocks.Count}");

        // TÍNH TOÁN TIMELINE CHUẨN:
        // Tâm bắt đầu quay ở 1.0s, quay mất 0.4s -> Xong ở 1.4s.
        // Ta cho nghỉ thêm 0.1s rồi mới bắt đầu rơi -> dropStartTime = 1.5f.
        float dropStartTime = spinDelay + spinDuration + 0.1f;
        float duration = 0.5f;

        while(index < diffcultBLocks.Count)
        {
            safeCounter++;
            if (safeCounter > 1000) break;

            Debug.Log($"Trying to place block {diffcultBLocks[index].name} at index {index}");
            for(int i = 0; i < randomSlots.Count; i++)
            {
                if(index >= diffcultBLocks.Count) break;

                if(randomSlots[i].blocks.Count >= 4 || !randomSlots[i].isRevealed || randomSlots[i].slotType == SlotController.SlotType.Ice) continue;

                BlockController block = diffcultBLocks[index];
                SlotController slot = randomSlots[i];
                
                Vector3 destination = new Vector3(slot.stackAnchor.position.x,
                                                slot.stackAnchor.position.y + Constants.BLOCK_HEIGHT * slot.blocks.Count,
                                                slot.stackAnchor.position.z);
                List<Vector3> path = new List<Vector3>{slot.arcPeak.position, destination};
                
                

                // TÍNH THỜI ĐIỂM TUYỆT ĐỐI CHO TỪNG KHỐI TRÊN TIMELINE
                // Bằng = Thời điểm bắt đầu rơi (1.5s) + Độ trễ lượn sóng (0.05s mỗi khối)
                float absoluteDropTime = dropStartTime + (index * 0.02f); 

                // Gắn Callback đúng vào cái absoluteDropTime này
                sequence.InsertCallback(absoluteDropTime, () => {
                    block.transform.SetParent(CoreServices.Get<BlocksManager>().transform);
                    block.transform.DOKill(); 
                });
                
                // Chạy animation rơi cũng từ đúng cái absoluteDropTime này
                sequence.Insert(absoluteDropTime, block.transform.DOPath(path.ToArray(), duration, PathType.CatmullRom).SetEase(Ease.OutQuad));

                slot.blocks.Push(block);

                index++;
            }
        }

        // Tùy chọn: Reset góc quay khi tất cả đã xong (Tính tổng thời gian thả cục cuối cùng)
        sequence.OnComplete(() => {
            centerPivot.rotation = Quaternion.identity;
            Debug.Log("Shuffle Animation Hoàn Tất!");
        });
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

    [Header("Hint References")]
    [SerializeField] private GameObject hintImage1;
    [SerializeField] private GameObject hintImage2;

    public bool SearchedBlocks()
    {
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

        foreach((int topicID, List<BlockController> blocks) in blocksByTopicID)
        {
            if(visitedBlock.ContainsKey(topicID))
            {
                visitedBlock[topicID].Add(blocks[0]);
                StartCoroutine(HintCoroutine(0.5f, hintImage1, hintImage2, blocks[0]));
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
