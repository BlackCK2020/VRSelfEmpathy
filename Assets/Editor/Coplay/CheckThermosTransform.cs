using UnityEngine;

public class CheckThermosTransform
{
    public static string Execute()
    {
        GameObject thermos = GameObject.Find("Thermos2");
        if (thermos == null) return "Thermos2 not found";

        return $"Position: {thermos.transform.position}, Rotation: {thermos.transform.rotation.eulerAngles}, Scale: {thermos.transform.localScale}";
    }
}
