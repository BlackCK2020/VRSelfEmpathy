using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class InspectAnimatorController
{
    public static string Execute()
    {
        string path = "Assets/Animation_comp/ChildController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) return "Controller not found at " + path;

        string info = "Layers:\n";
        foreach (var layer in controller.layers)
        {
            info += $"- {layer.name}\n";
            info += "  States:\n";
            foreach (var state in layer.stateMachine.states)
            {
                info += $"    - {state.state.name}\n";
            }
        }

        info += "\nParameters:\n";
        foreach (var param in controller.parameters)
        {
            info += $"- {param.name} ({param.type})\n";
        }

        return info;
    }
}
