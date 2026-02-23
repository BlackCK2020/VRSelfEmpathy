using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveScene
{
    public static string Execute()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        return "Scene saved";
    }
}
