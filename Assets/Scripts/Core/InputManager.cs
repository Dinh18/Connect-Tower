using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // private bool selected = false;
    // private SlotController selectedSlot;
    public static event Action<SlotController> OnSlotClicked;


    void Awake()
    {
        CoreServices.Register<InputManager>(this);

        // #if UNITY_EDITOR || UNITY_STANDALONE
        // if (GetComponent<PlaytestMouseInput>() == null)
        // {
        //     gameObject.AddComponent<PlaytestMouseInput>();
        // }
        // #endif
    }

    // Update is called once per frame
    void Update()
    {
        DetectInput();
    }
    public bool isInputBlocked = false;

    public void SetInputBlocked(bool blocked)
    {
        isInputBlocked = blocked;
    }

    // public void TriggerSlotClick(SlotController slot)
    // {
    //     if (slot != null)
    //     {
    //         OnSlotClicked?.Invoke(slot);
    //     }
    // }

    public void DetectInput()
    {
        if(isInputBlocked) return;
        
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                ProgressInput(touch.position, touch.fingerId);
            }
            return; // Ignore mouse if touch is active
        }
        
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            ProgressInput(Input.mousePosition, -1);
        }
#endif
    }

    private void ProgressInput(Vector3 screenPosition, int pointerId)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId)) return;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.TryGetComponent(out SlotController slot))
            {
                OnSlotClicked?.Invoke(slot);
            }
        }
    }
}
