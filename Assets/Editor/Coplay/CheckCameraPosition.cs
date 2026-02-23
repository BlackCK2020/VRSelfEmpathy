using UnityEngine;

public class CheckCameraPosition
{
    public static string Execute()
    {
        Camera cam = Camera.main;
        if (cam == null) return "Main Camera not found";
        return $"Camera Position: {cam.transform.position}, Rotation: {cam.transform.rotation.eulerAngles}";
    }
}
