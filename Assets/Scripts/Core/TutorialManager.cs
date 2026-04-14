
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialType { None, GridPlay, BoosterUI }
    public static TutorialManager Instance;
    [SerializeField] private GameObject handImage;
    [SerializeField] private Text tutorialText;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private FirstTimePlayTutorial firstTimePlayTutorial;
    [SerializeField] private FirstTimeUseBooster firstTimeUseBooster;
    private GameObject currentElevatedTarget;
    public TutorialType currentTutorial = TutorialType.None;
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
        currentTutorial = TutorialType.GridPlay;;
        firstTimePlayTutorial.StartFirstTimeTutorial();
    }

    public bool ProcessTutorialClick(SlotController slot)
    {
        return firstTimePlayTutorial.ExcuteTutorial(slot);
    }

    public void StartUseBoosterTutorial(GameObject boosterButton, string instruction)
    {
        currentTutorial = TutorialType.BoosterUI;
        StartTutorial(boosterButton, instruction);
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
