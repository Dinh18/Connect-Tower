using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public enum ColorBlock
    {
        Color_1 = 1,
        Color_2 = 2,
        Color_3 = 3,
        Color_4 = 4,
        Color_5 = 5,
        Color_6 = 6,
        Color_7 = 7,
        Color_8 = 8,
        Color_9 = 9,
        Color_W = 10
    }
    public enum BlockType
    {
        Normal,
        Hide
    }
    public enum BlockState
    {
        None,
        Selected,
        Collde
    }
    private BlockTopic topic;
    private BlockType type;
    private BlocksManager blocksManager;
    private Sprite itemImage;
    private BlockState currState = BlockState.None;
    
    [Header("Block Setting")]
    [SerializeField] private ColorBlock colorBlock;
    [SerializeField] private GameObject outLine;
    [SerializeField] private ItemImageBlock itemImageBlock;
    [SerializeField] private Sprite hideImage;
    [SerializeField] private GameObject hideVFX;
    [SerializeField] private GameObject iceVFX;
    [SerializeField] private ParticleSystem difVFX;
    [SerializeField] private GameObject iceImage;
    [SerializeField] private Transform visual;
    
    public bool isRevealed;
    
    // Caching MeshRenderer để tránh tốn CPU gọi GetComponent liên tục
    private MeshRenderer outLineRenderer;

    void Awake()
    {
        if(outLine != null)
        {
            outLineRenderer = outLine.GetComponent<MeshRenderer>();
        }
    }

    public int GetTopicID() => topic.topicID;
    public string GetTopicName() => topic.name;

    public ColorBlock GetColorBlock() => colorBlock;
    public void SetTypeBlock(ColorBlock colorBlock) => this.colorBlock = colorBlock;

    public BlockType GetBlockType() => type;
    public void SetBlockType(BlockType type) => this.type = type;

    public void Setup(BlocksManager blocksManager, int color, BlockTopic topic, BlockType type, Sprite itemImage, SlotController slot)
    {
        this.blocksManager = blocksManager;
        this.colorBlock = (ColorBlock) color;
        this.topic = topic;
        this.type = type;
        this.itemImage = itemImage;
        HideIceImage();
        iceVFX.SetActive(false);
        hideVFX.SetActive(false);
        difVFX.Stop();
        ResetOutLint();
        ChangeState(BlockState.None);
        
        if(slot.slotType == SlotController.SlotType.Ice)
        {
            ShowIceImage();
        }
        
        if(type == BlockType.Hide)
        {
            ChangeMaterialOutLine(Constants.MATERIAL_COLOR_HIDE_PATH);
            itemImageBlock.AddImage(hideImage);
            isRevealed = false;
        }
        else
        {
            itemImageBlock.AddImage(itemImage);
            isRevealed = true;
        }
    }
    
    public void Finished(SlotController slot)
    {
        SetColorOutLine();
        if(slot.slotType == SlotController.SlotType.Ice)
        {
            iceVFX.SetActive(true);
        }
    }

    public void Reveal()
    {
        ChangeMaterialOutLine(Constants.MATERIAL_COLOR_W_PATH);
        itemImageBlock.AddImage(itemImage);
        
        hideVFX.SetActive(true);
        isRevealed = true;
        AudioManager.Instance.PlayHideBlockAudio();
    }

    public void ChangeMaterialOutLine(string materialPath)
    {
        // Sử dụng Cache của BlocksManager thay vì Resources.Load trực tiếp
        if (outLineRenderer != null && blocksManager != null)
        {
            outLineRenderer.material = blocksManager.GetMaterial(materialPath);
        }
    }

    public void ShowIceImage()
    {
        iceImage.SetActive(true);
    }

    public void HideIceImage()
    {
        iceImage.SetActive(false);
    }

    public void PlayDifVFX()
    {
        difVFX.Play();
    }

    public void SetColorOutLine()
    {
        Material materialObj;
        switch (colorBlock)
        {
            case ColorBlock.Color_1: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_1_PATH); break;
            case ColorBlock.Color_2: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_2_PATH); break;
            case ColorBlock.Color_3: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_3_PATH); break;
            case ColorBlock.Color_4: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_4_PATH); break;
            case ColorBlock.Color_5: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_5_PATH); break;
            case ColorBlock.Color_6: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_6_PATH); break;
            case ColorBlock.Color_7: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_7_PATH); break;
            case ColorBlock.Color_8: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_8_PATH); break;
            case ColorBlock.Color_9: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_9_PATH); break;
            default: materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_W_PATH); break; 
        }
        
        if (outLineRenderer != null)
        {
            outLineRenderer.material = materialObj;
        }
    }
    
    private void ResetOutLint()
    {
        if (outLineRenderer != null && blocksManager != null)
        {
            outLineRenderer.material = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_W_PATH);
        }
    }


    public void ChangeState(BlockState blockState)
    {
        switch (blockState)
        {
            case BlockState.Selected:
                SelectedEffect();
                break;
            case BlockState.Collde:
                break;
            case BlockState.None:
                visual.DOKill(true);
                visual.localPosition = Vector3.zero;
                visual.localRotation = Quaternion.identity;
                visual.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                break;
        }
        this.currState = blockState;
    }

    public BlockState GetCurrState()
    {
        return currState;
    }

    public void SelectedEffect()
    {
        visual.DOKill();
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        
        visual.DOLocalRotate(new Vector3(0, 0, 6f), 0.35f)
              .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        visual.DOLocalMoveY(0.08f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);  
    }
    
    public void FallEffect(int index)
    {
        visual.DOKill(false); 

        Vector3 baseLocalPos = Vector3.zero;
        float jumpPower = Mathf.Max(0.1f, 0.5f - (index * 0.15f)); 

        Sequence seq = DOTween.Sequence();
        seq.Append(visual.DOLocalJump(baseLocalPos, jumpPower, 1, 0.4f));
        seq.Append(visual.DOPunchPosition(new Vector3(0, 0.2f, 0), 0.2f, 1, 0));
    }
    
    public void PlayErrorShake(Action onCompleteCallBack = null)
    {
        HapticManager.Instance.PlayHaptic();
        visual.DOKill(false);
        visual.localPosition = Vector3.zero;
        visual.DOShakePosition(0.3f, new Vector3(0.1f, 0f, 0f), 15).OnComplete(() =>{
            onCompleteCallBack?.Invoke();
        });
    }
}
