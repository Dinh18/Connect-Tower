using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialDimBackground : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(Input.touchCount > 0 && TutorialManager.Instance.currentTutorial == TutorialManager.TutorialType.Mechanic)
        {
            TutorialManager.Instance.EndMechanicTutorial();
        }
    }

}
