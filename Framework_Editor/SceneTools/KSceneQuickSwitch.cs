#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KToolkit
{
    public static class KSceneQuickSwitch
    {
        private const string KEntranceScenePath = "Assets/Scenes/KEntrance.unity";

        [MenuItem("KToolkit/Quick Switch/Open KEntrance Scene")]
        public static void OpenKEntranceScene()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(KEntranceScenePath);
            if (sceneAsset == null)
            {
                EditorUtility.DisplayDialog("KToolkit", $"Scene not found: {KEntranceScenePath}", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(KEntranceScenePath, OpenSceneMode.Single);
        }

        [MenuItem("KToolkit/Quick Switch/Open KEntrance Scene", true)]
        private static bool ValidateOpenKEntranceScene()
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(KEntranceScenePath) != null;
        }
    }
}
#endif
