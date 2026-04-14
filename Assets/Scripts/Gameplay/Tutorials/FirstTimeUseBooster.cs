using UnityEngine;

public class FirstTimeUseBooster : MonoBehaviour
{
    public void Excute(GameObject boosterButton, string instruction)
    {
        TutorialManager.Instance.StartTutorial(boosterButton, instruction);
    }

    public void EndBoosterTutorial(int id)
    {
        TutorialManager.Instance.EndTutorial();
        DataManager.Instance.UseBooster(id);
    }
}
