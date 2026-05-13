using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    private bool hasSelected = false;
    private SlotController selectedSlot = null;

    void OnEnable()
    {
        InputManager.OnSlotClicked+=HandleSlotClicked;
    }

    void OnDisable()
    {
        InputManager.OnSlotClicked-=HandleSlotClicked;
    }

    public void ResetSelection()
    {
        hasSelected = false;
        selectedSlot = null;
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
