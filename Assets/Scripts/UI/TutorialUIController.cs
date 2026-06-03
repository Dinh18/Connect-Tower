using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    [Header("Instruction_Holder")]
    [SerializeField] private RectTransform tutTextHolder;
    [SerializeField] private RectTransform npcHolder;

    private GameObject currentElevatedTarget;
    private bool canCloseTutorial = true;
    private Tween delayTween;
    private Vector2 originalTutTextPos;
    private Vector2 originalNpcPos;
    private Tween holdersTween;
    private Coroutine tutorialCoroutine;

    void Awake()
    {
        CoreServices.Register<TutorialUIController>(this);

        if (tutTextHolder != null) originalTutTextPos = tutTextHolder.anchoredPosition;
        if (npcHolder != null) originalNpcPos = npcHolder.anchoredPosition;

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

    private IEnumerator WaitLoadingAndExecute(System.Action action)
    {
        // Đợi 1 frame để đảm bảo các event bật LoadingImage (nếu có) được chạy trước
        yield return null;

        UIManager uiManager = CoreServices.Get<UIManager>();
        if (uiManager != null)
        {
            LoadingImage loadingImage = uiManager.GetUI<LoadingImage>();
            Debug.Log($"[TutorialUI] WaitLoadingAndExecute called. LoadingImage is null? {loadingImage == null}. Active? {(loadingImage != null ? loadingImage.gameObject.activeInHierarchy.ToString() : "N/A")}");
            if (loadingImage != null && loadingImage.gameObject.activeInHierarchy)
            {
                yield return new WaitUntil(() => loadingImage == null || !loadingImage.gameObject.activeInHierarchy);
            }
        }
        else
        {
            Debug.Log("[TutorialUI] UIManager is null!");
        }
        
        Debug.Log("[TutorialUI] Executing tutorial action!");
        action?.Invoke();
        tutorialCoroutine = null;
    }

    public void StartTutorial(GameObject target, string instruction, bool forceCircle = false)
    {
        UIManager uiManager = CoreServices.Get<UIManager>();
        if (uiManager != null)
        {
            if (tutorialCoroutine != null) uiManager.StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = uiManager.StartCoroutine(WaitLoadingAndExecute(() => DoStartTutorial(target, instruction, forceCircle)));
        }
        else
        {
            DoStartTutorial(target, instruction, forceCircle);
        }
    }

    private void DoStartTutorial(GameObject target, string instruction, bool forceCircle)
    {
        // Prevent closing tutorial for 1.5 seconds
        canCloseTutorial = false;
        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(1.5f, () => canCloseTutorial = true);

        tutorialCanvas.SetActive(true);
        AnimateHolders();
        mechanicImage.gameObject.SetActive(false);
        closeText.gameObject.SetActive(false);
        if (handImage != null) handImage.SetActive(true);

        RectTransform targetRect = target.GetComponent<RectTransform>();
        Canvas parentCanvas = targetRect != null ? targetRect.GetComponentInParent<Canvas>() : null;
        bool isWorldSpaceUI = parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace;
        RectTransform dimRect = dimImage.GetComponent<RectTransform>();
        Image dimImg = dimImage.GetComponent<Image>();
        
        dimImage.SetActive(true);
        Vector2 localPoint = Vector2.zero;

        if(targetRect != null && !isWorldSpaceUI)
        {
            if (handImage != null) handImage.GetComponent<RectTransform>().position = targetRect.position;
            if (dimImg != null) dimImg.raycastTarget = true;
            
            Vector3 localPoint3D = dimRect.InverseTransformPoint(targetRect.position);
            localPoint = new Vector2(localPoint3D.x, localPoint3D.y);
            
            ElevateTarget(target);
        }
        else
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);
            if (handImage != null) handImage.transform.position = screenPoint;
            if (dimImg != null) dimImg.raycastTarget = false; // Bỏ chặn click cho object 3D
            
            Canvas canvas = tutorialCanvas.GetComponent<Canvas>();
            Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dimRect, screenPoint, uiCamera, out localPoint);
        }

        // 1. TẠO HIỆU ỨNG LỖ HỔNG (Hole thu nhỏ dần vào target)
        if (dimImg != null && dimImg.material != null)
        {
            if (!dimImg.material.name.EndsWith("(Instance)"))
            {
                Material mat = new Material(dimImg.material);
                mat.name += " (Instance)";
                dimImg.material = mat;
            }
            Material matInst = dimImg.material;

            CanvasGroup dimGroup = dimImage.GetComponent<CanvasGroup>();
            if (dimGroup != null) dimGroup.alpha = 1f;

            Vector2 uvCenter = new Vector2(
                (localPoint.x - dimRect.rect.xMin) / dimRect.rect.width,
                (localPoint.y - dimRect.rect.yMin) / dimRect.rect.height
            );

            matInst.SetVector("_HoleCenter", new Vector4(uvCenter.x, uvCenter.y, 0, 0));
            matInst.SetFloat("_AspectRatio", dimRect.rect.width / dimRect.rect.height);

            float targetRadiusX = 0f;
            float targetRadiusY = 0f;
            if (targetRect == null || isWorldSpaceUI)
            {
                Renderer rend = target.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    if (forceCircle)
                    {
                        Vector3 extents = rend.bounds.extents;
                        Vector3 screenPtCenter = Camera.main.WorldToScreenPoint(rend.bounds.center);
                        Vector3 screenPtEdge = Camera.main.WorldToScreenPoint(rend.bounds.center + Camera.main.transform.right * extents.x + Camera.main.transform.up * extents.y);
                        float screenRadius = Vector3.Distance(screenPtCenter, screenPtEdge);
                        
                        targetRadiusX = screenRadius / Screen.height;
                        targetRadiusY = screenRadius / Screen.height;
                    }
                    else
                    {
                        Vector3 center = rend.bounds.center;
                        Vector3 extents = rend.bounds.extents;
                        
                        Vector3[] corners = new Vector3[8];
                        corners[0] = center + new Vector3(extents.x, extents.y, extents.z);
                        corners[1] = center + new Vector3(extents.x, extents.y, -extents.z);
                        corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
                        corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);
                        corners[4] = center + new Vector3(-extents.x, extents.y, extents.z);
                        corners[5] = center + new Vector3(-extents.x, extents.y, -extents.z);
                        corners[6] = center + new Vector3(-extents.x, -extents.y, extents.z);
                        corners[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);

                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minY = float.MaxValue, maxY = float.MinValue;

                        foreach (Vector3 corner in corners)
                        {
                            Vector3 screenPt = Camera.main.WorldToScreenPoint(corner);
                            if (screenPt.x < minX) minX = screenPt.x;
                            if (screenPt.x > maxX) maxX = screenPt.x;
                            if (screenPt.y < minY) minY = screenPt.y;
                            if (screenPt.y > maxY) maxY = screenPt.y;
                        }

                        targetRadiusX = ((maxX - minX) / 2f) / Screen.height;
                        targetRadiusY = ((maxY - minY) / 2f) / Screen.height;
                    }

                    targetRadiusX *= 1.2f; // padding
                    targetRadiusY *= 1.2f; // padding
                    
                    targetRadiusX = Mathf.Clamp(targetRadiusX, 0.05f, 0.4f);
                    targetRadiusY = Mathf.Clamp(targetRadiusY, 0.05f, 0.4f);
                }
                else if (targetRect != null && isWorldSpaceUI)
                {
                    Vector3[] corners = new Vector3[4];
                    targetRect.GetWorldCorners(corners);

                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;

                    foreach (Vector3 corner in corners)
                    {
                        Vector3 screenPt = Camera.main.WorldToScreenPoint(corner);
                        if (screenPt.x < minX) minX = screenPt.x;
                        if (screenPt.x > maxX) maxX = screenPt.x;
                        if (screenPt.y < minY) minY = screenPt.y;
                        if (screenPt.y > maxY) maxY = screenPt.y;
                    }

                    targetRadiusX = ((maxX - minX) / 2f) / Screen.height;
                    targetRadiusY = ((maxY - minY) / 2f) / Screen.height;

                    targetRadiusX *= 1.2f; 
                    targetRadiusY *= 1.2f; 
                    
                    targetRadiusX = Mathf.Clamp(targetRadiusX, 0.05f, 0.4f);
                    targetRadiusY = Mathf.Clamp(targetRadiusY, 0.05f, 0.4f);
                }
                else
                {
                    targetRadiusX = 0.15f; // Giá trị an toàn
                    targetRadiusY = 0.15f;
                }
            }

            matInst.SetVector("_HoleSize", new Vector4(1.5f, 1.5f, 0, 0));
            DOTween.To(() => matInst.GetVector("_HoleSize"), x => matInst.SetVector("_HoleSize", x), new Vector4(targetRadiusX, targetRadiusY, 0, 0), 0.5f).SetEase(Ease.OutQuad);
        }
        else
        {
            CanvasGroup dimGroup = dimImage.GetComponent<CanvasGroup>();
            if (dimGroup != null)
            {
                dimGroup.alpha = 0f;
                dimGroup.DOFade(1f, 0.3f);
            }
        }

        if (tutorialText != null) tutorialText.text = instruction;
    }

    public void StartMechanicTutorial(string mechanicId)
    {
        UIManager uiManager = CoreServices.Get<UIManager>();
        if (uiManager != null)
        {
            if (tutorialCoroutine != null) uiManager.StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = uiManager.StartCoroutine(WaitLoadingAndExecute(() => DoStartMechanicTutorial(mechanicId)));
        }
        else
        {
            DoStartMechanicTutorial(mechanicId);
        }
    }

    private void DoStartMechanicTutorial(string mechanicId)
    {
        // Prevent closing tutorial for 1.5 seconds
        canCloseTutorial = false;
        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(1.5f, () => canCloseTutorial = true);

        tutorialCanvas.SetActive(true);
        AnimateHolders();
        if (handImage != null) handImage.SetActive(false);
        dimImage.SetActive(true);
        if (dimImage.GetComponent<Image>() != null) dimImage.GetComponent<Image>().raycastTarget = true;
        
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
        UIManager uiManager = CoreServices.Get<UIManager>();
        if (tutorialCoroutine != null && uiManager != null)
        {
            uiManager.StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }

        delayTween?.Kill();
        holdersTween?.Kill();
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

    private void AnimateHolders()
    {
        holdersTween?.Kill();

        float offsetX = -1500f; // Slide in from the left

        if (tutTextHolder != null)
        {
            tutTextHolder.anchoredPosition = new Vector2(originalTutTextPos.x - offsetX, originalTutTextPos.y);
        }
        if (npcHolder != null)
        {
            npcHolder.anchoredPosition = new Vector2(originalNpcPos.x + offsetX, originalNpcPos.y);
        }

        Sequence seq = DOTween.Sequence();
        if (npcHolder != null)
        {
            seq.Append(npcHolder.DOAnchorPos(originalNpcPos, 0.5f).SetEase(Ease.OutBack));
        }
        if (tutTextHolder != null)
        {
            float insertTime = npcHolder != null ? 0.15f : 0f;
            seq.Insert(insertTime, tutTextHolder.DOAnchorPos(originalTutTextPos, 0.5f).SetEase(Ease.OutBack));
        }
        
        holdersTween = seq;
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
