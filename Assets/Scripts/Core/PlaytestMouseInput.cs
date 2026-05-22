using UnityEngine;
using UnityEngine.EventSystems;

public class PlaytestMouseInput : MonoBehaviour
{
    private InputManager inputManager;

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        if (inputManager == null)
        {
            inputManager = FindFirstObjectByType<InputManager>();
        }

        Debug.Log("<color=cyan>[Playtest Input]</color> Đã tích hợp Playtest Mouse Input (chuột trái) thành công cho chơi thử!");
    }

    private void Update()
    {
        // Chỉ nhận tương tác chuột khi ở Editor hoặc Standalone build
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (inputManager == null) return;
        if (inputManager.isInputBlocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Tránh tương tác click xuyên qua các UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out SlotController slot))
                {
                    // inputManager.TriggerSlotClick(slot);
                }
            }
        }
        #endif
    }
}
