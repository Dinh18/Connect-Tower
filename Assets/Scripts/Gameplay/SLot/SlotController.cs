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
    public Stack<BlockController> blocks{get; private set;}
    public GameObject blockPrefab;
    public Transform stackAnchor;
    public Transform arcPeak;
    
    [Header("Movement Settings")]
    [SerializeField] private float height = 0.7f;
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

    // --- STATIC CACHE CHO PERFORMANCE ---
    private static Mesh iceMeshCache;
    private static Material iceMaterialCache;
    private static Mesh baseMeshCache;
    private static Material baseMaterialCache;
    // ------------------------------------

    private MeshFilter baseMeshFilter;
    private MeshRenderer baseMeshRenderer;
    private GameManager gameManager;

    void Awake()
    {
        if (BaseSlot != null)
        {
            baseMeshFilter = BaseSlot.GetComponent<MeshFilter>();
            baseMeshRenderer = BaseSlot.GetComponent<MeshRenderer>();
        }
    }

    void Start()
    {
        gameManager = CoreServices.Get<GameManager>();
    }

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
            hideSlotHolder.SetActive(true);
            itemImage.sprite = blockTopic.blocksSprite[0];
        } 
        else isRevealed = true;
    }

    public void SetupIceSlot()
    {
        if(slotType == SlotType.Ice)
        {
            // Tối ưu: Dùng cache thay vì Load lại mỗi lần
            if (iceMeshCache == null) iceMeshCache = Resources.Load<Mesh>(Constants.MESH_ICE_BASE_PATH);
            if (iceMaterialCache == null) iceMaterialCache = Resources.Load<Material>(Constants.MATERIAL_ICE_PATH);

            if (baseMeshFilter != null) baseMeshFilter.mesh = iceMeshCache;
            if (baseMeshRenderer != null) baseMeshRenderer.material = iceMaterialCache;

            iceRod.SetActive(true);
            iceVFX.SetActive(true);

             if(blocks.Count > 0)
            {
                iceRod.transform.position = new Vector3(iceRod.transform.position.x, blocks.Count * Constants.BLOCK_HEIGHT + Constants.BLOCK_HEIGHT + Constants.SLOT_HEIGHT * row, iceRod.transform.position.z);
            }
            else
            {
                iceRod.transform.localPosition = new Vector3(0,3.82f,0); 
            }
        }
    }

    public bool SelectToMove()
    {
        if(isMoving || isFinished || gameManager.GetCurrState() == GameManager.GameState.Pause
        || gameManager.GetCurrState() == GameManager.GameState.Lose
        || !isRevealed || slotType == SlotType.Ice) return false;
        
        int topicID = blocks.Peek().GetTopicID();
        int i = 0;

        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.Selected);
            Vector3 targetPosition = new Vector3(stackAnchor.position.x, stackAnchor.position.y + (blocks.Count - i) * Constants.BLOCK_HEIGHT, stackAnchor.position.z);
            block.transform.DOKill();
            block.transform.DOMove(targetPosition, selectDuration).SetEase(Ease.OutQuad);
            i++;
        }
        AudioManager.Instance.PlaySelectSlotAudio();

        return true;
    }

    public bool SelectToRecive(SlotController otherSlot)
    {
        if(isMoving || isFinished || gameManager.GetCurrState() == GameManager.GameState.Pause
            || gameManager.GetCurrState() == GameManager.GameState.Lose
            || !isRevealed) return false;
            
        if(slotType == SlotType.Ice)
        {
            if(blocks.Count > 0)
            {
                if(otherSlot.blocks.Peek().GetTopicID() != this.blocks.Peek().GetTopicID())
                {
                    AudioManager.Instance.PlayMoveFailAudio();
                    foreach(BlockController block in otherSlot.blocks)
                    {
                        if(block.GetCurrState() == BlockController.BlockState.Selected)
                        {
                            block.PlayErrorShake(block.SelectedEffect);
                        }
                    }
                    return false;
                } 
            }
        }
        
        bool isSameType = false;
        bool isSlotEmpty = false;
        if(this.blocks.Count > 0)
        {    
            BlockController peekThisSLot = this.blocks.Peek();
            BlockController peekOtherSlot = otherSlot.blocks.Peek();
            if(peekThisSLot.GetTopicID() == peekOtherSlot.GetTopicID()) isSameType = true;
        }
        else isSlotEmpty = true;
        
        int topicID = otherSlot.blocks.Peek().GetTopicID();
        int blockCount = 0;
        foreach(BlockController block in otherSlot.blocks)
        {
            if(block.GetTopicID()!= topicID || !block.isRevealed) break;
            blockCount++;
        }
        
        blocksToMove = Math.Min(4 - blocks.Count, blockCount);
        if(blocksToMove <= 0)
        {
            AudioManager.Instance.PlayMoveFailAudio();
            int i = 0;
            foreach(var block in otherSlot.blocks)
            {
                if(i >= blockCount) break;
                block.PlayErrorShake(block.SelectedEffect);
                i++;
            }
            return false;
        }
        
        float startY = (blocks.Count == 0) ? stackAnchor.position.y : blocks.Peek().transform.position.y + height;

        for(int i = 0;i < blocksToMove; i++)
        {
            BlockController block = otherSlot.blocks.Pop();
            block.ChangeState(BlockController.BlockState.None); 

            float finalPeakX = (otherSlot.arcPeak.position.y < this.arcPeak.position.y) ? otherSlot.arcPeak.position.x : this.arcPeak.position.x;
            float finalPeakY = Mathf.Max(otherSlot.arcPeak.position.y, this.arcPeak.position.y);
            float finalPeakZ = (otherSlot.arcPeak.position.y < this.arcPeak.position.y) ? otherSlot.arcPeak.position.z : this.arcPeak.position.z;
            Vector3 finalPeak = new Vector3(finalPeakX, finalPeakY, finalPeakZ); 

            Vector3 finalDestination = new Vector3(this.stackAnchor.position.x, startY + height * i, this.stackAnchor.position.z);    
            List<Vector3> path = new List<Vector3>{
                otherSlot.arcPeak.position,
                finalPeak,
                finalDestination
            };
            if(otherSlot.arcPeak.position.y < this.arcPeak.position.y) path.Insert(2, this.arcPeak.position);
            
            blocks.Push(block);
            float delay = i * delayBetweenBlocks;
            // Tối ưu: Truyền thẳng BlockController thay vì GameObject
            MoveBlockSmoothly(block, path, moveDuration, otherSlot, isSameType, isSlotEmpty, delay);     
        }
        
        int j = 0;
        foreach (BlockController block in otherSlot.blocks)
        {
            if(block.GetTopicID() != topicID || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.None);
            Vector3 targetPosition = new Vector3(otherSlot.stackAnchor.position.x, otherSlot.stackAnchor.position.y + (otherSlot.blocks.Count - 1 - j) * Constants.BLOCK_HEIGHT, otherSlot.stackAnchor.position.z);
            MoveBlockSmoothly(block, new List<Vector3> { targetPosition }, selectDuration);
            j++;
        }
        return true;
    }
    
    public bool UnSelect()
    {
        if(isMoving || isFinished || gameManager.GetCurrState() == GameManager.GameState.Pause
        || gameManager.GetCurrState() == GameManager.GameState.Lose
        || !isRevealed) return false;
        
        int topicID = blocks.Peek().GetTopicID();
        int i = 0;
        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID || !block.isRevealed) break;
            block.ChangeState(BlockController.BlockState.None);
            Vector3 targetPosition = new Vector3(stackAnchor.position.x, stackAnchor.position.y + (blocks.Count - 1 - i) * Constants.BLOCK_HEIGHT, stackAnchor.position.z);
            block.transform.DOKill();
            block.transform.DOMove(targetPosition, selectDuration).SetEase(Ease.OutQuad);
            i++;
        }
        return true;
    }
    
    // Đổi GameObject block thành BlockController block để tránh gọi GetComponent
    private void MoveBlockSmoothly(BlockController block, List<Vector3> path, float duration, SlotController slot = null, bool isSameType = false, bool isSlotEmpty = false, float delay = 0f)
    {
        BlockStartMoving();
        Vector3[] pathArr = path.ToArray();

        block.transform.DOKill();

        block.transform.DOPath(pathArr, duration, PathType.CatmullRom)
            .SetDelay(delay)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                BlockReachedDestination(block, slot, isSameType, isSlotEmpty); 
            });
    }

    private void CheckSlotComplete()
    {
        int topicID = blocks.Peek().GetTopicID();
        foreach(BlockController block in blocks)
        {
            if(block.GetTopicID() != topicID || !block.isRevealed) return;
        }
        
        isFinished = true;
        AudioManager.Instance.PlaySlotFinishedAudio();
        
        if(slotType == SlotType.Ice)
        {
            iceRod.SetActive(false);
            iceVFX.SetActive(false);
            
            // Tối ưu: Dùng cache
            if (baseMeshCache == null) baseMeshCache = Resources.Load<Mesh>(Constants.MESH_BASE_PATH);
            if (baseMaterialCache == null) baseMaterialCache = Resources.Load<Material>(Constants.MATERIAL_BASE_PATH);
            
            if (baseMeshFilter != null) baseMeshFilter.mesh = baseMeshCache;
            if (baseMeshRenderer != null) baseMeshRenderer.material = baseMaterialCache;
            
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
            } 
            else
            {
                AudioManager.Instance.PlayBlockFailAudio();
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
                iceRod.transform.DOKill();
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

            if(blocks.Count == 4) CheckSlotComplete();
            isMoving = false;
            OnMoveFisnished?.Invoke(isMoving);
        } 
    }

    public void Reveal()
    {
        isRevealed = true;
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
            
            block.transform.DOKill(); // Tối ưu: DOKill trước khi gán Tween mới
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
        
        block.transform.DOKill(); // Tối ưu: DOKill trước khi gán Tween mới
        block.transform.DOPath(path.ToArray(), 0.5f, PathType.CatmullRom);
        blocks.Push(block);
    }
}
