using UnityEngine;

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
