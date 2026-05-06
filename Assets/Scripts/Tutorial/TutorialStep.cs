using System;
using UnityEngine;

public abstract class TutorialStep
{
    public Action OnStepComplete;

    public abstract void Enter();
    public abstract bool Execute(object data);
    public abstract void Exit();

    protected void CompleteStep()
    {
        OnStepComplete?.Invoke();
    }
}

public class ClickSlotStep : TutorialStep
{
    private int slotIndex;
    private string instruction;

    public ClickSlotStep(int slotIndex, string instruction)
    {
        this.slotIndex = slotIndex;
        this.instruction = instruction;
    }

    public override void Enter()
    {
        var levelLoader = CoreServices.Get<LevelLoader>();
        if (levelLoader == null || levelLoader.slots.Count <= slotIndex) return;

        SlotController targetSlot = levelLoader.GetSlotByIndex(slotIndex);

        var tutorialUI = CoreServices.Get<TutorialUIController>(); // Lấy UI Controller
        if (tutorialUI != null)
        {
            tutorialUI.StartTutorial(targetSlot.gameObject, instruction);
        }
    }

    public override bool Execute(object data)
    {
        if (data is SlotController clickedSlot)
        {
            var levelLoader = CoreServices.Get<LevelLoader>();
            if (levelLoader == null) return false;

            SlotController targetSlot = levelLoader.GetSlotByIndex(slotIndex);

            if (clickedSlot == targetSlot)
            {
                CompleteStep();
                return true;
            }
        }
        return false;
    }

    public override void Exit()
    {
        var tutorialUI = CoreServices.Get<TutorialUIController>();
        if (tutorialUI != null)
        {
            tutorialUI.EndTutorial();
        }
    }
}

public class ShowMechanicStep : TutorialStep
{
    private int mechanicId;
    private string instruction;

    public ShowMechanicStep(int mechanicId, string instruction)
    {
        this.mechanicId = mechanicId;
        this.instruction = instruction;
    }

    public override void Enter()
    {
        var tutorialUI = CoreServices.Get<TutorialUIController>();
        if (tutorialUI != null)
        {
            tutorialUI.StartMechanicTutorial(mechanicId, instruction);
        }
    }

    public override bool Execute(object data)
    {
        // Nhận event bất kỳ để đóng (ví dụ null data)
        CompleteStep();
        return true;
    }

    public override void Exit()
    {
        var tutorialUI = CoreServices.Get<TutorialUIController>();
        if (tutorialUI != null)
        {
            tutorialUI.EndTutorial(); // Dùng tạm EndTutorial() để tắt UI
            tutorialUI.OnBackgroundClicked();
        }
    }
}

public class ClickButtonStep : TutorialStep
{
    private UnityEngine.UI.Button targetButton;
    private string instruction;

    public ClickButtonStep(UnityEngine.UI.Button targetButton, string instruction)
    {
        this.targetButton = targetButton;
        this.instruction = instruction;
    } 

    public override void Enter()
    {
        TutorialUIController tutorialUI = CoreServices.Get<TutorialUIController>();
        if(tutorialUI != null && targetButton != null)
        {
            tutorialUI.StartTutorial(targetButton.gameObject, instruction);
        }
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        CompleteStep();
    }

    public override bool Execute(object data)
    {
        if (data is GameObject clickedObj && clickedObj == targetButton?.gameObject)
        {
            CompleteStep();
            return true;
        }
        return false;
    }

    public override void Exit()
    {
        var tutorialUI = CoreServices.Get<TutorialUIController>();
        if (tutorialUI != null)
        {
            tutorialUI.EndTutorial();
        }
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
