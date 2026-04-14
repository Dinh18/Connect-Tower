using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public enum SlotType
    {
        Normal,
        Hide,
        Ice
    }
    public Stack<BlockController> blocks;
    public GameObject blockPrefab;
    public Transform stackAnchor;
    public Transform arcPeak;
    // [SerializeField] private int amountOfBlocks = 2;
    [Header("Movement Settings")]
    [SerializeField] private float height = 0.7f;
    // [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float selectDuration = 0.1f;
    public bool isFinished = false;
    public SlotType slotType;
    private int movingBlocksCount = 0;
    private bool isMoving = false;
    [Header("Header Settings")]
    [SerializeField] private HeaderSlot header;
    [SerializeField] private SlotVFX slotVFX;
    [Header("Hide Slot Settings")]
    // [SerializeField] private Sprite hideSlotSprite;
    [SerializeField] GameObject hideSlotHolder;
    [SerializeField] private Image itemImage;
    public bool isRevealed;
    public BlockTopic blockTopic = null;
    [Header("Ice Slot Settings")]
    [SerializeField] private GameObject BaseSlot;
    [SerializeField] private GameObject iceRod;
    [SerializeField] private GameObject iceVFX;
    private float baseIceRodLocalY;
    private int row;
    private int blocksToMove;
    [SerializeField] private float delayBetweenBlocks = 0.1f;
    public static event Action<int> OnSlotCompleted;
    public static event Action<bool> OnMoveFisnished;
    public void Setup(SlotType slotType, int row, BlockTopic blockTopic = null)
    {
        this.row = row;
        blocks = new Stack<BlockController>();
        this.slotType = slotType;
        isFinished = false;
        iceVFX.SetActive(false);
        if(blockTopic != null) this.blockTopic = blockTopic;
        if(header != null) header.Setup(this);
        if(slotVFX != null) slotVFX.Setup();
        if(slotType == SlotType.Hide)
        {
            isRevealed = false;
            // hideSlotImage.sprite = hideSlotSprite;
            hideSlotHolder.SetActive(true);
            itemImage.sprite = blockTopic.blocksSprite[0];
        } 
        else isRevealed = true;
    }

    public void SetupIceSlot()
    {
        if(slotType == SlotType.Ice)
        {
            BaseSlot.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>(Constants.MESH_ICE_BASE_PATH);
            BaseSlot.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Constants.MATERIAL_ICE_PATH);

            iceRod.SetActive(true);

            iceVFX.SetActive(true);

             if(blocks.Count > 0)
            {
                iceRod.transform.position = new Vector3(iceRod.transform.position.x, blocks.Count * Constants.BLOCK_HEIGHT + Constants.BLOCK_HEIGHT + Constants.SLOT_HEIGHT * row, iceRod.transform.position.z);
            }
        }
    }

    public bool SelectToMove()
    {
        if(isMoving || isFinished || GameManager.Instance.GetCurrState() == GameManager.GameState.Pause
        || GameManager.Instance.GetCurrState() == GameManager.GameState.Lose
        || !isRevealed || slotType == SlotType.Ice) return false;
        int topicID = blocks.Peek().GetComponent<BlockController>().GetTopicID();
        int i = 0;
        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID 
                || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.Selected);
            Vector3 targetPosition = new Vector3(stackAnchor.position.x, stackAnchor.position.y + (blocks.Count - i) * Constants.BLOCK_HEIGHT, stackAnchor.position.z);
            block.DOKill();
            block.transform.DOMove(targetPosition, selectDuration).SetEase(Ease.OutQuad);
            i++;
        }
        return true;
    }

    /// <param name="otherSlot">SLot bắt đầu</param>
    /// <summary>
    /// - Logic:
    ///     + số lượng các block được di chuyển là số lượng block được chọn nếu <= số lượng block còn trống ở slot địch
    ///         ngược lại là số lượng block còn trống ở block đích
    ///     + các block có thể di chuyển được pop ra khỏi stack của slot đầu và push vào stack của slot đích
    ///     + các block không được di chuyển thì không được pop ra khỏi stack
    /// - Visual:
    ///     + các block sẽ được di chuyển theo đường đi: điểm bắt đầu là đỉnh của slot đầu,
    ///         điểm tiếp theo là final peak có y là điểm cao nhất của 2 slot,
    ///         điểm tiếp theo có thể là đỉnh của slot đích nếu nó thấp hơn đỉnh của slot đầu,
    ///         điểm cuối cùng là vị trí mới của block được tính toán dựa theo số lượng block đã có trong slot đích.
    /// </summary>

    public bool SelectToRecive(SlotController otherSlot)
    {
        if(isMoving || isFinished || GameManager.Instance.GetCurrState() == GameManager.GameState.Pause
            || GameManager.Instance.GetCurrState() == GameManager.GameState.Lose
                || !isRevealed) return false;
        if(slotType == SlotType.Ice)
        {
            if(blocks.Count > 0)
            {
                if(otherSlot.blocks.Peek().GetTopicID() != this.blocks.Peek().GetTopicID())
                {
                    foreach(BlockController block in otherSlot.blocks)
                    {
                        if(block.GetCurrState() == BlockController.BlockState.Selected)
                        {
                            block.PlayErrorShake();
                        }
                    }
                    return false;
                } 
                
            }
        }
            // Loại block di chuyển
        bool isSameType = false;
        bool isSlotEmpty = false;
        if(this.blocks.Count > 0)
        {    
            BlockController peekThisSLot = this.blocks.Peek();
            BlockController peekOtherSlot = otherSlot.blocks.Peek();
            if(peekThisSLot.GetTopicID() == peekOtherSlot.GetTopicID()) isSameType = true;
        }
        else isSlotEmpty = true;
        int topicID = otherSlot.blocks.Peek().GetComponent<BlockController>().GetTopicID();
        // Số lượng block có thể di chuyển
        int blockCount = 0;
        foreach(BlockController block in otherSlot.blocks)
        {
            if(block.GetTopicID()!= topicID 
                || !block.isRevealed) break;
            blockCount++;
        }
        blocksToMove = Math.Min(4 - blocks.Count, blockCount);
        // Vị trí bắt đầu của block đầu tiên được di chuyển
        float startY = (blocks.Count == 0) ? stackAnchor.position.y : blocks.Peek().transform.position.y + height;


        for(int i = 0;i < blocksToMove; i++)
        {
            // Pop block từ slot đầu
            BlockController block = otherSlot.blocks.Pop();
            block.ChangeState(BlockController.BlockState.None); 

            // Tính toán đường đi cho block
            float finalPeakX = (otherSlot.arcPeak.position.y < this.arcPeak.position.y) ? otherSlot.arcPeak.position.x : this.arcPeak.position.x;
            float finalPeakY = Mathf.Max(otherSlot.arcPeak.position.y, this.arcPeak.position.y);
            float finalPeakZ = (otherSlot.arcPeak.position.y < this.arcPeak.position.y) ? otherSlot.arcPeak.position.z : this.arcPeak.position.z;
            Vector3 finalPeak = new Vector3(finalPeakX, finalPeakY, finalPeakZ); 

            Vector3 finalDestination = new Vector3(this.stackAnchor.position.x, startY + height * i, this.stackAnchor.position.z);    
            List<Vector3> path = new List<Vector3>{
                // block.transform.position,
                otherSlot.arcPeak.position,
                // this.arcPeak.position,
                finalPeak,
                finalDestination
            };
            if(otherSlot.arcPeak.position.y < this.arcPeak.position.y) path.Insert(2, this.arcPeak.position);
            // Push block vào slot đích
            blocks.Push(block);
            // block.transform.SetParent(this.stackAnchor);
            // Di chuyển block theo đường đi đã tính toán
            float delay = i * delayBetweenBlocks;
            MoveBlockSmoothly(block.gameObject, path, moveDuration, otherSlot, isSameType, isSlotEmpty, delay);     
        }
        // if(blocks.Count == 4) CheckSlotComplete();
        // Di chuyển các block còn lại ở slot đầu về vị trí ban đầu
        int j = 0;
        foreach (BlockController block in otherSlot.blocks)
        {
            if(block.GetTopicID() != topicID 
                || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.None);
            Vector3 targetPosition = new Vector3(otherSlot.stackAnchor.position.x, otherSlot.stackAnchor.position.y + (otherSlot.blocks.Count - 1 - j) * Constants.BLOCK_HEIGHT, otherSlot.stackAnchor.position.z);
            MoveBlockSmoothly(block.gameObject, new List<Vector3> { targetPosition }, selectDuration);
            j++;
        }
        return true;
    }
    public bool UnSelect()
    {
        if(isMoving || isFinished || GameManager.Instance.GetCurrState() == GameManager.GameState.Pause
        || GameManager.Instance.GetCurrState() == GameManager.GameState.Lose
        || !isRevealed) return false;
        int topicID = blocks.Peek().GetComponent<BlockController>().GetTopicID();
        int i = 0;
        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID 
                || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.None);
            Vector3 targetPosition = new Vector3(stackAnchor.position.x, stackAnchor.position.y + (blocks.Count - 1 - i) * Constants.BLOCK_HEIGHT, stackAnchor.position.z);
            // MoveBlockSmoothly(block.gameObject, new List<Vector3>{targetPosition}, selectDuration);
            block.transform.DOMove(targetPosition, selectDuration).SetEase(Ease.OutQuad);
            i++;
        }
        return true;
    }
    // Di chuyển block mượt mà theo đường đi đã tính toán
    private void MoveBlockSmoothly(GameObject block, List<Vector3> path, float duration, SlotController slot = null, bool isSameType = false, bool isSlotEmpty = false, float delay = 0f)
    {
        BlockStartMoving();
        Vector3[] pathArr = path.ToArray();

        block.transform.DOKill();

        block.transform.DOPath(pathArr, duration, PathType.CatmullRom)
            .SetDelay(delay)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => 
            {
                // Gọi hàm này khi DOTween chạy xong
                BlockReachedDestination(block.GetComponent<BlockController>(),slot, isSameType, isSlotEmpty); 
            });
    }


    private void CheckSlotComplete()
    {
        int topicID = blocks.Peek().GetTopicID();
        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID 
                || !block.isRevealed) return;
        }
        isFinished = true;
        AudioManager.Instance.PlaySlotFinishedAudio();
        if(slotType == SlotType.Ice)
        {
            iceRod.SetActive(false);
            iceVFX.SetActive(false);
            BaseSlot.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>(Constants.MESH_BASE_PATH);
            BaseSlot.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Constants.MATERIAL_BASE_PATH);
            AudioManager.Instance.PlayBlockIceFinishedAudio();
        } 
        foreach(BlockController block in blocks)
        {
            block.Finished(this);
            block.HideIceImage();
        }
        header.Show();
        slotVFX.PlayVFX();
        Debug.Log("Slot Completed");
        OnSlotCompleted?.Invoke(blocks.Peek().GetTopicID());
    }

    private void BlockStartMoving()
    {
        movingBlocksCount++;
        AudioManager.Instance.PlayMoveWooshAudio();
        isMoving = true;
    }
    private void BlockReachedDestination(BlockController b,SlotController otherSlot, bool isSameType, bool isSlotEmpty)
    {
        
        movingBlocksCount--;
        if(slotType == SlotType.Ice)
        {
            b.ShowIceImage();
        }
        if(movingBlocksCount <= 0)
        {
            if(isSameType)
            {
                int topicID = blocks.Peek().GetTopicID();
                int i = 0;
                foreach(BlockController block in blocks)
                {
                    if(block.GetTopicID() != topicID) break;
                    block.ChangeState(BlockController.BlockState.Collde);
                    block.FallEffect(i);
                    i++;
                }
                AudioManager.Instance.PlayPopMovedAudio(i);
                StartCoroutine(HapticManager.Instance.PlayVibrateBlockFall(i, 0.2f));
            } 
            else
            {
                AudioManager.Instance.PlayBlockFailAudio();
                HapticManager.Instance.PlayVibrateHeavy();
                // b.PlayErrorShake();
                int i = 0;
                foreach(BlockController block in blocks)
                {
                    if(i >= blocksToMove) break;
                    block.PlayErrorShake();
                    i++;
                }
                if(!isSameType) b.PlayDifVFX();
            }
            movingBlocksCount = 0;

            if(slotType == SlotType.Ice)
            {
                
                // float startLocalY = (blocks.Count * Constants.BLOCK_HEIGHT) + Constants.BLOCK_HEIGHT;
                // iceRod.transform.DOMove(new Vector3(iceRod.transform.position.x,blocks.Count * Constants.BLOCK_HEIGHT + Constants.BLOCK_HEIGHT, iceRod.transform.position.z), 0.5f);
                iceRod.transform.DOMove(new Vector3(iceRod.transform.position.x,
                                        blocks.Count * Constants.BLOCK_HEIGHT + Constants.BLOCK_HEIGHT + Constants.SLOT_HEIGHT * row, 
                                        iceRod.transform.position.z), 0.5f);
                AudioManager.Instance.PlayFreezeUpAudio();
            } 

            if (otherSlot != null && otherSlot.blocks.Count > 0)
                {
                    if (!otherSlot.blocks.Peek().isRevealed) 
                    {
                        otherSlot.blocks.Peek().Reveal();   
                    }
                }


        //     float delay = 0.5f;

        //    StartCoroutine(WaitAndFinishMoving(delay, otherSlot));

            isMoving = false;
            OnMoveFisnished?.Invoke(isMoving);
            
            if(blocks.Count == 4) CheckSlotComplete();
        } 
    }
    private IEnumerator WaitAndFinishMoving(float delay, SlotController otherSlot)
    {
        yield return new WaitForSeconds(delay);
        isMoving = false;
        OnMoveFisnished?.Invoke(isMoving);
    }
    public void Reveal()
    {
        isRevealed = true;
        // hideSlotHolder.SetActive(false);
        Vector3 originPos = hideSlotHolder.transform.position;
        Quaternion originRot = hideSlotHolder.transform.rotation;

        Sequence sequence = DOTween.Sequence();
        AudioManager.Instance.PlayClothAudio();
        sequence.Append(hideSlotHolder.transform.DOMoveY(originPos.y + 10f, 1f));

        sequence.Join(hideSlotHolder.transform.DORotate(new Vector3(0, 0, 15f), 0.5f));

        sequence.OnComplete(() =>
        {
            hideSlotHolder.SetActive(false);

            hideSlotHolder.transform.position = originPos;
            hideSlotHolder.transform.rotation = originRot;
        });




        // itemImage.gameObject.SetActive(true);
    }

    public void MoveToShuffle(Transform gridRoot, List<BlockController> diffcultBlocks, Dictionary<int, List<BlockController>> sameBlocks)
    {
        while(blocks.Count > 0)
        {
            BlockController block = blocks.Pop();
            if(!block.isRevealed)
            {
                blocks.Push(block);
                return;
            }
            Vector3[] pathArr = new Vector3[] {arcPeak.transform.position, new Vector3 (gridRoot.position.x, gridRoot.position.y + Constants.SLOT_HEIGHT - Constants.BLOCK_HEIGHT, gridRoot.position.z)};
            block.transform.DOPath(pathArr, 0.5f, PathType.CatmullRom);
            if(sameBlocks.ContainsKey(block.GetTopicID()))
            {
                if(sameBlocks[block.GetTopicID()].Count < 2) sameBlocks[block.GetTopicID()].Add(block);
                else diffcultBlocks.Add(block);
            }
            else
            {
                if(sameBlocks.Count < 3)
                {
                    List<BlockController> blocks = new List<BlockController>();
                    blocks.Add(block);
                    sameBlocks.Add(block.GetTopicID(), blocks);
                }
                else
                {
                    diffcultBlocks.Add(block);
                }
            }
        }
    }

    public void MoveToSlot(BlockController block)
    {
        Vector3 destination = new Vector3(stackAnchor.position.x,
                                          stackAnchor.position.y + Constants.BLOCK_HEIGHT * blocks.Count,
                                          stackAnchor.position.z);
        List<Vector3> path = new List<Vector3>{arcPeak.position, destination};
        block.transform.DOPath(path.ToArray(), 0.5f, PathType.CatmullRom);
        blocks.Push(block);
    }
}
