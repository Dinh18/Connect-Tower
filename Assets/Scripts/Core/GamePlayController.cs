using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveStep
{
    public SlotController sourceSlot;
    public SlotController targetSlot;
    public int numsBlock;
    public MoveStep(SlotController sourceSlot, SlotController targetSlot, int numsBlock)
    {
        this.sourceSlot = sourceSlot;
        this.targetSlot = targetSlot;
        this.numsBlock = numsBlock;
        Debug.Log("Add MoveInfo from " + sourceSlot.gameObject.name + " to " + targetSlot.gameObject.name+": "+numsBlock);
    }
}

public class GamePlayController : MonoBehaviour
{
    private bool hasSelected = false;
    private SlotController selectedSlot = null;
    [SerializeField] private Stack<MoveStep> undoStack;

    void OnEnable()
    {
        InputManager.OnSlotClicked+=HandleSlotClicked;
        GameEventBus.Subscribe<MovedBlocksEvent>(RecordMove);
    }

    void OnDisable()
    {
        InputManager.OnSlotClicked-=HandleSlotClicked;
        GameEventBus.UnSubscribe<MovedBlocksEvent>(RecordMove);
    }

    public void ResetSelection()
    {
        hasSelected = false;
        selectedSlot = null;
    }

    public void ResetUndoStack()
    {
        undoStack = new Stack<MoveStep>();
        Debug.Log("Remove history");
        GameEventBus.Publish(new UndoAvailabilityChangedEvent { canUndo = false });
    }

    public void RecordMove(MovedBlocksEvent evt)
    {
        if(evt.targetSlot.slotType == SlotController.SlotType.Ice)
        {
            ResetUndoStack();
            return;
        } 
        MoveStep moveStep = new MoveStep(evt.sourceSlot, evt.targetSlot, evt.numsBlock);
        undoStack.Push(moveStep);
        GameEventBus.Publish(new UndoAvailabilityChangedEvent { canUndo = true });
    }

    public MoveStep PreMoveStep()
    {
        if(undoStack.Count > 0) 
        {
            var step = undoStack.Pop();
            GameEventBus.Publish(new UndoAvailabilityChangedEvent { canUndo = undoStack.Count > 0 });
            return step;
        }
        return null;
    }

    private void HandleSlotClicked(SlotController slot)
    {
        if(CoreServices.Get<GameManager>().GetCurrState() != GameManager.GameState.Playing) return;

        // Bỏ qua xử lý logic game nếu Tutorial đang chạy
        var tutorialService = CoreServices.Get<TutorialService>();
        if (tutorialService != null && tutorialService.IsTutorialActive())
        {
            if(!tutorialService.ProcessInput(slot))
            {
                return;
            }
        }

        HapticManager.Instance.PlayHaptic();
        if(!hasSelected && !slot.isFinished && slot.blocks.Count > 0)
        {
            if(slot.SelectToMove())
            {
                hasSelected = true;
                selectedSlot = slot;
            }
            else
            {
                GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.MoveFail});
            }
        }
        else if(hasSelected && slot != selectedSlot)
        {
            if(slot.SelectToRecive(selectedSlot))
            {
                
                // undoStack.Push(new MoveInfo(selectedSlot, slot, slot.NumsOfBlocksToMove(selectedSlot)));
                ResetSelection();
            }
            else
            {
                MoveFail();
            }
        }
        else if(hasSelected && slot == selectedSlot)
        {
            if(slot.UnSelect())
            {
                ResetSelection();
            }
        }
    }
    private void MoveFail()
    {
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.MoveFail});
        foreach(var block in selectedSlot.blocks)
        {
            if(block.GetCurrState() == BlockController.BlockState.Selected)
            {
                block.PlayErrorShake(block.SelectedEffect);
            }
        }
    }
}
