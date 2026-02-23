using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            // Point Z at the camera
            transform.LookAt(Camera.main.transform);
            // Rotate 180 degrees around Y so -Z faces the camera (which is the front for UI)
            transform.Rotate(0, 180, 0);
        }
    }
}
