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
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject hideVFX;
        [SerializeField] private GameObject iceVFX;
        [SerializeField] private ParticleSystem difVFX;
        [SerializeField] private GameObject iceImage;
        public bool isRevealed;
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
        }

        public void ChangeMaterialOutLine(string materialPath)
        {
            Material materialObj = Resources.Load<Material>(materialPath);
                outLine.GetComponent<MeshRenderer>().material = materialObj;
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

        private void GetMaterial(string materialPath)
        {
            
        }

        public void SetColorOutLine()
        {
            Material materialObj;
            switch (colorBlock)
            {
                case ColorBlock.Color_1:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_1_PATH);
                    break;
                case ColorBlock.Color_2:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_2_PATH);
                    break;
                case ColorBlock.Color_3:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_3_PATH);
                    break;
                case ColorBlock.Color_4:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_4_PATH);
                    break;
                case ColorBlock.Color_5:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_5_PATH);
                    break;
                case ColorBlock.Color_6:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_6_PATH);;
                    break;
                case ColorBlock.Color_7:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_7_PATH);
                    break;
                case ColorBlock.Color_8:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_8_PATH);
                    break;
                case ColorBlock.Color_9:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_9_PATH);
                    break;
                default:
                    materialObj = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_W_PATH);
                    break; 
            }
            
            outLine.GetComponent<MeshRenderer>().material = materialObj;
        }
        private void ResetOutLint()
        {
            outLine.GetComponent<MeshRenderer>().material = blocksManager.GetMaterial(Constants.MATERIAL_COLOR_W_PATH);
        }


        public void ChangeState(BlockState blockState)
        {
            // transform.DOKill(true);
            switch (blockState)
            {
                case BlockState.Selected:
                    animator.Play("SelectedAnim", -1, 0f);
                    break;
                case BlockState.Collde:
                    // animator.Play("ColliderWithSameType",-1,0f);;
                    break;
                case BlockState.None:
                animator.Play("None", -1, 0f);
                    break;
            }
            this.currState = blockState;
        }

        
        public void FallEffect(int index)
        {
            // Vì DOPath đã đưa block đến đích, vị trí hiện tại chính là "Mặt đất" chuẩn
            Vector3 basePosition = this.transform.position;

            // Dừng mọi animation di chuyển cũ để chống lỗi kẹt tọa độ
            transform.DOKill(true);

            // Khởi tạo chuỗi hiệu ứng
            Sequence seq = DOTween.Sequence();

            // Tính toán lực nảy: Block trên cùng nảy mạnh (0.6f), càng xuống dưới càng giảm
            float jumpPower = Mathf.Max(0.1f, 0.5f - (index * 0.15f)); 
            float duration = 0.4f; // Thời gian bay lên và rớt xuống

            // Giai đoạn 1: Bị dội dội ngược lên không trung
            seq.Append(transform.DOJump(basePosition, jumpPower, 1, duration));

            // Giai đoạn 2: Nảy búng nhẹ 1 xíu khi đáp đất lần 2
            seq.Append(transform.DOPunchPosition(new Vector3(0, 0.2f, 0), 0.2f, 1, 0))
                .OnComplete(() => 
                {
                    if (currState != BlockState.Selected) 
                    {
                        transform.position = basePosition; 
                    }
                });
        }
    }
