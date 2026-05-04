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
        tutorialCanvas.SetActive(false);
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
            if (handImage != null) handImage.transform.position = targetRect.position;
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
        var tutorialService = CoreServices.Get<TutorialService>();
        if (tutorialService != null && tutorialService.IsTutorialActive())
        {
            tutorialService.ProcessInput(null); // Passing null to complete steps waiting for any click
        }
    }
}
