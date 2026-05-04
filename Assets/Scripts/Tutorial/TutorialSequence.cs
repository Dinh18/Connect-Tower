using System.Collections.Generic;

public class TutorialSequence
{
    private Queue<TutorialStep> steps = new Queue<TutorialStep>();
    private TutorialStep currentStep;
    public System.Action OnSequenceComplete;

    public void AddStep(TutorialStep step)
    {
        steps.Enqueue(step);
    }

    public void Start()
    {
        NextStep();
    }

    private void NextStep()
    {
        if (currentStep != null)
        {
            currentStep.Exit();
            currentStep.OnStepComplete -= NextStep;
        }

        if (steps.Count > 0)
        {
            currentStep = steps.Dequeue();
            currentStep.OnStepComplete += NextStep;
            currentStep.Enter();
        }
        else
        {
            currentStep = null;
            OnSequenceComplete?.Invoke();
        }
    }

    public bool ExecuteCurrentStep(object data)
    {
        return currentStep?.Execute(data) ?? false;
    }

    public bool IsActive => currentStep != null;
}
