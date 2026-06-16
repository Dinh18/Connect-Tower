using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
    // [SerializeField] private GameObject hideImage;
    [SerializeField] private GameObject hideVFX;
    [SerializeField] private GameObject iceVFX;
    [SerializeField] private ParticleSystem difVFX;
    [SerializeField] private ParticleSystem sameVFX;
    [SerializeField] private GameObject iceImage;
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject maskHole;
    [SerializeField] private GameObject spcialBlockVFX;
    
    public bool isRevealed;
    public bool isSpecialBlock { get; private set; }
    
    // Caching MeshRenderer để tránh tốn CPU gọi GetComponent liên tục
    private MeshRenderer outLineRenderer;

    void Awake()
    {
        if(outLine != null)
        {
            outLineRenderer = outLine.GetComponent<MeshRenderer>();
        }
    }

    public int GetTopicID() => topic != null ? topic.topicID : -1;
    public string GetTopicName() => topic != null ? topic.name : "SpecialTop";
    public BlockTopic GetBlockTopic() => topic;
    public Sprite GetItemImage() => itemImage;

    public ColorBlock GetColorBlock() => colorBlock;
    public void SetTypeBlock(ColorBlock colorBlock) => this.colorBlock = colorBlock;

    public BlockType GetBlockType() => type;
    public void SetBlockType(BlockType type) => this.type = type;

    public void Setup(BlocksManager blocksManager, int color, BlockTopic topic, BlockType type, Sprite itemImage, SlotController slot, bool isSpecialBlock = false)
    {
        this.blocksManager = blocksManager;
        this.colorBlock = (ColorBlock) color;
        this.topic = topic;
        this.type = type;
        this.itemImage = itemImage;
        this.isSpecialBlock = isSpecialBlock;
        if (itemImage != null)
        {
            itemImageBlock.AddImage(itemImage);
            itemImageBlock.ShowImage();
        }
        HideIceImage();
        iceVFX.SetActive(false);
        // hideImage.SetActive(false);
        hideVFX.SetActive(false);
        difVFX.Stop();
        ResetOutLint();
        maskHole.SetActive(false);
        spcialBlockVFX.SetActive(false);
        transform.DOKill();
        ChangeState(BlockState.None);
        
        if(slot.slotType == SlotController.SlotType.Ice)
        {
            ShowIceImage();
        }
        
        if(type == BlockType.Hide)
        {
            ChangeMaterialOutLine(Constants.MATERIAL_COLOR_HIDE_PATH);
            // hideImage.SetActive(true);
            maskHole.SetActive(true);
            itemImageBlock.HideImage();
            isRevealed = false;
        }
        else
        {
            if (itemImage != null)
            {
                itemImageBlock.AddImage(itemImage);
            }
            isRevealed = true;
        }

        if (this.isSpecialBlock)
        {
            // Simple visual indicator for now: Change outline to a specific color or keep it active with a specific glow
            // We'll use the 'W' color (white/glow) as a placeholder for special block indication if it's not hidden
            if (type != BlockType.Hide)
            {
                ChangeMaterialOutLine(Constants.MATERIAL_Special_PATH);
                spcialBlockVFX.SetActive(true);
            }
            itemImageBlock.HideImage();
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
        itemImageBlock.ShowImage();
        // hideImage.SetActive(false);
        hideVFX.SetActive(true);
        maskHole.SetActive(false);
        isRevealed = true;
        
        if (isSpecialBlock)
        {
            ChangeMaterialOutLine(Constants.MATERIAL_Special_PATH);
            itemImageBlock.HideImage();
        }

        // AudioManager.Instance.PlayHideBlockAudio();
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.HideBlock});
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
        
        if (isSpecialBlock && type != BlockType.Hide)
        {
            materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_W_PATH);
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
        visual.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        
        Sequence seq = DOTween.Sequence();
        seq.SetTarget(visual);
        seq.Append(visual.DOLocalRotate(new Vector3(0, 0, 3f), 0.5f).SetEase(Ease.OutSine));
        seq.Append(visual.DOLocalRotate(new Vector3(0, 0, -3f), 1f).SetEase(Ease.InOutSine));
        seq.Append(visual.DOLocalRotate(new Vector3(0, 0, 0f), 0.5f).SetEase(Ease.InSine));
        seq.SetLoops(-1);

        visual.DOLocalMoveY(0.08f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);  
    }
    
    public void FallEffect(int index)
    {
        visual.DOKill(false); 
        sameVFX.Play(true);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        float jump1 = Mathf.Max(0.15f, 0.5f - (index * 0.12f)); 
        float jump2 = jump1 * 0.4f; // Nảy lần 2 thấp hơn
        float jump3 = jump1 * 0.15f; // Nảy lần 3 rất nhỏ

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(visual);
        
        // --- Nhịp 1 (Nảy cao nhất) ---
        seq.Append(visual.DOLocalMoveY(jump1, 0.15f).SetEase(Ease.OutQuad));
        seq.Append(visual.DOLocalMoveY(0f, 0.15f).SetEase(Ease.InQuad));
        seq.AppendCallback(() => CoreServices.Get<HapticManager>().PlayHaptic());
        
        // Squash 1 (Chạm đất bẹp mạnh)
        seq.Append(visual.DOScale(new Vector3(0.85f, 0.6f, 0.85f), 0.06f).SetEase(Ease.OutQuad));
        
        // --- Nhịp 2 (Nảy vừa) ---
        // Vừa nảy lên vừa dãn nhẹ (Stretch)
        seq.Append(visual.DOLocalMoveY(jump2, 0.09f).SetEase(Ease.OutQuad));
        seq.Join(visual.DOScale(new Vector3(0.72f, 0.78f, 0.72f), 0.09f).SetEase(Ease.OutQuad));
        
        // Rơi xuống, trở lại scale bình thường
        seq.Append(visual.DOLocalMoveY(0f, 0.09f).SetEase(Ease.InQuad));
        seq.Join(visual.DOScale(new Vector3(0.75f, 0.75f, 0.75f), 0.09f).SetEase(Ease.InQuad));

        // Squash 2 (Chạm đất bẹp nhẹ)
        seq.Append(visual.DOScale(new Vector3(0.8f, 0.68f, 0.8f), 0.04f).SetEase(Ease.OutQuad));

        // --- Nhịp 3 (Nảy nhẹ) ---
        // Trở về scale bình thường khi nảy lên
        seq.Append(visual.DOLocalMoveY(jump3, 0.06f).SetEase(Ease.OutQuad));
        seq.Join(visual.DOScale(new Vector3(0.75f, 0.75f, 0.75f), 0.06f).SetEase(Ease.OutQuad));
        
        // Rơi xuống lần cuối
        seq.Append(visual.DOLocalMoveY(0f, 0.06f).SetEase(Ease.InQuad));

        // Nảy nhẹ lần cuối (để có độ đàn hồi)
        seq.Append(visual.DOScale(new Vector3(0.77f, 0.72f, 0.77f), 0.04f).SetEase(Ease.OutQuad));
        seq.Append(visual.DOScale(new Vector3(0.75f, 0.75f, 0.75f), 0.08f).SetEase(Ease.OutBack));
    }

    public void IceShakeEffect(int index)
    {
        visual.DOKill(false); 
        sameVFX.Play(true);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(visual);
        
        seq.Append(visual.DOShakePosition(0.25f, new Vector3(0.06f, 0f, 0.06f), 25));
        seq.Join(visual.DOShakeRotation(0.25f, new Vector3(0f, 0f, 8f), 25));
        seq.AppendCallback(() => CoreServices.Get<HapticManager>().PlayHaptic());
        
        seq.OnComplete(() => {
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
        });
    }

    
    public void PlayErrorShake(Action onCompleteCallBack = null)
    {
        CoreServices.Get<HapticManager>().PlayHaptic();
        visual.DOKill(false);
        visual.localPosition = Vector3.zero;
        visual.DOShakePosition(0.3f, new Vector3(0.1f, 0f, 0f), 15).OnComplete(() =>{
            onCompleteCallBack?.Invoke();
        });
    }
}
