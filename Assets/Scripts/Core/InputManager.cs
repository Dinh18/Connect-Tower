using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    private bool selected = false;
    [SerializeField]private SlotController selectedSlot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SelectSlot();
    }
    public void SelectSlot()
    {
        if(GameManager.Instance.GetCurrState() != GameManager.GameState.Playing) return;
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.CompareTag("Slot"))
                    {
                        SlotController slot = hit.collider.GetComponentInChildren<SlotController>();

                        if(!selected && !slot.isFinished && slot.blocks.Count > 0)
                        {
                            if (slot.SelectToMove())
                            {
                                selectedSlot = slot;
                                selected = true;    
                            } 
                        }
                        else if(selected && selectedSlot == slot)
                        {
                            if(slot.UnSelect())
                            {
                                selectedSlot = null;
                                selected = false;
                            }
                        }
                        else if(selected && selectedSlot != slot)
                        {
                            if(slot.SelectToRecive(selectedSlot.GetComponentInChildren<SlotController>()))
                            {
                                selectedSlot = null;
                                selected = false;
                                gameManager.Move();
                            }
                        }
                    }
                }
            }
        }
    }
}
