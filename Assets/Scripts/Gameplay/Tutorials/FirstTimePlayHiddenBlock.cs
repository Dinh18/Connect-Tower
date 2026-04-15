using System.Collections.Generic;
using UnityEngine;

public class HiddenBlockTutorial : MonoBehaviour
{
    public List<TutorialTask> sequence;
    private int currStep;
    void Awake()
    {
        sequence = new List<TutorialTask>();
        
    }

    void UpdateTutorialVisual()
    {
        // Gọi TutorialManager để hiện bàn tay và Text hướng dẫn cho bước hiện tại
        TutorialManager.Instance.StartTutorial(
            sequence[currStep].slot.gameObject, 
            sequence[currStep].instruction
        );
    }

    public void Setup()
    {
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(1), "To move the block, please tap it!"));
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(2), "Move the block by tapping an empty space!"));
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(1), "Block revealed!"));
    }
    public void StartHiddenTutorial()
    {
        UpdateTutorialVisual();
    }
    
    public bool ExcuteTutorial(SlotController slotClicked)
    {
        if(currStep >= sequence.Count) return true;
        if(slotClicked == sequence[currStep].slot)
        {
            currStep++;

            if(currStep < sequence.Count)
            {
                UpdateTutorialVisual();
                return true;
            }
            else
            {
                TutorialManager.Instance.EndTutorial();
                return true;
            }
        }
        return false;
    }
}
