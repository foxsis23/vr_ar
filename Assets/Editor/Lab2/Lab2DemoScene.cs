using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Готова демо-сцена з семпла XRI Starter Assets — для порівняння з власноруч зібраною сценою.
    /// Оригінал не змінюється: робиться копія в Assets/Scenes, куди додається XR Interaction Simulator.
    /// Меню: Lab2 -> 6. Build XRI Demo Scene
    /// </summary>
    public static class Lab2DemoScene
    {
        private const string SourceScenePath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/DemoScene.unity";
        private const string TargetScenePath = "Assets/Scenes/Lab2_XRI_Demo.unity";
        private const string SimulatorPrefabName = "XR Interaction Simulator.prefab";

        [MenuItem("Lab2/6. Build XRI Demo Scene", priority = 6)]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath) == null)
            {
                Debug.LogError($"Lab2: демо-сцену не знайдено ({SourceScenePath}). Спочатку Lab2 -> 1. Setup XR Project.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath) != null)
            {
                AssetDatabase.DeleteAsset(TargetScenePath);
            }

            if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
            {
                Debug.LogError("Lab2: не вдалося скопіювати демо-сцену.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            AddSimulator(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TargetScenePath);
            AddSceneToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lab2: демо-сцену XRI підготовано -> {TargetScenePath}");
        }

        /// <summary>
        /// Демо-сцена розрахована на реальний шолом, тому додаємо симулятор для перевірки в редакторі.
        /// </summary>
        private static void AddSimulator(Scene scene)
        {
            if (scene.GetRootGameObjects().Any(root => root.name.Contains("XR Interaction Simulator")))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Samples" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(SimulatorPrefabName, System.StringComparison.Ordinal))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject simulator = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                simulator.name = "XR Interaction Simulator";
                return;
            }

            Debug.LogWarning("Lab2: префаб XR Interaction Simulator не знайдено — демо-сцена без симулятора.");
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == TargetScenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(TargetScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
