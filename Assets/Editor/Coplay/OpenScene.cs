using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenScene
{
    public static string Execute()
    {
        EditorSceneManager.OpenScene("Assets/_Unity Essentials/Scenes/2_KidsRoom_3D_Scene.unity");
        return "Scene opened";
    }
}