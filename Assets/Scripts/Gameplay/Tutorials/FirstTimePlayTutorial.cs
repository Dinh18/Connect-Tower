using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class TutorialTask
{
    public SlotController slot;
    public string instruction;
    public TutorialTask(SlotController slot, string instruction)
    {
        this.slot = slot;
        this.instruction = instruction;
    }
}
public class FirstTimePlayTutorial : MonoBehaviour
{
    public List<TutorialTask> sequence;
    private int currStep;
    void Awake()
    {
        sequence = new List<TutorialTask>();
        
    }
    public void Setup()
    {
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(1), "To move the block, please tap it!"));
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(2), "Move the block by tapping an empty space!"));
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(1), "Stacked blocks of the same category move together!"));
        sequence.Add(new TutorialTask(LevelLoader.Instance.GetSlotByIndex(0), "Stacked blocks of the same category move together!"));
    }
    public void StartFirstTimeTutorial()
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
    void UpdateTutorialVisual()
    {
        // Gọi TutorialManager để hiện bàn tay và Text hướng dẫn cho bước hiện tại
        TutorialManager.Instance.StartTutorial(
            sequence[currStep].slot.gameObject, 
            sequence[currStep].instruction
        );
    }
}
