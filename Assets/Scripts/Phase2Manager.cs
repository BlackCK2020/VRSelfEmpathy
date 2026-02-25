using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class Phase2Manager : MonoBehaviour
{
    [Header("Phase 1 -> Phase 2 Trigger")]
    public DialogueFlowController phase1;
    public int phase1EndStepId = 8;

    [Header("Dialogue Systems")]
    public GameObject dialogueSystemPhase1;
    public GameObject dialogueSystemPhase2;

    [Header("XR")]
    public XROrigin xrOrigin;
    public Transform childViewpointAnchor;

    [Header("Freeze movement after swap (recommended)")]
    [Tooltip("Disable these locomotion providers after teleport so the rig doesn't move.")]
    public LocomotionProvider[] locomotionToDisable; // Move/Turn/GrabMove/Climb etc.

    [Header("Optional: also disable CharacterController movement collisions drift")]
    public CharacterController characterController;

    [Header("Optional scripts to disable (no embodiment)")]
    public Behaviour adultFollow;
    public Behaviour childFollow;
    public Animator childAnimator;

    [Header("Options")]
    public bool matchYawRotation = true;

    private bool phase2Started = false;

    private void OnEnable()
    {
        if (phase1 != null)
            phase1.OnStepCompleted += HandleStepCompleted;
    }

    private void OnDisable()
    {
        if (phase1 != null)
            phase1.OnStepCompleted -= HandleStepCompleted;
    }

    private void HandleStepCompleted(int stepId)
    {
        if (phase2Started) return;
        if (stepId != phase1EndStepId) return;

        phase2Started = true;

        if (phase1 != null)
            phase1.OnStepCompleted -= HandleStepCompleted;

        StartCoroutine(BeginPhase2Routine());
    }

    private IEnumerator BeginPhase2Routine()
    {
        // Let any last step UI/events finish
        yield return null;

        // You said: no movement / no follow
        if (adultFollow != null) adultFollow.enabled = false;
        if (childFollow != null) childFollow.enabled = false;
        if (childAnimator != null) childAnimator.enabled = false;

        // Wait for XR pose update so MoveCameraToWorldLocation uses the latest HMD pose
        yield return new WaitForEndOfFrame();

        // 1) Put CAMERA exactly at the child anchor (XR-safe)
        MoveCameraExactlyToAnchor(childViewpointAnchor);

        // 2) Freeze locomotion so the rig doesn't move afterwards
        DisableLocomotion();

        // 3) Switch dialogue systems
        if (dialogueSystemPhase1 != null) dialogueSystemPhase1.SetActive(false);
        if (dialogueSystemPhase2 != null) dialogueSystemPhase2.SetActive(true);
    }

    private void MoveCameraExactlyToAnchor(Transform anchor)
    {
        if (xrOrigin == null || xrOrigin.Camera == null || anchor == null)
        {
            Debug.LogWarning("[Phase2Manager] Missing xrOrigin/camera/anchor.");
            return;
        }

        // (A) Optional yaw align FIRST (yaw only)
        if (matchYawRotation)
        {
            float currentYaw = xrOrigin.Camera.transform.eulerAngles.y;
            float targetYaw = anchor.eulerAngles.y;
            float deltaYaw = Mathf.DeltaAngle(currentYaw, targetYaw);

            // Rotate the rig around the camera so camera stays pinned
            xrOrigin.transform.RotateAround(xrOrigin.Camera.transform.position, Vector3.up, deltaYaw);
        }

        // (B) Now move the rig so CAMERA position matches anchor position exactly
        xrOrigin.MoveCameraToWorldLocation(anchor.position);
    }

    private void DisableLocomotion()
    {
        // Disable locomotion providers (Move/Turn/GrabMove/Climb/etc)
        if (locomotionToDisable != null)
        {
            foreach (var p in locomotionToDisable)
                if (p != null) p.enabled = false;
        }

        // Optional: prevent CharacterController from pushing/stepping
        if (characterController != null)
            characterController.enabled = false;
    }
}