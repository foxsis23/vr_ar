using System.Collections.Generic;
using System.Linq;
using Lab3.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Генератор сцени лабораторної роботи 4 — природні взаємодії руками (Hand Tracking)
    /// на дільниці ремонту газопроводу: складання вузла заміру тиску жестом щипка
    /// та керування стендом опресовки дотиками пальця.
    /// Меню: Lab4 -> 2. Build Hand Tracking Scene
    /// </summary>
    public static class Lab4SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Lab4_HandTracking.unity";

        private const int TeleportInteractionLayer = 31;
        private const string HandsRigPrefabName = "XR Origin Hands (XR Rig).prefab";
        private const string SimulatorPrefabName = "XR Interaction Simulator.prefab";

        [MenuItem("Lab4/2. Build Hand Tracking Scene", priority = 2)]
        public static void Build()
        {
            GameObject rigPrefab = LoadSamplePrefab(HandsRigPrefabName);
            GameObject simulatorPrefab = LoadSamplePrefab(SimulatorPrefabName);

            if (rigPrefab == null || simulatorPrefab == null)
            {
                Debug.LogError("Lab4: спочатку виконайте Lab4 -> 1. Setup Hand Tracking " +
                               "(пакет XR Hands + семпли XRI, зокрема Hands Interaction Demo).");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            new GameObject("XR Interaction Manager", typeof(XRInteractionManager));
            BuildEventSystem();

            Lab3Environment.BuildLights();
            GameObject hall = Lab3Environment.BuildHall();
            BuildBackdropPipeline();

            Lab4StandBuilder.StandRig stand = Lab4StandBuilder.Build();
            Lab4ControlsBuilder.ControlRig controls = Lab4ControlsBuilder.Build();

            Text progressText = BuildAssemblyBoard();
            Text resultText = BuildResultBoard();
            BuildHandTrackingBoard();
            BuildWallHint();

            AssemblyTask task = BuildTask(stand, progressText);
            BuildPressureRig(task, controls, stand, resultText);

            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            rig.transform.position = new Vector3(0f, 0f, -1.4f);
            SetupTeleportArea(Lab3Environment.FindFloor(hall), rig.GetComponentInChildren<TeleportationProvider>(true));

            GameObject simulator = (GameObject)PrefabUtility.InstantiatePrefab(simulatorPrefab);
            simulator.name = "XR Interaction Simulator";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lab4: сцену відстеження рук створено -> {ScenePath}");
        }

        private static AssemblyTask BuildTask(Lab4StandBuilder.StandRig stand, Text progressText)
        {
            GameObject taskObject = new GameObject("Assembly Task");
            AssemblyTask task = taskObject.AddComponent<AssemblyTask>();

            Lab3Serialized.For(task)
                .References("sockets", stand.Sockets.Cast<Object>().ToArray())
                .Reference("progressText", progressText)
                .Apply();

            return task;
        }

        private static void BuildPressureRig(
            AssemblyTask task,
            Lab4ControlsBuilder.ControlRig controls,
            Lab4StandBuilder.StandRig stand,
            Text resultText)
        {
            PressureTestRig rig = controls.Root.AddComponent<PressureTestRig>();

            Lab3Serialized.For(rig)
                .Reference("task", task)
                .Reference("powerSwitch", controls.PowerSwitch)
                .Reference("pressureSlider", controls.PressureSlider)
                .Reference("startButton", controls.StartButton)
                .Reference("resetButton", controls.ResetButton)
                .Reference("resultText", resultText)
                .Reference("statusLamp", controls.StatusLamp)
                .References("parts", stand.Parts.Cast<Object>().ToArray())
                .Apply();
        }

        /// <summary>Нитка газопроводу вздовж північної стіни — тло дільниці ремонту.</summary>
        private static void BuildBackdropPipeline()
        {
            Material steel = Lab4Materials.Steel;

            GameObject pipeline = new GameObject("Backdrop Pipeline");
            pipeline.transform.position = new Vector3(0f, 1.3f, 4.2f);

            Lab3Environment.CreateCylinder("Pipe", Vector3.zero, new Vector3(0f, 0f, 90f),
                new Vector3(0.3f, 4.5f, 0.3f), steel, pipeline.transform);

            foreach (float x in new[] { -3.6f, -1.2f, 1.2f, 3.6f })
            {
                Lab3Environment.CreateBox("Support", new Vector3(x, -0.65f, 0f),
                    new Vector3(0.12f, 1.3f, 0.12f), steel, pipeline.transform);
            }
        }

        /// <summary>Табло складання вузла: список пазів і прогрес.</summary>
        private static Text BuildAssemblyBoard()
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Assembly Board", null, new Vector2(1000f, 620f), 0.0014f);
            canvas.transform.position = new Vector3(2.6f, 2.2f, 4.9f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 12f, new RectOffset(28, 28, 24, 24));
            Lab3UiFactory.CreateText("Title", column, "Вузол заміру тиску ГРП", 32, FontStyle.Bold, TextAnchor.UpperCenter);
            return Lab3UiFactory.CreateText("Progress", column, "Складання вузла: 0/4", 26, FontStyle.Normal, TextAnchor.UpperLeft);
        }

        /// <summary>Табло результату опресовки.</summary>
        private static Text BuildResultBoard()
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Result Board", null, new Vector2(1000f, 320f), 0.0014f);
            canvas.transform.position = new Vector3(0f, 2.65f, 4.9f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 10f, new RectOffset(28, 28, 20, 20));
            Lab3UiFactory.CreateText("Title", column, "Стенд опресовки", 30, FontStyle.Bold, TextAnchor.UpperCenter);
            return Lab3UiFactory.CreateText("Result", column, "Стенд готовий.", 26, FontStyle.Normal, TextAnchor.UpperCenter);
        }

        /// <summary>Табло стану відстеження рук — суглоби, зазор щипка, кількість жестів.</summary>
        private static void BuildHandTrackingBoard()
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Hand Tracking Board", null, new Vector2(1000f, 620f), 0.0014f);
            canvas.transform.position = new Vector3(-2.6f, 2.2f, 4.9f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 12f, new RectOffset(28, 28, 24, 24));
            Lab3UiFactory.CreateText("Title", column, "Відстеження рук", 32, FontStyle.Bold, TextAnchor.UpperCenter);
            Text status = Lab3UiFactory.CreateText("Status", column, "Підсистема рук не запущена.", 26, FontStyle.Normal, TextAnchor.UpperLeft);

            HandTrackingMonitor monitor = canvas.gameObject.AddComponent<HandTrackingMonitor>();
            Lab3Serialized.For(monitor).Reference("statusText", status).Apply();
        }

        /// <summary>Довідкова панель: що перевіряється в лабораторній і якими клавішами симулятора.</summary>
        private static void BuildWallHint()
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Wall Hint", null, new Vector2(1200f, 640f), 0.0014f);
            canvas.transform.position = new Vector3(0f, 2.2f, -4.9f);
            canvas.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 14f, new RectOffset(28, 28, 24, 24));
            Lab3UiFactory.CreateText("Title", column, "Лабораторна 4 — взаємодії руками", 34, FontStyle.Bold, TextAnchor.UpperCenter);
            Lab3UiFactory.CreateText("Body", column,
                "[ або ] — активувати ліву/праву руку; друге натискання тієї самої клавіші\n" +
                "перемикає режим «контролер ↔ рука» (Hand Tracking).\n" +
                "M — щипок (Pinch): узяти деталь з лотка й відпустити в монтажному пазу.\n" +
                "N — поза «вказівний палець» (Poke): тумблер, слайдер і кнопки пульта.\n" +
                "K — захват долонею, O — відкрита долоня, P — кулак, L — великий палець.\n" +
                "Права кнопка миші — огляд, W/A/S/D — переміщення, H — голова.\n" +
                "Порядок: зберіть 4 деталі у вузол, увімкніть живлення, виставте 6–8 бар, «Пуск».",
                24, FontStyle.Normal, TextAnchor.UpperLeft);
        }

        /// <summary>
        /// EventSystem з <see cref="XRUIInputModule"/> — потрібен навіть тут, бо підписи
        /// органів керування лежать на World-Space Canvas.
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

            Debug.LogWarning($"Lab4: не знайдено префаб '{fileName}' у Assets/Samples.");
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
