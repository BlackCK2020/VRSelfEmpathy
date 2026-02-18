using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueFlowController : MonoBehaviour
{
    [Serializable]
    public class StepDefinition
    {
        public int stepId;
        [TextArea] public string headPromptText;   // shown briefly above the girl
        [TextArea] public string hudTaskText;      // shown on top-right HUD until next step
        [TextArea] public string girlDialogueText; // optional: girl's line (bubble/subtitle later)
        public bool requiresRecording;             // used later
        public float headPromptDuration = 180f;
    }

    [Header("UI References")]
    public TMP_Text topRightTaskText;
    public TMP_Text headPromptText;        // world-space TMP text near the girl's head
    public Transform headPromptAnchor;     // anchor transform on girl's head

    [Header("Steps (MVP)")]
    public List<StepDefinition> steps = new List<StepDefinition>();

    [Header("Debug / Control")]
    public bool autoStartOnPlay = false;
    public KeyCode debugNextKey = KeyCode.N;

    // state
    public int CurrentStepId { get; private set; } = -1;
    private int currentIndex = -1;
    private bool flowStarted = false;

    // events for teammates
    public event Action<int> OnStepStarted;
    public event Action<int> OnStepCompleted;
    public event Action OnPhase1Completed;

    void Start()
    {
        // Optional: keep head prompt positioned at anchor
        if (headPromptText != null)
            headPromptText.gameObject.SetActive(false);

        if (autoStartOnPlay)
            StartFlow();
    }

    void Update()
    {
        // MVP debug: press N to complete current step and go next
        if (flowStarted && Input.GetKeyDown(debugNextKey))
        {
            MarkStepComplete(CurrentStepId);
        }

        // Keep head prompt following anchor (basic, no billboard yet)
        if (headPromptText != null && headPromptAnchor != null)
        {
            headPromptText.transform.position = headPromptAnchor.position;
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
        currentIndex++;

        if (currentIndex >= steps.Count)
        {
            Debug.Log("[DialogueFlow] Phase 1 completed.");
            OnPhase1Completed?.Invoke();
            return;
        }


        var step = steps[currentIndex];
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

        Debug.Log($"[DialogueFlow] Step started: {step.stepId}");
        OnStepStarted?.Invoke(step.stepId);
    }

    private IEnumerator ShowHeadPrompt(string text, float duration)
    {
        if (headPromptText == null) yield break;

        headPromptText.text = text;
        headPromptText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        headPromptText.gameObject.SetActive(false);
    }

    // Teammate A (interactions) will call this to complete action steps.
    // Voice recorder will call this when recording stops for recording steps.
    public void MarkStepComplete(int stepId)
    {
        if (!flowStarted) return;
        if (stepId != CurrentStepId)
        {
            Debug.LogWarning($"[DialogueFlow] Ignored completion: stepId={stepId}, but CurrentStepId={CurrentStepId}");
            return;
        }

        Debug.Log($"[DialogueFlow] Step completed: {stepId}");
        OnStepCompleted?.Invoke(stepId);

        MoveNextStep();
    }
}
