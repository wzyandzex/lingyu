using UnityEditor;
using UnityEngine;

namespace Aetherion.Editor
{
    /// <summary>Menu helpers for VS0 recovery when project opens empty or in Safe Mode after fix.</summary>
    public static class Vs0Menus
    {
        [MenuItem("Aetherion/VS0/Open Boot Scene")]
        public static void OpenBoot()
        {
            var path = "Assets/_Project/Scenes/Boot/Boot.unity";
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene == null)
            {
                Debug.LogError("[VS0] Boot.unity not found at " + path);
                return;
            }

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            Debug.Log("[VS0] Opened Boot.unity — press Play to spawn R01 greybox.");
        }

        [MenuItem("Aetherion/VS0/Force Rebuild R01 In Play Mode Info")]
        public static void Info()
        {
            Debug.Log("[VS0] R01 shell is built at runtime by GameBootstrap/RuntimeWorldBuilder when you press Play.");
        }
    }
}
