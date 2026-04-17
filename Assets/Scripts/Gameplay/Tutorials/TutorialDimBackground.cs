using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialDimBackground : MonoBehaviour, IPointerDownHandler
{
    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     Debug.Log("Touch DimImage");
    //     if(TutorialManager.Instance.currentTutorial == TutorialManager.TutorialType.Mechanic)
    //     {
    //         TutorialManager.Instance.EndMechanicTutorial();
    //     }
    // }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(TutorialManager.Instance.currentTutorial == TutorialManager.TutorialType.Mechanic)
        {
            TutorialManager.Instance.EndMechanicTutorial();
        }
    }
}
