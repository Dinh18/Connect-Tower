using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class TutorialUIController : MonoBehaviour
{
    [SerializeField] private GameObject handImage;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text closeText;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Image mechanicImage;
    [SerializeField] private List<Sprite> mechanicSprites;

    private GameObject currentElevatedTarget;
    private bool canCloseTutorial = true;
    private Tween delayTween;

    void Awake()
    {
        CoreServices.Register<TutorialUIController>(this);

        if (dimImage != null)
        {
            // Tự động gán sự kiện click cho dimImage
            Button btn = dimImage.GetComponent<Button>();
            if (btn == null) btn = dimImage.AddComponent<Button>();
            btn.onClick.RemoveListener(OnBackgroundClicked); // Prevent duplicates
            btn.onClick.AddListener(OnBackgroundClicked);

            // Đảm bảo dimImage nhận được click
            Image img = dimImage.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
        }

        // Tắt raycastTarget của các thành phần con để tránh block click
        if (mechanicImage != null) mechanicImage.raycastTarget = false;
        if (tutorialText != null) tutorialText.raycastTarget = false;
        if (closeText != null) closeText.raycastTarget = false;
        if (handImage != null)
        {
            Image handImg = handImage.GetComponent<Image>();
            if (handImg != null) handImg.raycastTarget = false;
        }
    }

    void Update()
    {
        if (currentElevatedTarget != null && handImage != null && handImage.activeSelf)
        {
            RectTransform targetRect = currentElevatedTarget.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                handImage.GetComponent<RectTransform>().position = targetRect.position;
            }
            else
            {
                handImage.transform.position = Camera.main.WorldToScreenPoint(currentElevatedTarget.transform.position);
            }
        }
    }

    public void StartTutorial(GameObject target, string instruction)
    {
        // Prevent closing tutorial for 1.5 seconds
        canCloseTutorial = false;
        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(1.5f, () => canCloseTutorial = true);

        tutorialCanvas.SetActive(true);
        mechanicImage.gameObject.SetActive(false);
        closeText.gameObject.SetActive(false);
        if (handImage != null) handImage.SetActive(true);

        RectTransform targetRect = target.GetComponent<RectTransform>();
        RectTransform dimRect = dimImage.GetComponent<RectTransform>();
        if(targetRect != null)
        {
            if (handImage != null) handImage.GetComponent<RectTransform>().position = targetRect.position;
            dimImage.SetActive(true);

            dimImage.SetActive(true);
        
        // 1. TẠO HIỆU ỨNG LỖ HỔNG (Hole thu nhỏ dần vào target)
        Image dimImg = dimImage.GetComponent<Image>();
        if (dimImg != null && dimImg.material != null)
        {
            // Đảm bảo chỉ tạo bản sao material 1 lần để tránh rò rỉ bộ nhớ
            if (!dimImg.material.name.EndsWith("(Instance)"))
            {
                Material mat = new Material(dimImg.material);
                mat.name += " (Instance)";
                dimImg.material = mat;
            }
            Material matInst = dimImg.material;

            // Đảm bảo CanvasGroup hiển thị 100% (nếu có)
            CanvasGroup dimGroup = dimImage.GetComponent<CanvasGroup>();
            if (dimGroup != null) dimGroup.alpha = 1f;

            // Tìm vị trí tương đối của target bên trong dimRect
            Vector3 localPoint = dimRect.InverseTransformPoint(targetRect.position);
            
            // Chuyển tọa độ sang dạng UV (0.0 đến 1.0)
            Vector2 uvCenter = new Vector2(
                (localPoint.x - dimRect.rect.xMin) / dimRect.rect.width,
                (localPoint.y - dimRect.rect.yMin) / dimRect.rect.height
            );

            matInst.SetVector("_HoleCenter", new Vector4(uvCenter.x, uvCenter.y, 0, 0));
            matInst.SetFloat("_AspectRatio", dimRect.rect.width / dimRect.rect.height);

            // Bắt đầu với lỗ hổng rất to (bao phủ toàn màn hình)
            matInst.SetFloat("_HoleRadius", 1.5f);
            
            // Thu nhỏ lỗ hổng về 0 để tạo hiệu ứng focus vào target
            DOTween.To(() => matInst.GetFloat("_HoleRadius"), x => matInst.SetFloat("_HoleRadius", x), 0f, 0.5f).SetEase(Ease.OutQuad);
        }
        else
        {
            // Fallback nếu không có Image/Material
            CanvasGroup dimGroup = dimImage.GetComponent<CanvasGroup>();
            if (dimGroup != null)
            {
                dimGroup.alpha = 0f;
                dimGroup.DOFade(1f, 0.3f);
            }
        }

        // 2. NHẤC TARGET LÊN TRÊN CÙNG
        // Hàm này cấp Canvas + Raycaster + Sorting Order cao cho Target
        ElevateTarget(target);

        // 3. TẠO ĐIỂM NHẤN CHO TARGET (Thay vì scale nền đen, ta scale chính Target)
        // targetRect.localScale = Vector3.zero;
        // targetRect.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            if (handImage != null) handImage.transform.position = Camera.main.WorldToScreenPoint(target.gameObject.transform.position);
            dimImage.SetActive(false);
        }
        if (tutorialText != null) tutorialText.text = instruction;
    }

    public void StartMechanicTutorial(string mechanicId)
    {
        // Prevent closing tutorial for 1.5 seconds
        canCloseTutorial = false;
        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(1.5f, () => canCloseTutorial = true);

        tutorialCanvas.SetActive(true);
        if (handImage != null) handImage.SetActive(false);
        dimImage.SetActive(true);
        
        closeText.gameObject.SetActive(true);
        mechanicImage.gameObject.SetActive(true);
        if (mechanicSprites != null && int.TryParse(mechanicId, out int mechanicIndex) && mechanicIndex >= 0 && mechanicIndex < mechanicSprites.Count)
        {
            mechanicImage.sprite = mechanicSprites[mechanicIndex];
        }
        
        // Hardcode instruction for now since it was moved to step
        // Or we can add instruction to StartMechanicTutorial signature if needed.
    }

    public void StartMechanicTutorial(string mechanicId, string instruction)
    {
        StartMechanicTutorial(mechanicId);
        if (tutorialText != null) tutorialText.text = instruction;
    }

    public void EndTutorial()
    {
        delayTween?.Kill();
        canCloseTutorial = true;
        
        tutorialCanvas.SetActive(false);
        if (handImage != null) handImage.SetActive(true);
        RestoreTarget();
    }

    private void ElevateTarget(GameObject target)
    {
        currentElevatedTarget = target;
        Canvas canvas = target.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100; 
        target.AddComponent<GraphicRaycaster>();
    }

    private void RestoreTarget()
    {
        if (currentElevatedTarget != null)
        {
            Destroy(currentElevatedTarget.GetComponent<GraphicRaycaster>());
            Destroy(currentElevatedTarget.GetComponent<Canvas>());
            currentElevatedTarget = null;
        }
    }

    public void OnBackgroundClicked()
    {
        if (!canCloseTutorial) return;

        tutorialCanvas.SetActive(false);
        var tutorialService = CoreServices.Get<TutorialService>();
        if (tutorialService != null)
        {
            tutorialService.CancelTutorial();
        }
    }
}
