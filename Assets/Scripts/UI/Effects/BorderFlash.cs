using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BorderFlash : MonoBehaviour
{
    [SerializeField] private Image warningBorderImage; // Kéo object Warning vào
    [SerializeField] private Image iceBorderImage;     // Kéo object Ice vào
    [SerializeField] private float borderMaxAlpha = 0.5f;

    private BorderType currentActiveBorder;
    private bool isFlashing = false;
    private Tween restartWarningTween;

    void OnEnable()
    {
        GameEventBus.Subscribe<StartBorderFlashEvent>(StartFlash);
        GameEventBus.Subscribe<StopBorderFlashEvent>(StopFlash);  
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<StartBorderFlashEvent>(StartFlash);
        GameEventBus.UnSubscribe<StopBorderFlashEvent>(StopFlash);
        warningBorderImage.DOKill();
        iceBorderImage.DOKill();
        restartWarningTween?.Kill();
    }

    public void StartFlash(StartBorderFlashEvent startBorderFlash)
    {   
        restartWarningTween?.Kill();
        currentActiveBorder = startBorderFlash.borderType;
        isFlashing = true;

        float flashSpeed = startBorderFlash.flashSpeed;
        Image activeImage = null;

        // Dập tắt cả 2 viền trước khi bắt đầu
        warningBorderImage.DOKill();
        iceBorderImage.DOKill();
        warningBorderImage.color = new Color(1f, 0f, 0f, 0f); // Màu đỏ Alpha 0
        iceBorderImage.color = new Color(1f, 1f, 1f, 0f);     // Màu trắng Alpha 0

        // Chọn đúng viền để nhấp nháy
        if (startBorderFlash.borderType == BorderType.Warning)
        {
            activeImage = warningBorderImage;
        }
        else if (startBorderFlash.borderType == BorderType.Ice)
        {
            activeImage = iceBorderImage;
            
            // Kích hoạt hiệu ứng đóng băng nếu cần
            var frost = iceBorderImage.GetComponent<UIFrostEffect>();
            if (frost != null) frost.FrostAmount = 1f; 
        }

        if (activeImage != null)
        {
            activeImage.DOFade(borderMaxAlpha, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    public void StopFlash(StopBorderFlashEvent stopBorderFlash)
    {
        restartWarningTween?.Kill();

        warningBorderImage.DOKill();
        iceBorderImage.DOKill();
        
        warningBorderImage.DOFade(0f, 0.2f);
        iceBorderImage.DOFade(0f, 0.2f);

        if (isFlashing && currentActiveBorder == BorderType.Ice)
        {
            var gameManager = CoreServices.Get<GameManager>();
            if (gameManager != null)
            {
                int moves = gameManager.GetCurrentMoves();
                if (moves > 0 && moves <= 5)
                {
                    restartWarningTween = DOVirtual.DelayedCall(0.25f, () => 
                    {
                        var gm = CoreServices.Get<GameManager>();
                        if (gm != null && gm.GetCurrentMoves() > 0 && gm.GetCurrentMoves() <= 5)
                        {
                            StartFlash(new StartBorderFlashEvent { borderType = BorderType.Warning, flashSpeed = 0.5f });
                        }
                    });
                }
            }
        }
        
        isFlashing = false;
    }
}