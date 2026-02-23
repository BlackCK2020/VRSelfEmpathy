using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ThermosGrabController : MonoBehaviour
{
    public GameObject thermos;
    public Button grabButton;
    public Button giveButton;
    public ActionBasedController leftController;
    public ActionBasedController rightController;

    private bool isGrabbed = false;

    void Start()
    {
        if (thermos == null) thermos = this.gameObject;
        if (grabButton != null) grabButton.onClick.AddListener(() => GrabThermos(null));
        if (giveButton != null) 
        {
            giveButton.onClick.AddListener(GiveThermos);
            giveButton.gameObject.SetActive(false); // Hide initially
        }
        
        // Try to find controllers if not assigned
        if (leftController == null || rightController == null)
        {
            var controllers = FindObjectsOfType<ActionBasedController>();
            foreach (var c in controllers)
            {
                if (c.name.Contains("Left") && leftController == null) leftController = c;
                if (c.name.Contains("Right") && rightController == null) rightController = c;
            }
        }
    }

    void Update()
    {
        if (!isGrabbed)
        {
            // Keyboard 'F' for Grab
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                GrabThermos(null);
            }

            // VR Grab Button (Left)
            if (leftController != null && leftController.selectAction.action != null && leftController.selectAction.action.WasPressedThisFrame())
            {
                GrabThermos(leftController.transform);
            }

            // VR Grab Button (Right)
            if (rightController != null && rightController.selectAction.action != null && rightController.selectAction.action.WasPressedThisFrame())
            {
                GrabThermos(rightController.transform);
            }
        }
        else
        {
            // Keyboard 'F' for Give
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                GiveThermos();
            }

            // VR 'G' for Give (Keyboard fallback for testing)
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                GiveThermos();
            }

            // VR Controller Inputs for Give (e.g., Trigger or Grip again)
            // Assuming 'select' action is used for grab, we can use it to 'give' or 'release'
            if (leftController != null && leftController.selectAction.action != null && leftController.selectAction.action.WasPressedThisFrame())
            {
                GiveThermos();
            }
            if (rightController != null && rightController.selectAction.action != null && rightController.selectAction.action.WasPressedThisFrame())
            {
                GiveThermos();
            }
        }
    }

    public void GrabThermos(Transform targetController)
    {
        if (isGrabbed) return;
        
        isGrabbed = true;

        // Determine target parent
        Transform target = targetController;
        if (target == null)
        {
            // If triggered by mouse/keyboard/button, attach to right controller if available, else camera
            if (rightController != null)
            {
                target = rightController.transform;
            }
            else if (Camera.main != null)
            {
                target = Camera.main.transform;
            }
        }

        if (target != null)
        {
            thermos.transform.SetParent(target);
            thermos.transform.localPosition = Vector3.zero; 
            thermos.transform.localRotation = Quaternion.identity;
        }

        // Highlight Green
        Renderer[] renderers = thermos.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // Create a new material instance to avoid modifying the shared material
            r.material = new Material(r.material);
            r.material.color = Color.green;
        }

        // Hide Grab Button
        if (grabButton != null) grabButton.gameObject.SetActive(false);

        // Show Give Button
        if (giveButton != null) giveButton.gameObject.SetActive(true);
    }

    public void GiveThermos()
    {
        if (!isGrabbed) return;

        // Hide Give Button
        if (giveButton != null) giveButton.gameObject.SetActive(false);

        // Destroy Thermos (or deactivate)
        thermos.SetActive(false);
        // Destroy(thermos); // Use Destroy if you want to completely remove it
    }
}
