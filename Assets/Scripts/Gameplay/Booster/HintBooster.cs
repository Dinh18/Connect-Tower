using UnityEngine;

public class HintBooster : Booster
{
    [SerializeField] private FloatingNotifier floatingNotifier;
    public override BoosterType GetBoosterType() => BoosterType.Hint;

    public override bool CanExecute()
    {
        bool hasHiddenBlocks = false;
        foreach(SlotController slot in CoreServices.Get<SlotsManager>().GetAllSlots())
        {
            if(!slot.isRevealed) hasHiddenBlocks = true;
            foreach(BlockController block in slot.blocks)
            {
                if(!block.isRevealed) hasHiddenBlocks = true;
            }
        }

        if (!hasHiddenBlocks) 
        {
            if (floatingNotifier != null)
            {
                floatingNotifier.ShowWarning("All blocks have been revealed!");
            }
            return false;
        }

        return true;
    }

    public override void Excute(System.Action onComplete = null)
    {
        GameEventBus.Publish(new RequestExecuteBoosterEvent 
        { 
            boosterType = BoosterType.Hint,
            onComplete = (success) => 
            {
                if(success)
                {
                    CoreServices.Get<DataManager>().UseBooster((int)BoosterType.Hint);
                    GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.HintBooster});
                    Debug.Log("Thuc hien Hint thanh cong");
                } 
                else 
                {
                    if (floatingNotifier != null) floatingNotifier.ShowWarning("All blocks have been revealed!");
                }
                onComplete?.Invoke();
            }
        });
    }

    
}
