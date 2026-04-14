using UnityEngine;
using UnityEngine.EventSystems;

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
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    if(TutorialManager.Instance.IsTutorialActive() && !DataManager.Instance.playerData.isFirstTimePlay) return;
                    if(hit.collider.CompareTag("Slot"))
                    {
                        SlotController slot = hit.collider.GetComponentInChildren<SlotController>();

                        if (TutorialManager.Instance.currentTutorial != TutorialManager.TutorialType.None)
                        {
                            if (TutorialManager.Instance.currentTutorial == TutorialManager.TutorialType.GridPlay)
                            {
                                // Đang dạy xếp gạch -> Nộp bài để sếp tổng chấm
                                if (!TutorialManager.Instance.ProcessTutorialClick(slot)) return;
                            }
                            else
                            {
                                // Đang dạy Booster hoặc loại khác -> Cấm tiệt đụng vào Slot
                                return; 
                            }
                        }

                        HapticManager.Instance.PlayVibrateMedium();

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
                            if(slot.SelectToRecive(selectedSlot))
                            {
                                selectedSlot = null;
                                selected = false;
                                // gameManager.Move();
                            }
                        }
                    }
                }
            }
        }
    }
}
