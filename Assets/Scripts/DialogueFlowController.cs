using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueFlowController : MonoBehaviour
{
    public enum StepCompletionMode
    {
        AutoAfterDelay,          // completes automatically after autoCompleteDelay
        WaitForExternalSignal,   // teammate scripts call CompleteCurrentStep()
        Recording,               // voice system calls CompleteCurrentStepWithRecording(...)
        PressConfirmButton       // player presses a confirm key to complete
    }

    [Serializable]
    public class StepDefinition
    {
        public int stepId;

        [TextArea] public string headPromptText;   // shown briefly above the girl
        [TextArea] public string hudTaskText;      // shown on top-right HUD until next step
        [TextArea] public string girlDialogueText; // optional: girl's line (bubble/subtitle later)

        public StepCompletionMode completionMode = StepCompletionMode.WaitForExternalSignal;

        [Tooltip("Used when completionMode = AutoAfterDelay")]
        public float autoCompleteDelay = 2f;

        [Tooltip("Used when completionMode = Recording")]
        public bool requiresRecording = false;

        [Tooltip("Head prompt display duration (seconds)")]
        public float headPromptDuration = 1.5f;

        [Tooltip("Optional: per-step handler component for custom logic (actions, animations, triggers, etc.)")]
        public DialogueStepHandler handler; // can be null
    }

    [Header("UI References")]
    public TMP_Text topRightTaskText;
    public TMP_Text headPromptText;        // world-space TMP text near the girl's head
    public Transform headPromptAnchor;     // anchor transform on girl's head

    [Header("Steps (MVP)")]
    public List<StepDefinition> steps = new List<StepDefinition>();

    [Header("Control")]
    public bool autoStartOnPlay = false;

    [Header("Debug / Test Mode")]
    public bool testModeIgnoreConditions = false;  // if true, ignore conditions
    public KeyCode debugNextKey = KeyCode.N;       // press to force next step (esp. in test mode)
    public KeyCode confirmKey = KeyCode.Space;     // used when completionMode = PressConfirmButton

    // state
    public int CurrentStepId { get; private set; } = -1;
    public StepDefinition CurrentStep { get; private set; } = null;
    private int currentIndex = -1;
    private bool flowStarted = false;
    private Coroutine autoCompleteRoutine;

    // events for teammates
    public event Action<int> OnStepStarted;
    public event Action<int> OnStepCompleted;
    public event Action OnPhase1Completed;

    void Start()
    {
        if (headPromptText != null)
            headPromptText.gameObject.SetActive(false);

        if (autoStartOnPlay)
            StartFlow();
    }

    void Update()
    {
        if (!flowStarted) return;

        // Keep head prompt following anchor (basic, no billboard yet)
        if (headPromptText != null && headPromptAnchor != null)
            headPromptText.transform.position = headPromptAnchor.position;

        // Test/debug: force completion
        if (Input.GetKeyDown(debugNextKey))
        {
            if (testModeIgnoreConditions)
            {
                CompleteCurrentStep();
            }
            else
            {
                // Allow forcing next even when not in test mode, if you want:
                // CompleteCurrentStep();
                // Or keep it strict:
                Debug.Log("[DialogueFlow] DebugNextKey pressed but testModeIgnoreConditions is OFF.");
            }
        }

        // If this step uses "PressConfirmButton", allow confirm key to complete it
        if (!testModeIgnoreConditions && CurrentStep != null &&
            CurrentStep.completionMode == StepCompletionMode.PressConfirmButton &&
            Input.GetKeyDown(confirmKey))
        {
            CompleteCurrentStep();
        }

        // Optional: per-step tick
        if (!testModeIgnoreConditions && CurrentStep != null && CurrentStep.handler != null)
        {
            CurrentStep.handler.Tick(this, CurrentStep);
        }
    }

    // Call this when player enters trigger (Phase 1 start)
    public void StartFlow()
    {
        if (flowStarted) return;
        flowStarted = true;
        currentIndex = -1;
        MoveNextStep();
    }

    private void MoveNextStep()
    {
        // cleanup previous step
        StopAutoCompleteRoutineIfAny();
        if (CurrentStep != null && CurrentStep.handler != null)
            CurrentStep.handler.OnStepExit(this, CurrentStep);

        currentIndex++;

        if (currentIndex >= steps.Count)
        {
            Debug.Log("[DialogueFlow] Phase 1 completed.");
            OnPhase1Completed?.Invoke();
            return;
        }

        var step = steps[currentIndex];
        CurrentStep = step;
        CurrentStepId = step.stepId;

        // 1) show head prompt briefly
        if (headPromptText != null)
            StartCoroutine(ShowHeadPrompt(step.headPromptText, step.headPromptDuration));

        // 2) set HUD task text (persistent)
        if (topRightTaskText != null)
            topRightTaskText.text = step.hudTaskText;

        // 3) optional: log girl's dialogue for now
        if (!string.IsNullOrWhiteSpace(step.girlDialogueText))
            Debug.Log($"[DialogueFlow] Girl says: {step.girlDialogueText}");

        Debug.Log($"[DialogueFlow] Step started: {step.stepId} mode={step.completionMode}");
        OnStepStarted?.Invoke(step.stepId);

        // 4) handler hook (teammates can implement per-step behavior here)
        if (step.handler != null)
            step.handler.OnStepEnter(this, step);

        // 5) completion mode setup
        if (!testModeIgnoreConditions)
        {
            if (step.completionMode == StepCompletionMode.AutoAfterDelay)
            {
                autoCompleteRoutine = StartCoroutine(AutoCompleteAfter(step.autoCompleteDelay));
            }
            // Recording and WaitForExternalSignal are completed by external calls.
            // PressConfirmButton is handled in Update().
        }
    }

    private IEnumerator AutoCompleteAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentStep();
    }

    private void StopAutoCompleteRoutineIfAny()
    {
        if (autoCompleteRoutine != null)
        {
            StopCoroutine(autoCompleteRoutine);
            autoCompleteRoutine = null;
        }
    }

    private IEnumerator ShowHeadPrompt(string text, float duration)
    {
        if (headPromptText == null) yield break;

        headPromptText.text = text;
        headPromptText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        headPromptText.gameObject.SetActive(false);
    }

    // ==== Public completion APIs for teammates ====

    // Use this for action steps, confirm steps, auto steps, etc.
    public void CompleteCurrentStep()
    {
        if (!flowStarted) return;
        if (CurrentStep == null) return;

        Debug.Log($"[DialogueFlow] Step completed: {CurrentStepId}");
        OnStepCompleted?.Invoke(CurrentStepId);
        MoveNextStep();
    }

    // Use this when recording stops (you will call this from your voice system)
    public void CompleteCurrentStepWithRecording(AudioClip recordedClip)
    {
        // For now, just complete. Later you can store recordedClip in a dictionary here.
        // Example: stepRecordings[CurrentStepId] = recordedClip;

        CompleteCurrentStep();
    }

    // Backward-compatible call (if your teammates already call MarkStepComplete(stepId))
    public void MarkStepComplete(int stepId)
    {
        if (!flowStarted) return;
        if (stepId != CurrentStepId)
        {
            Debug.LogWarning($"[DialogueFlow] Ignored completion: stepId={stepId}, but CurrentStepId={CurrentStepId}");
            return;
        }

        CompleteCurrentStep();
    }
}
