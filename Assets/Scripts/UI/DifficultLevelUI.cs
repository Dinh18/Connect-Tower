using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultLevel : MonoBehaviour
{
    // Mở comment dimImage để làm tối background nhé
    [SerializeField] private Image bgImage; 
    [SerializeField] private GameObject dimImage;
    [SerializeField] private Transform hard_NPC_Image;
    [SerializeField] private Transform textHolder;
    [SerializeField] private Image bgTextImage;
    [SerializeField] private TextMeshProUGUI difficultLevelText;
    [SerializeField] private TextMeshProUGUI shadowText;
    [Header("Hard Level Setting")]
    [SerializeField] private Color hardBgColor;
    [SerializeField] private Color hardbgTextColor;
    [Header("Super Hard Level Setting")]
    [SerializeField] private Color spHardBgColor;
    [SerializeField] private Color spHardBgTextColor;
    [SerializeField] private GameObject fireHolder;

    // Biến để lưu vị trí Y ban đầu của NPC
    private float npcOriginalPosY;

    private void Awake()
    {
        // Lưu lại vị trí chuẩn trên Scene để khi nảy lên sẽ dùng tọa độ này
        npcOriginalPosY = hard_NPC_Image.localPosition.y;
    }

    public void ShowDifficultLevel(LevelLoader.GameDifficult difficultLevel)
    {
        if(difficultLevel == LevelLoader.GameDifficult.Hard)
        {
            bgImage.color = hardBgColor;
            bgTextImage.color = hardbgTextColor;
            difficultLevelText.text = "HARD LEVEL";
            shadowText.text = "HARD LEVEL";
            fireHolder.SetActive(false);
            // GameEventBus.Publish(new RequestChangeAnimationBoss { newState = BossState.Hard });

        }
        else if(difficultLevel == LevelLoader.GameDifficult.VeryHard)
        {
            bgImage.color = spHardBgColor;
            bgTextImage.color = spHardBgTextColor;
            difficultLevelText.text = "SUPER HARD";
            shadowText.text = "SUPER HARD";
            fireHolder.SetActive(true);
            // GameEventBus.Publish(new RequestChangeAnimationBoss { newState = BossState.SuperHard });
        }
        this.gameObject.SetActive(true);
        dimImage.SetActive(true);

        // 1. Cài đặt trạng thái ban đầu trước khi chạy Animation
        textHolder.localScale = Vector3.zero;
        hard_NPC_Image.localScale = Vector3.zero;
        
        // Kéo NPC tụt xuống một đoạn để lát nữa làm hiệu ứng nhảy lên
        hard_NPC_Image.localPosition = new Vector3(hard_NPC_Image.localPosition.x, npcOriginalPosY - 150f, 0); 

        if (bgImage != null)
        {
            bgImage.gameObject.SetActive(true);
            Color dimColor = bgImage.color;
            dimColor.a = 0f; // Bắt đầu với alpha = 0 (trong suốt)
            bgImage.color = dimColor; // Trong suốt
        }

        Sequence sequence = DOTween.Sequence();

        // 2. Kịch bản Animation
        
        // Tối màn hình dần dần
        if (bgImage != null)
            sequence.Append(bgImage.DOFade(0.7f, 0.3f));

        // Banner đập ra và rung nhẹ tạo cảm giác chấn động
        sequence.Append(textHolder.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        sequence.Join(textHolder.DOPunchRotation(new Vector3(0, 0, -3f), 0.4f, 5, 0.5f)); 

        // NPC phóng to và nảy từ dưới chui lên trên banner
        sequence.Insert(0.2f, hard_NPC_Image.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        sequence.Insert(0.2f, hard_NPC_Image.DOLocalMoveY(npcOriginalPosY, 0.5f).SetEase(Ease.OutBack));

        sequence.OnComplete(() => {
            // Sau khi hoàn thành Animation, đợi 1.5s rồi tự động ẩn đi
            // DOVirtual.DelayedCall(1.5f, HideDifficultLevel);
            if(difficultLevel == LevelLoader.GameDifficult.Hard)
        {
            GameEventBus.Publish(new RequestChangeAnimationBoss { newState = BossState.Hard });

        }
        else if(difficultLevel == LevelLoader.GameDifficult.VeryHard)
        {
            GameEventBus.Publish(new RequestChangeAnimationBoss { newState = BossState.SuperHard });
        }
        });

        // 3. Đợi 1.5s rồi tự động ẩn đi (Thay cho Coroutine)
        DOVirtual.DelayedCall(1.5f, HideDifficultLevel);
    }

    // Đổi thành void vì không dùng IEnumerator nữa
    public void HideDifficultLevel() 
    {
        Sequence sequence = DOTween.Sequence();

        // Cho NPC thụt vòi xuống trước
        sequence.Append(hard_NPC_Image.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        sequence.Join(hard_NPC_Image.DOLocalMoveY(npcOriginalPosY - 100f, 0.3f).SetEase(Ease.InBack));

        // Sau đó thu nhỏ banner
        sequence.Append(textHolder.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

        // Cuối cùng nhả sáng màn hình và tắt gameobject
        if (bgImage != null)
            sequence.Join(bgImage.DOFade(0f, 0.3f));

        sequence.OnComplete(() =>
        {
            this.gameObject.SetActive(false);
            dimImage.SetActive(false);
        });
    }
}