using System.Collections.Generic;
using UnityEngine;

public class TutorialService : MonoBehaviour
{
    private TutorialSequence currentSequence;

    void Awake()
    {
        CoreServices.Register<TutorialService>(this);
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<LevelLoadedEvent>(OnLevelLoaded);
    }

    private void OnLevelLoaded(LevelLoadedEvent evt)
    {
        var dataManager = CoreServices.Get<DataManager>();
        if (dataManager == null) return;

        // Trigger First Time Play Tutorial
        if (evt.levelIndex == 0 /* && !dataManager.IsTutorialCompleted("FirstPlay") */)
        {
            StartFirstTimePlayTutorial();
            return; // Tránh overlap mechanic tutorial ngay lập tức
        }

        // Trigger Mechanic Tutorials
        foreach (var mechanic in dataManager.GetMechanics())
        {
            if (dataManager.IsFirstTimePlayMechanic(mechanic.id))
            {
                dataManager.PlayedMechanic(mechanic.id);
                StartMechanicTutorial(mechanic.id);
                break; // Chỉ hiện 1 mechanic mỗi lần load level
            }
        }
    }

    private void StartFirstTimePlayTutorial()
    {
        var sequence = new TutorialSequence();
        sequence.AddStep(new ClickSlotStep(1, "To move the block, please tap it!"));
        sequence.AddStep(new ClickSlotStep(2, "Move the block by tapping an empty space!"));
        sequence.AddStep(new ClickSlotStep(1, "Stacked blocks of the same category move together!"));
        sequence.AddStep(new ClickSlotStep(0, "Stacked blocks of the same category move together!"));

        StartSequence(sequence);
    }

    private void StartMechanicTutorial(int mechanicId)
    {
        var sequence = new TutorialSequence();
        string instruction = "";
        if (mechanicId == 0) instruction = "Move blocks to reveal mystery!";
        else if (mechanicId == 1) instruction = "Complete the category to unveil the curtain!";
        else instruction = "Must stack blocks are in category!";

        sequence.AddStep(new ShowMechanicStep(mechanicId, instruction));
        StartSequence(sequence);
    }

    private void StartSequence(TutorialSequence sequence)
    {
        if (currentSequence != null && currentSequence.IsActive) return;

        currentSequence = sequence;
        currentSequence.OnSequenceComplete += () => currentSequence = null;
        currentSequence.Start();
    }

    public bool IsTutorialActive()
    {
        return currentSequence != null && currentSequence.IsActive;
    }

    public bool ProcessInput(object data)
    {
        if (IsTutorialActive())
        {
            return currentSequence.ExecuteCurrentStep(data);
        }
        return false;
    }
}

