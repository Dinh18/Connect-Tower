
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialType { None, GridPlay, BoosterUI, Mechanic}
    public static TutorialManager Instance;
    [SerializeField] private GameObject handImage;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text closeText;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Image mechanicImage;
    [SerializeField] private FirstTimePlayTutorial firstTimePlayTutorial;
    [SerializeField] List<Sprite> mechanicSprite = new List<Sprite>();
    // [SerializeField] private HiddenBlockTutorial hiddenBlockTutorial;
    // [SerializeField] private FirstTimeUseBooster firstTimeUseBooster;
    private GameObject currentElevatedTarget;
    public TutorialType currentTutorial = TutorialType.None;
    public int mechanic;
    void Awake()
    {
        Instance = this;

        tutorialCanvas.SetActive(false);

        
    }

    public void SetupFirstTimeTutorial()
    {
        firstTimePlayTutorial.Setup();
    }
    
    public void StartFirstTimeTutorial()
    {
        currentTutorial = TutorialType.GridPlay;
        firstTimePlayTutorial.StartFirstTimeTutorial();
        mechanicImage.gameObject.SetActive(false);
        closeText.gameObject.SetActive(false);
    }

    public bool ProcessTutorialClick(SlotController slot)
    {
        return firstTimePlayTutorial.ExcuteTutorial(slot);
    }

    // public void SetupHiddenTutorial()
    // {
    //     hiddenBlockTutorial.Setup();
    // }

    // public void StartHiddenBlockTutorial()
    // {
    //     currentTutorial = TutorialType.GridPlay;
    //     hiddenBlockTutorial.StartHiddenTutorial();
    // }

    // public bool ProcessHiddenBlockTutorialClick(SlotController slot)
    // {
    //     return hiddenBlockTutorial.ExcuteTutorial(slot);
    // }

    public void StartUseBoosterTutorial(GameObject boosterButton, string instruction)
    {
        currentTutorial = TutorialType.BoosterUI;
        StartTutorial(boosterButton, instruction);
        closeText.gameObject.SetActive(false);
        mechanicImage.gameObject.SetActive(false);
    }

    public void StartMechanicTutorial(int id)
    {
        closeText.gameObject.SetActive(true);
        mechanicImage.gameObject.SetActive(true);
        mechanicImage.sprite = mechanicSprite[id];
        currentTutorial = TutorialType.Mechanic;
        mechanic = id;
        if(id == 0) StartTutorial(closeText.gameObject, "Move blocks to reveal mystery!");
        else if(id == 1) StartTutorial(closeText.gameObject, "Completet the category to unveil the curtain!");
        else StartTutorial(closeText.gameObject, "Must stack blocks are in category!");
    }

    public void EndMechanicTutorial()
    {
        EndTutorial();
        DataManager.Instance.PlayedMechanic(mechanic);
    }

    public void EndBoosterTutorial(int id)
    {
        EndTutorial();
        // DataManager.Instance.UseBooster(id);
        DataManager.Instance.UsedBooster(id);
    }

    public void StartTutorial(GameObject target, string instruction)
    {
        if(!tutorialCanvas.activeSelf) tutorialCanvas.SetActive(true);
        RectTransform targetRect = target.GetComponent<RectTransform>();
        if(targetRect != null)
        {
            handImage.transform.position = targetRect.position;
            dimImage.SetActive(true);
            ElevateTarget(target);
        }
        else
        {
            handImage.transform.position = Camera.main.WorldToScreenPoint(target.gameObject.transform.position);
            dimImage.SetActive(false);
        }
        tutorialText.text = instruction;
    }
    public void EndTutorial()
    {
        currentTutorial = TutorialType.None;
        tutorialCanvas.SetActive(false);
        RestoreTarget();
    }

    private void ElevateTarget(GameObject target)
    {
        currentElevatedTarget = target;

        // 1. Thêm Canvas để có quyền ưu tiên hiển thị (Sorting)
        Canvas canvas = target.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100; // Đặt một số rất lớn để đảm bảo nó nằm trên DarkOverlay (VD DarkOverlay order đang là 0)

        // 2. Thêm GraphicRaycaster để nó có thể nhận click chuột trên cái Layer Canvas mới này
        target.AddComponent<GraphicRaycaster>();
    }

    private void RestoreTarget()
    {
        if (currentElevatedTarget != null)
        {
            // Tháo gỡ các Component đã thêm vào, trả nút về trạng thái bình thường dưới mặt đất
            Destroy(currentElevatedTarget.GetComponent<GraphicRaycaster>());
            Destroy(currentElevatedTarget.GetComponent<Canvas>());
            
            currentElevatedTarget = null;
        }
    }

    public bool IsTutorialActive() => currentTutorial != TutorialType.None;

    
}
