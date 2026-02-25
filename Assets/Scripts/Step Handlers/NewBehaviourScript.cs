using UnityEngine;

public class Step3_GirlDialogueHandler : DialogueStepHandler
{
    [TextArea] public string[] girlLines;

    public override void OnStepEnter(DialogueFlowController controller, DialogueFlowController.StepDefinition step)
    {
        // set multi-segment bubble lines
        controller.SetGirlDialogueSegments(girlLines, showImmediately: true);
    }

    public override void OnStepExit(DialogueFlowController controller, DialogueFlowController.StepDefinition step)
    {
        // optional: hide bubble when leaving step
        controller.ClearGirlDialogue();
    }
}