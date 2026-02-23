using UnityEngine;

public class CheckThermosBounds
{
    public static string Execute()
    {
        GameObject thermos = GameObject.Find("Thermos2");
        if (thermos == null) return "Thermos2 not found";

        Renderer[] renderers = thermos.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return "No renderers found";

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return $"Bounds Center: {bounds.center}, Size: {bounds.size}, Min: {bounds.min}, Max: {bounds.max}";
    }
}
