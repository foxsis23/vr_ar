using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Генератор сцени лабораторної роботи 3 — просторові інтерфейси та покрокові форми
    /// на прикладі огляду й дефектоскопії газопроводу.
    /// Меню: Lab3 -> 2. Build Spatial UI Scene
    /// </summary>
    public static class Lab3SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Lab3_SpatialUI.unity";

        private const int TeleportInteractionLayer = 31;
        private const string RigPrefabName = "XR Origin (XR Rig).prefab";
        private const string SimulatorPrefabName = "XR Interaction Simulator.prefab";

        [MenuItem("Lab3/2. Build Spatial UI Scene", priority = 2)]
        public static void Build()
        {
            GameObject rigPrefab = LoadSamplePrefab(RigPrefabName);
            GameObject simulatorPrefab = LoadSamplePrefab(SimulatorPrefabName);

            if (rigPrefab == null || simulatorPrefab == null)
            {
                Debug.LogError("Lab3: спочатку виконайте Lab3 -> 1. Setup XR Project (імпорт семплів XRI).");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            new GameObject("XR Interaction Manager", typeof(XRInteractionManager));
            BuildEventSystem();

            Lab3Environment.BuildLights();
            GameObject hall = Lab3Environment.BuildHall();

            GameObject networkObject = new GameObject("Pipeline Network");
            PipelineNetwork network = networkObject.AddComponent<PipelineNetwork>();

            Lab3PipelineBuilder.PipelineRig pipeline = Lab3PipelineBuilder.Build();
            Lab3ConsoleBuilder.ConsoleRig consoleRig = Lab3ConsoleBuilder.Build(network, pipeline.Valves);
            SpatialTablet tablet = Lab3TabletBuilder.Build(network);

            WireNetwork(network, pipeline, consoleRig.ReadoutText);
            Lab3Serialized.For(consoleRig.Console).Reference("tablet", tablet).Apply();

            BuildWallHint();

            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            rig.transform.position = new Vector3(0f, 0f, -1.8f);
            SetupTeleportArea(Lab3Environment.FindFloor(hall), rig.GetComponentInChildren<TeleportationProvider>(true));

            GameObject simulator = (GameObject)PrefabUtility.InstantiatePrefab(simulatorPrefab);
            simulator.name = "XR Interaction Simulator";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lab3: сцену просторового інтерфейсу створено -> {ScenePath}");
        }

        /// <summary>
        /// Зв'язує модель тиску з ділянками, вентилями, індикатором витоку та показником консолі.
        /// Вентилі теж отримують зворотне посилання, щоб перемикання одразу перераховувало тиск.
        /// </summary>
        private static void WireNetwork(PipelineNetwork network, Lab3PipelineBuilder.PipelineRig pipeline, Text readout)
        {
            Lab3Serialized.For(network)
                .Number("sourcePressureBar", PipelineSegment.NominalBar)
                .References("segments", pipeline.Segments.Cast<Object>().ToArray())
                .References("valves", pipeline.Valves.Cast<Object>().ToArray())
                .Number("leakSegmentIndex", 1)
                .Reference("leakIndicator", pipeline.LeakIndicator)
                .Reference("readoutText", readout)
                .Apply();

            foreach (ValveControl valve in pipeline.Valves)
            {
                Lab3Serialized.For(valve).Reference("network", network).Apply();
            }
        }

        /// <summary>
        /// EventSystem з <see cref="XRUIInputModule"/> — без нього ні промінь, ні дотик
        /// не доходять до елементів World-Space Canvas.
        /// </summary>
        private static void BuildEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem.AddComponent<XRUIInputModule>();
        }

        private static void SetupTeleportArea(GameObject floor, TeleportationProvider provider)
        {
            TeleportationArea area = floor.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << TeleportInteractionLayer;
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            area.teleportationProvider = provider;
        }

        /// <summary>Довідкова панель на стіні: що саме перевіряється в цій лабораторній і якими клавішами.</summary>
        private static void BuildWallHint()
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Wall Hint", null, new Vector2(1100f, 620f), 0.0014f);
            canvas.transform.position = new Vector3(0f, 2.45f, 4.9f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 14f, new RectOffset(28, 28, 24, 24));
            Lab3UiFactory.CreateText("Title", column, "Лабораторна 3 — просторовий інтерфейс", 34, FontStyle.Bold, TextAnchor.UpperCenter);
            Lab3UiFactory.CreateText("Body", column,
                "Menu на контролері (клавіша M у симуляторі) — відкрити/закрити планшет оператора.\n" +
                "Промінь контролера — наведення та клік по елементах планшета й консолі.\n" +
                "Палець / кінчик контролера — пряме натискання кнопок консолі (Direct Poke).\n" +
                "Наведення на трубу або вентиль — спливаюча картка з поточним тиском і станом.\n" +
                "Симулятор: [ / ] — ліва/права рука, H — голова, права кнопка миші — огляд,\n" +
                "T — Trigger (клік по UI), G — Grip, I/J/K/L — стік контролера.\n" +
                "Поки активне поле введення форми, керування вимкнено; Enter — завершити введення.",
                24, FontStyle.Normal, TextAnchor.UpperLeft);
        }

        private static GameObject LoadSamplePrefab(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Samples" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(fileName, System.StringComparison.Ordinal))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            Debug.LogWarning($"Lab3: не знайдено префаб '{fileName}' у Assets/Samples.");
            return null;
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(entry => entry.path == ScenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
