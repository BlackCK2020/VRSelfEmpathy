using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AddHappyState
{
    public static string Execute()
    {
        string path = "Assets/Animation_comp/ChildController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) return "Controller not found at " + path;

        // Add Parameter
        controller.AddParameter("IsHappy", AnimatorControllerParameterType.Trigger);

        // Add State
        var rootStateMachine = controller.layers[0].stateMachine;
        var happyState = rootStateMachine.AddState("Happy");
        
        // Assign Animation Clip
        // Assuming Happy.anim is in the same folder or I can find it
        AnimationClip happyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation_comp/Happy.anim");
        if (happyClip == null) return "Happy.anim not found";
        happyState.motion = happyClip;

        // Add Transition from Any State
        var transition = rootStateMachine.AddAnyStateTransition(happyState);
        transition.AddCondition(AnimatorConditionMode.If, 0, "IsHappy");
        transition.duration = 0.25f;

        return "Added Happy state and transition";
    }
}
