using UnityEngine;

public class Step4_StrokeHeadHandler : DialogueStepHandler
{
    [Header("References")]
    public HeadTouchSensor headTouchSensor;

    [Header("Completion Rule")]
    [Tooltip("How long the hand must stay on head (seconds).")]
    public float requiredTouchSeconds = 0.6f; 

    [Tooltip("If true, touching must be continuous; if false, accumulate.")]
    public bool requireContinuous = true;

    [Header("Girl bubble line on enter")]
    [TextArea] public string girlLineOnEnter = "......"; // fill in Inspector
    public bool showGirlLineOnEnter = true;


    private float timer = 0f;

    public override void OnStepEnter(DialogueFlowController controller, DialogueFlowController.StepDefinition step)
    {
        timer = 0f;

        if (headTouchSensor == null)
            Debug.LogWarning("[Step4_StrokeHeadHandler] headTouchSensor not assigned.");
        // NEW: show girl's line when Step4 starts
        if (showGirlLineOnEnter && !string.IsNullOrWhiteSpace(girlLineOnEnter))
        {
            controller.SetGirlDialogueSegments(new[] { girlLineOnEnter }, showImmediately: true);
        }
    }

    public override void Tick(DialogueFlowController controller, DialogueFlowController.StepDefinition step)
    {
        if (headTouchSensor == null) return;

        if (headTouchSensor.IsTouching)
        {
            Debug.Log("is touching");
            timer += Time.deltaTime;
            if (timer >= requiredTouchSeconds)
            {
                controller.CompleteCurrentStep();
            }
        }
        else
        {
            if (requireContinuous)
                timer = 0f;
        }
    }

    public override void OnStepExit(DialogueFlowController controller, DialogueFlowController.StepDefinition step)
    {
        timer = 0f;
        // NEW: clear bubble when leaving Step4
        controller.ClearGirlDialogue();
    }
}
