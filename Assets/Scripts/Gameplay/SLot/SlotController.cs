using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;
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
    [SerializeField] public float height = 0.7f;
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
    // [SerializeField] GameObject hideSlotHolder;
    [SerializeField] private Animator hiddenSlotAnimator;
    [SerializeField] private string[] animationStates = {"CLOTH_04" };
    private int[] animationHashes;
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
    [SerializeField] private Transform completedVFX;
    
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
        // Chuyển đổi toàn bộ tên string sang dạng Hash ID (số nguyên) khi bắt đầu
        animationHashes = new int[animationStates.Length];
        for (int i = 0; i < animationStates.Length; i++)
        {
            animationHashes[i] = Animator.StringToHash(animationStates[i]);
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
        if(iceRod != null) 
        {
            iceRod.transform.DOKill();
            iceRod.SetActive(false);
        }
        

        if(blockTopic != null) this.blockTopic = blockTopic;
        if(header != null) header.Setup(this);
        if(slotVFX != null) slotVFX.Setup();
        
        if (slotType != SlotType.Ice)
        {
            if (baseMeshCache == null) baseMeshCache = Resources.Load<Mesh>(Constants.MESH_BASE_PATH);
            if (baseMaterialCache == null) baseMaterialCache = Resources.Load<Material>(Constants.MATERIAL_BASE_PATH);
            if (baseMeshFilter != null && baseMeshCache != null) baseMeshFilter.mesh = baseMeshCache;
            if (baseMeshRenderer != null && baseMaterialCache != null) baseMeshRenderer.material = baseMaterialCache;
        }

        if(slotType == SlotType.Hide)
        {
            isRevealed = false;
            hiddenSlotAnimator.gameObject.SetActive(true);
            if (hiddenSlotAnimator != null)
            {
                hiddenSlotAnimator.Rebind();
                hiddenSlotAnimator.Play(animationHashes[0], -1, 0f);
                hiddenSlotAnimator.Update(0f);
            }
            hiddenSlotAnimator.GetComponent<SpriteSkin>().rootBone.localPosition = Vector3.zero; // Reset vị trí rootBone để tránh bị lệch
            if (itemImage != null && blockTopic != null && blockTopic.blocksSprite.Count > 0) itemImage.sprite = blockTopic.blocksSprite[0];
        } 
        else 
        {
            isRevealed = true;
            if (hiddenSlotAnimator != null) hiddenSlotAnimator.gameObject.SetActive(false);
        }
    }

    public void SetupIceSlot()
    {
        if(slotType == SlotType.Ice)
        {
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
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.SelectSlot}); 

        return true;
    }

    public int NumsOfBlocksToMove(SlotController otherSlot)
    {
        int topicID = otherSlot.blocks.Peek().GetTopicID();
        int blockCount = 0;
        foreach(BlockController block in otherSlot.blocks)
        {
            if(block.GetTopicID()!= topicID || !block.isRevealed) break;
            blockCount++;
        }
        return Math.Min(4 - blocks.Count, blockCount);
    }

    public bool SelectToRecive(SlotController otherSlot)
    {
        if(isMoving || isFinished || gameManager.GetCurrState() == GameManager.GameState.Pause
            || gameManager.GetCurrState() == GameManager.GameState.Lose
            || !isRevealed || (this.blocks.Count > 0 && this.blocks.Peek().GetTopicID() != otherSlot.blocks.Peek().GetTopicID()))
                 return false;
            
        if(slotType == SlotType.Ice)
        {
            if(blocks.Count > 0)
            {
                if(otherSlot.blocks.Peek().GetTopicID() != this.blocks.Peek().GetTopicID())
                {
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
        
        blocksToMove = NumsOfBlocksToMove(otherSlot);
        if(blocksToMove <= 0)
        {
            return false;
        }
        GameEventBus.Publish(new MovedBlocksEvent{sourceSlot = otherSlot, targetSlot = this, numsBlock = blocksToMove});
        
        float startY = (blocks.Count == 0) ? stackAnchor.position.y : blocks.Peek().transform.position.y + height;

        for(int i = 0;i < blocksToMove; i++)
        {
            BlockController block = otherSlot.blocks.Pop();
            block.ChangeState(BlockController.BlockState.None); 

            List<Vector3> path = PathToMoveBlock(otherSlot, i, startY);
            
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

    public List<Vector3> PathToMoveBlock(SlotController sourceSlot, int index, float startY)
    {
        float zOffset = -2.0f; // Di chuyển block tiến về phía camera để không bị che bởi slot khác
        
        float finalPeakX = (sourceSlot.arcPeak.position.y < this.arcPeak.position.y) ? sourceSlot.arcPeak.position.x : this.arcPeak.position.x;
        float finalPeakY = Mathf.Max(sourceSlot.arcPeak.position.y, this.arcPeak.position.y);
        float finalPeakZ = ((sourceSlot.arcPeak.position.y < this.arcPeak.position.y) ? sourceSlot.arcPeak.position.z : this.arcPeak.position.z) + zOffset;
        Vector3 finalPeak = new Vector3(finalPeakX, finalPeakY, finalPeakZ); 

        Vector3 finalDestination = new Vector3(this.stackAnchor.position.x, startY + height * index, this.stackAnchor.position.z);    
        
        Vector3 sourcePeak = sourceSlot.arcPeak.position;
        sourcePeak.z += zOffset;

        List<Vector3> path = new List<Vector3>{
            sourcePeak,
            finalPeak,
            finalDestination
        };
        
        if(sourceSlot.arcPeak.position.y < this.arcPeak.position.y) 
        {
            Vector3 targetPeak = this.arcPeak.position;
            targetPeak.z += zOffset;
            path.Insert(2, targetPeak);
        }
        return path;
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
    
    private void MoveBlockSmoothly(BlockController block, List<Vector3> path, float duration, SlotController slot = null, bool isSameType = false, bool isSlotEmpty = false, float delay = 0f)
    {
        BlockStartMoving();
        Vector3[] pathArr = path.ToArray();

        block.transform.DOKill();

        // Tăng resolution từ mặc định (10) lên 30 để đường cong CatmullRom mềm và mượt hơn
        block.transform.DOPath(pathArr, duration, PathType.CatmullRom, PathMode.Full3D, 30)
            .SetDelay(delay)
            .SetEase(Ease.InOutSine) // Dùng InOutSine cho cảm giác mượt mà (soft) hơn InOutQuad
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
        // AudioManager.Instance.PlaySlotFinishedAudio();
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.SlotFinished});
        
        if(slotType == SlotType.Ice)
        {
            iceRod.SetActive(false);
            iceVFX.SetActive(false);
            
            // Tối ưu: Dùng cache
            if (baseMeshCache == null) baseMeshCache = Resources.Load<Mesh>(Constants.MESH_BASE_PATH);
            if (baseMaterialCache == null) baseMaterialCache = Resources.Load<Material>(Constants.MATERIAL_BASE_PATH);
            
            if (baseMeshFilter != null) baseMeshFilter.mesh = baseMeshCache;
            if (baseMeshRenderer != null) baseMeshRenderer.material = baseMaterialCache;
            
            // AudioManager.Instance.PlayBlockIceFinishedAudio();
            GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.BlockIceFinished});
        } 
        
        foreach(BlockController block in blocks)
        {
            block.Finished(this);
            block.HideIceImage();
        }
        header.Show();
        slotVFX.PlayVFX();
        PlayCompletedVFX();
        Debug.Log("Slot Completed");
        OnSlotCompleted?.Invoke(blocks.Peek().GetTopicID());
        CoreServices.Get<GamePlayController>().ResetUndoStack();
    }

    private void PlayCompletedVFX()
    {
        if (completedVFX == null) return;
        
        var uiManager = CoreServices.Get<UIManager>();
        Transform targetTransform = null;
        if (uiManager != null)
        {
            var inGameMenu = uiManager.GetMenu<InGameMenu>() as InGameMenu;
            if (inGameMenu != null)
            {
                targetTransform = inGameMenu.GetHeaderPanel()?.GetProgressBarTransform();
            }
        }
        
        if (targetTransform == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Canvas canvas = targetTransform.GetComponentInParent<Canvas>();
        Camera canvasCam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas?.worldCamera;
        
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCam, targetTransform.position);
        
        float distance = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 targetWorldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));

        completedVFX.gameObject.SetActive(true);

        for(int i = 0; i < completedVFX.childCount; i++)
        {
            Transform p = completedVFX.GetChild(i);
            p.gameObject.SetActive(true);
            
            ParticleSystem[] pSystems = p.GetComponentsInChildren<ParticleSystem>();
            foreach(var ps in pSystems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
            
            Vector3 startPos = transform.position;
            p.position = startPos;
            
            Vector3 midPoint = startPos + (targetWorldPos - startPos) / 2f;
            midPoint.x += UnityEngine.Random.Range(-2f, 2f);
            midPoint.y += UnityEngine.Random.Range(0f, 2f);

            Vector3[] path = new Vector3[] { startPos, midPoint, targetWorldPos };

            float delay = i * 0.1f; 
            p.DOPath(path, 0.8f, PathType.CatmullRom)
             .SetDelay(delay)
             .SetEase(Ease.InQuad)
             .OnComplete(() => {
                 p.gameObject.SetActive(false);
                 p.localPosition = Vector3.zero;
                 
                 // Giết tween cũ và reset scale về mặc định trước khi rung
                 targetTransform.DOKill(false);
                 targetTransform.localScale = Vector3.one;
                 targetTransform.DOPunchScale(Vector3.one * 0.15f, 0.3f).OnComplete(() => {
                     targetTransform.localScale = Vector3.one;
                 });
             });
        }
    }

    private void BlockStartMoving()
    {
        movingBlocksCount++;
        // AudioManager.Instance.PlayMoveWooshAudio();
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.MoveWoosh});

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
                    if(block.GetTopicID() != topicID || !block.isRevealed) break;
                    block.ChangeState(BlockController.BlockState.Collde);
                    block.FallEffect(i);
                    i++;
                }
                SoundID sID;
                switch(i)
                {
                    case 1: 
                        sID = SoundID.PopMoved1;
                        break;
                    case 2: 
                        sID = SoundID.PopMoved2;
                        break;
                    case 3: 
                        sID = SoundID.PopMoved3;
                        break;
                    case 4: 
                        sID = SoundID.PopMoved4;
                        break;
                    default:
                        sID = SoundID.None;
                        break;
                    
                }
                GameEventBus.Publish(new RequestPlaySFX{soundID = sID});
            } 
            else
            {
                // AudioManager.Instance.PlayBlockFailAudio();
                GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.BlockFail});
                int i = 0;
                foreach(BlockController block in blocks)
                {
                    if(i >= blocksToMove) break;
                    // block.PlayErrorShake();
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
                // AudioManager.Instance.PlayFreezeUpAudio();
                GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.FreezeUp});
                
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
        
        if (hiddenSlotAnimator != null) 
        {
            hiddenSlotAnimator.Play("CLOTH_04");
            StartCoroutine(WaitAndDisableAnimator());
        }

        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.Cloth});
    }

    private IEnumerator WaitAndDisableAnimator()
    {
        yield return null; // Đợi 1 frame để Animator cập nhật sang state mới
        
        float animLength = hiddenSlotAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        
        if (hiddenSlotAnimator != null)
        {
            hiddenSlotAnimator.Rebind();
            hiddenSlotAnimator.Play("CLOTH_04", -1, 0f);
            hiddenSlotAnimator.Update(0f); // Ép cập nhật frame ngay lập tức để xương về lại vị trí ban đầu
            hiddenSlotAnimator.GetComponent<SpriteSkin>().rootBone.localPosition = Vector3.zero; // Reset vị trí rootBone để tránh bị lệch
            hiddenSlotAnimator.gameObject.SetActive(false);
        }
    }

    public List<BlockController> MoveToShuffle(List<BlockController> diffcultBlocks, Dictionary<int, List<BlockController>> sameBlocks)
    {
        int countBlock = 0;
        List<BlockController> poppedBlocks = new List<BlockController>();
        while(blocks.Count > 0)
        {
            BlockController block = blocks.Pop();
            if(!block.isRevealed)
            {
                blocks.Push(block);
                break;
            }
            
            if(sameBlocks.ContainsKey(block.GetTopicID()))
            {
                if(sameBlocks[block.GetTopicID()].Count < 2) sameBlocks[block.GetTopicID()].Add(block);
                else diffcultBlocks.Add(block);
            }
            else
            {
                if(sameBlocks.Count < 3)
                {
                    List<BlockController> newList = new List<BlockController>();
                    newList.Add(block);
                    sameBlocks.Add(block.GetTopicID(), newList);
                }
                else
                {
                    diffcultBlocks.Add(block);
                }
            }
            countBlock++;
            poppedBlocks.Add(block);
        }
        Debug.Log($"Moved {countBlock} blocks from Slot {row} to shuffle pool");
        return poppedBlocks;
    }

    public void MoveToSlot(BlockController block)
    {
        block.transform.SetParent(CoreServices.Get<BlocksManager>().transform);
        Vector3 destination = new Vector3(stackAnchor.position.x,
                                          stackAnchor.position.y + Constants.BLOCK_HEIGHT * blocks.Count,
                                          stackAnchor.position.z);
                                          
        Vector3 peak = arcPeak.position;
        peak.z -= 2.0f; // Di chuyển block tiến về phía camera để không bị che bởi slot khác
        
        List<Vector3> path = new List<Vector3>{peak, destination};
        
        block.transform.DOKill(); // Tối ưu: DOKill trước khi gán Tween mới
        block.transform.DOPath(path.ToArray(), 0.5f, PathType.CatmullRom);
        blocks.Push(block);
    }
}
