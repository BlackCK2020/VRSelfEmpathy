using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeddyTransferToGirl : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Teddy's XRGrabInteractable (usually on the teddy root).")]
    public XRGrabInteractable grab;

    [Tooltip("Girl root transform (VRChild). Not used for distance anymore, but kept for clarity.")]
    public Transform girlRoot;

    [Tooltip("Girl Animator (on VRChild).")]
    public Animator girlAnimator;

    [Tooltip("Where the teddy should snap to on the girl (chest/hug point). This is ALSO used for distance check.")]
    public Transform girlHugAnchor;

    [Tooltip("Optional: a checkpoint on the teddy (child transform) placed at the 'center' of where it should be measured from. If null, uses teddy transform.")]
    public Transform teddyCheckPoint;

    [Header("Settings")]
    [Tooltip("How close the teddy checkpoint must be to the girl's hug anchor to trigger the take.")]
    public float takeDistance = 0.35f;

    [Tooltip("Seconds to wait after triggering reach animation before snapping teddy.")]
    public float reachAnimDelay = 0.15f;

    [Tooltip("Animator trigger name on the girl's controller.")]
    public string girlReachTrigger = "Reach";

    [Tooltip("Disable grab after the girl takes it.")]
    public bool disableGrabAfterTaken = true;

    [Tooltip("If true, we freeze the rigidbody (recommended) when snapping.")]
    public bool makeKinematicWhenTaken = true;

    private bool taken;
    private Coroutine takeCo;

    void Reset()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Awake()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (taken) return;
        if (!girlHugAnchor || !girlAnimator || !grab) return;

        // Only allow "take" if player is currently holding it
        if (!grab.isSelected) return;

        // IMPORTANT FIX:
        // Measure distance to the girl's HUG ANCHOR (not girl hips/root),
        // and measure from a teddy checkpoint (or teddy root if not set).
        Vector3 teddyPos = teddyCheckPoint ? teddyCheckPoint.position : transform.position;
        float d = Vector3.Distance(teddyPos, girlHugAnchor.position);

        if (d <= takeDistance && takeCo == null)
        {
            takeCo = StartCoroutine(TakeRoutine());
        }
    }

    IEnumerator TakeRoutine()
    {
        taken = true;

        // Fire reach trigger if it exists
        if (!string.IsNullOrEmpty(girlReachTrigger))
        {
            // Safety: only set trigger if animator actually has it
            bool hasTrigger = false;
            foreach (var p in girlAnimator.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Trigger && p.name == girlReachTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }

            if (hasTrigger)
            {
                girlAnimator.ResetTrigger(girlReachTrigger);
                girlAnimator.SetTrigger(girlReachTrigger);
            }
            else
            {
                Debug.LogWarning($"[TeddyTransferToGirl] Trigger '{girlReachTrigger}' not found on Animator '{girlAnimator.name}'.");
            }
        }

        // Wait a bit so the reach starts
        if (reachAnimDelay > 0f)
            yield return new WaitForSeconds(reachAnimDelay);

        // Force release from player's interactor (so it doesn't fight parenting)
        if (grab.isSelected && grab.interactorsSelecting != null && grab.interactorsSelecting.Count > 0)
        {
            var interactor = grab.interactorsSelecting[0];

            // In XRIT 2.x this is the safe way if interactionManager exists
            if (grab.interactionManager)
                grab.interactionManager.SelectExit(interactor, grab);
            else
                grab.interactionManager?.SelectExit(interactor, grab);
        }

        // Stop physics fighting us
        var rb = GetComponent<Rigidbody>();
        if (rb && makeKinematicWhenTaken)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Snap teddy to girl's hug anchor
        transform.SetParent(girlHugAnchor, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (disableGrabAfterTaken)
            grab.enabled = false;

        Debug.Log("[TeddyTransferToGirl] Teddy parented to girl hug anchor.");

        takeCo = null;
    }

    // Optional: draw the distance sphere in Scene view to tune takeDistance
    void OnDrawGizmosSelected()
    {
        if (!girlHugAnchor) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(girlHugAnchor.position, takeDistance);

        if (teddyCheckPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(teddyCheckPoint.position, 0.02f);
        }
    }
}