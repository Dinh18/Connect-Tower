using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        tutorialCanvas.SetActive(true);
        mechanicImage.gameObject.SetActive(false);
        closeText.gameObject.SetActive(false);
        
        if (handImage != null) handImage.SetActive(true);

        RectTransform targetRect = target.GetComponent<RectTransform>();
        if(targetRect != null)
        {
            if (handImage != null) handImage.GetComponent<RectTransform>().position = targetRect.position;
            dimImage.SetActive(true);
            ElevateTarget(target);
        }
        else
        {
            if (handImage != null) handImage.transform.position = Camera.main.WorldToScreenPoint(target.gameObject.transform.position);
            dimImage.SetActive(false);
        }
        if (tutorialText != null) tutorialText.text = instruction;
    }

    public void StartMechanicTutorial(int mechanicId)
    {
        tutorialCanvas.SetActive(true);
        if (handImage != null) handImage.SetActive(false);
        dimImage.SetActive(true);
        
        closeText.gameObject.SetActive(true);
        mechanicImage.gameObject.SetActive(true);
        if (mechanicSprites != null && mechanicId >= 0 && mechanicId < mechanicSprites.Count)
        {
            mechanicImage.sprite = mechanicSprites[mechanicId];
        }
        
        // Hardcode instruction for now since it was moved to step
        // Or we can add instruction to StartMechanicTutorial signature if needed.
    }

    public void StartMechanicTutorial(int mechanicId, string instruction)
    {
        StartMechanicTutorial(mechanicId);
        if (tutorialText != null) tutorialText.text = instruction;
    }

    public void EndTutorial()
    {
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
        tutorialCanvas.SetActive(false);
    }
}
