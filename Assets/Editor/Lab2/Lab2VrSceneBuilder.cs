using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Генератор VR-сцени для лабораторної роботи 2.
    /// Меню: Lab2 -> 2. Build VR Scene
    /// </summary>
    public static class Lab2VrSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Lab2_VR.unity";
        private const int TeleportInteractionLayer = 31;

        private const string RigPrefabName = "XR Origin (XR Rig).prefab";
        private const string VignettePrefabName = "TunnelingVignette.prefab";
        private const string SimulatorPrefabName = "XR Interaction Simulator.prefab";

        [MenuItem("Lab2/2. Build VR Scene", priority = 2)]
        public static void Build()
        {
            GameObject rigPrefab = LoadSamplePrefab(RigPrefabName);
            GameObject simulatorPrefab = LoadSamplePrefab(SimulatorPrefabName);
            GameObject vignettePrefab = LoadSamplePrefab(VignettePrefabName);

            if (rigPrefab == null || simulatorPrefab == null)
            {
                Debug.LogError("Lab2: спочатку виконайте Lab2 -> 1. Setup XR Project (імпорт семплів XRI).");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            new GameObject("XR Interaction Manager", typeof(XRInteractionManager));

            Lab2RoomBuilder.BuildLights();
            GameObject room = Lab2RoomBuilder.BuildRoom();

            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            rig.transform.position = new Vector3(0f, 0f, -2.5f);

            TeleportationProvider teleportationProvider = rig.GetComponentInChildren<TeleportationProvider>(true);
            SetupTeleportArea(Lab2RoomBuilder.FindFloor(room), teleportationProvider);
            BuildTeleportAnchors(teleportationProvider);

            BuildInteractables();
            Lab2RoomBuilder.BuildShelf(new Vector3(-2.6f, 1.5f, 3.85f));
            // Канвас у World Space читається з боку, протилежного його forward,
            // тому панель на північній стіні не розвертаємо.
            Lab2RoomBuilder.BuildHud(new Vector3(0f, 1.8f, 3.9f), Quaternion.identity);

            SetupComfortVignette(rig, vignettePrefab);

            GameObject simulator = (GameObject)PrefabUtility.InstantiatePrefab(simulatorPrefab);
            simulator.name = "XR Interaction Simulator";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Lab2: VR-сцену створено -> {ScenePath}");
        }

        /// <summary>
        /// Уся підлога кімнати стає зоною телепортації.
        /// </summary>
        private static void SetupTeleportArea(GameObject floor, TeleportationProvider provider)
        {
            TeleportationArea area = floor.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << TeleportInteractionLayer;
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            area.teleportationProvider = provider;
            floor.AddComponent<TeleportReporter>();
        }

        /// <summary>
        /// Якірні точки телепортації — фіксовані позиції з наперед заданим напрямком погляду.
        /// </summary>
        private static void BuildTeleportAnchors(TeleportationProvider provider)
        {
            var anchors = new (string name, Vector3 position, float yaw)[]
            {
                ("Anchor Table", new Vector3(0f, 0.02f, 0.9f), 0f),
                ("Anchor Corner", new Vector3(-3f, 0.02f, -3f), 45f),
                ("Anchor Shelf", new Vector3(-2.6f, 0.02f, 2.9f), 0f),
            };

            Material anchorMaterial = Lab2RoomBuilder.CreateMaterial("Lab2_Anchor", new Color(0.20f, 0.85f, 0.55f));

            foreach ((string name, Vector3 position, float yaw) in anchors)
            {
                GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name = name;
                pad.transform.position = position;
                pad.transform.localScale = new Vector3(0.7f, 0.02f, 0.7f);
                pad.GetComponent<Renderer>().sharedMaterial = anchorMaterial;

                GameObject anchorPoint = new GameObject("Anchor Point");
                anchorPoint.transform.SetParent(pad.transform, false);
                anchorPoint.transform.position = position;
                anchorPoint.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

                TeleportationAnchor anchor = pad.AddComponent<TeleportationAnchor>();
                anchor.interactionLayers = 1 << TeleportInteractionLayer;
                anchor.matchOrientation = MatchOrientation.TargetUpAndForward;
                anchor.teleportAnchorTransform = anchorPoint.transform;
                anchor.teleportationProvider = provider;

                pad.AddComponent<TeleportReporter>();
            }
        }

        /// <summary>
        /// Предмети, які беруться контролерами, та зона їх точного розміщення на столі.
        /// </summary>
        private static void BuildInteractables()
        {
            Transform table = Lab2RoomBuilder.BuildTable(new Vector3(0f, 0f, 2.2f));
            float surface = Lab2RoomBuilder.TableHeight + 0.04f;

            Lab2RoomBuilder.BuildGrabbable("Cube", PrimitiveType.Cube,
                table.position + new Vector3(-0.45f, surface + 0.1f, 0f),
                Vector3.one * 0.18f, new Color(0.90f, 0.35f, 0.25f));

            Lab2RoomBuilder.BuildGrabbable("Sphere", PrimitiveType.Sphere,
                table.position + new Vector3(0.45f, surface + 0.1f, 0.2f),
                Vector3.one * 0.18f, new Color(0.25f, 0.50f, 0.95f));

            Lab2RoomBuilder.BuildGrabbable("Cylinder", PrimitiveType.Cylinder,
                table.position + new Vector3(0.55f, surface + 0.12f, -0.2f),
                new Vector3(0.12f, 0.12f, 0.12f), new Color(0.95f, 0.78f, 0.20f));

            Lab2RoomBuilder.BuildGrabbable("Capsule", PrimitiveType.Capsule,
                new Vector3(-2.6f, 1.65f, 3.85f),
                new Vector3(0.12f, 0.12f, 0.12f), new Color(0.65f, 0.30f, 0.85f));

            Lab2RoomBuilder.BuildSnapZone(table.position + new Vector3(0f, surface + 0.12f, 0f), table);
        }

        /// <summary>
        /// Затінення периферійного зору під час плавного руху, поворотів і телепортації.
        /// </summary>
        private static void SetupComfortVignette(GameObject rig, GameObject vignettePrefab)
        {
            if (vignettePrefab == null)
            {
                Debug.LogWarning("Lab2: префаб TunnelingVignette не знайдено — Comfort Vignette не налаштовано.");
                return;
            }

            Camera camera = rig.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                Debug.LogWarning("Lab2: у ригу немає камери — Comfort Vignette не налаштовано.");
                return;
            }

            GameObject vignette = (GameObject)PrefabUtility.InstantiatePrefab(vignettePrefab, camera.transform);
            vignette.transform.localPosition = Vector3.zero;
            vignette.transform.localRotation = Quaternion.identity;

            TunnelingVignetteController controller = vignette.GetComponent<TunnelingVignetteController>();
            if (controller == null)
            {
                return;
            }

            var providers = new List<LocomotionProvider>();
            providers.AddRange(rig.GetComponentsInChildren<ContinuousMoveProvider>(true));
            providers.AddRange(rig.GetComponentsInChildren<SnapTurnProvider>(true));
            providers.AddRange(rig.GetComponentsInChildren<TeleportationProvider>(true));

            controller.locomotionVignetteProviders = providers
                .Where(provider => provider != null)
                .Select(provider => new LocomotionVignetteProvider { locomotionProvider = provider, enabled = true })
                .ToList();

            EditorUtility.SetDirty(controller);
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

            Debug.LogWarning($"Lab2: не знайдено префаб '{fileName}' у Assets/Samples.");
            return null;
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == ScenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
