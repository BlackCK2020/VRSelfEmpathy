using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class SetupThermosGrab
{
    public static string Execute()
    {
        GameObject thermos = GameObject.Find("Thermos2");
        if (thermos == null) return "Thermos2 not found";

        GameObject child = GameObject.Find("Child with text/VRChild");
        if (child == null) return "Child not found";

        // --- Grab Button (Thermos) ---
        // Remove ALL existing canvases on Thermos
        bool found = true;
        while (found)
        {
            found = false;
            Transform[] children = thermos.GetComponentsInChildren<Transform>(true);
            foreach (Transform c in children)
            {
                if (c != null && c.name == "ThermosCanvas")
                {
                    Object.DestroyImmediate(c.gameObject);
                    found = true;
                    break; 
                }
            }
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("ThermosCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.transform.SetParent(thermos.transform);
        canvasObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        canvasObj.transform.localPosition = new Vector3(7.5f, 10, 0); 
        
        if (Camera.main != null)
        {
             canvasObj.transform.LookAt(Camera.main.transform);
        }

        canvasObj.AddComponent<Billboard>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Create Grab Button
        GameObject buttonObj = new GameObject("GrabButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.grey;
        Button grabButton = buttonObj.AddComponent<Button>();
        buttonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 60);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = "Grab";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 24;
        textObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        textObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        textObj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        textObj.GetComponent<RectTransform>().offsetMax = Vector2.zero;


        // --- Give Button (Child) ---
        // Remove existing GiveCanvas if any
        Transform existingGiveCanvas = child.transform.Find("GiveCanvas");
        if (existingGiveCanvas != null) Object.DestroyImmediate(existingGiveCanvas.gameObject);

        GameObject giveCanvasObj = new GameObject("GiveCanvas");
        Canvas giveCanvas = giveCanvasObj.AddComponent<Canvas>();
        giveCanvas.renderMode = RenderMode.WorldSpace;
        giveCanvasObj.transform.SetParent(child.transform);
        
        // Position at chest level
        // Child height is ~1.5m. Chest is around 1.0m - 1.2m.
        // Child local scale is 1.
        giveCanvasObj.transform.localPosition = new Vector3(0, 1.2f, 0.3f); // Slightly forward
        giveCanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        if (Camera.main != null)
        {
             giveCanvasObj.transform.LookAt(Camera.main.transform);
        }

        giveCanvasObj.AddComponent<Billboard>();
        CanvasScaler giveScaler = giveCanvasObj.AddComponent<CanvasScaler>();
        giveScaler.dynamicPixelsPerUnit = 10;
        giveCanvasObj.AddComponent<GraphicRaycaster>();
        giveCanvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Create Give Button
        GameObject giveButtonObj = new GameObject("GiveButton");
        giveButtonObj.transform.SetParent(giveCanvasObj.transform, false);
        Image giveButtonImage = giveButtonObj.AddComponent<Image>();
        giveButtonImage.color = Color.grey;
        Button giveButton = giveButtonObj.AddComponent<Button>();
        giveButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 60);

        GameObject giveTextObj = new GameObject("Text");
        giveTextObj.transform.SetParent(giveButtonObj.transform, false);
        Text giveText = giveTextObj.AddComponent<Text>();
        giveText.text = "Give";
        giveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        giveText.color = Color.white;
        giveText.alignment = TextAnchor.MiddleCenter;
        giveText.fontSize = 24;
        giveTextObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        giveTextObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        giveTextObj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        giveTextObj.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // --- Controller Setup ---
        ThermosGrabController controller = thermos.GetComponent<ThermosGrabController>();
        if (controller == null) controller = thermos.AddComponent<ThermosGrabController>();
        
        controller.thermos = thermos;
        controller.grabButton = grabButton;
        controller.giveButton = giveButton;

        var controllers = Object.FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
        foreach (var c in controllers)
        {
            if (c.name.Contains("Left")) controller.leftController = c;
            if (c.name.Contains("Right")) controller.rightController = c;
        }

        // Ensure Give Button is initially hidden
        giveButton.gameObject.SetActive(false);

        return "Setup complete";
    }
}
