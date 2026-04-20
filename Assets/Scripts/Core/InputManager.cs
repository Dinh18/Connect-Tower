using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // private bool selected = false;
    // private SlotController selectedSlot;
    public static event Action<SlotController> OnSlotClicked;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        DetectInput();
    }
    public void Setup()
    {
        
    }
    public void DetectInput()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                ProgressInput(touch.position, touch.fingerId);
            }
        }
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
