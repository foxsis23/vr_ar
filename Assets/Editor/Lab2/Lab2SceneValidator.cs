using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Перевірка зібраної VR-сцени за пунктами завдання лабораторної роботи 2.
    /// Меню: Lab2 -> 3. Validate VR Scene
    /// </summary>
    public static class Lab2SceneValidator
    {
        private const string ScenePath = "Assets/Scenes/Lab2_VR.unity";

        [MenuItem("Lab2/3. Validate VR Scene", priority = 3)]
        public static void Validate()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();

            XROrigin origin = Find<XROrigin>(roots);
            TunnelingVignetteController vignette = Find<TunnelingVignetteController>(roots);

            Report("XR Origin (камера + відстеження голови)", origin != null && origin.Camera != null);
            Report("XR Interaction Manager", Find<XRInteractionManager>(roots) != null);
            Report("Teleportation Area (підлога)", Count<TeleportationArea>(roots) >= 1);
            Report("Teleportation Anchor (щонайменше 3)", Count<TeleportationAnchor>(roots) >= 3);
            Report("Плавний рух (ContinuousMoveProvider)", Find<ContinuousMoveProvider>(roots) != null);
            Report("Поворот ривками (SnapTurnProvider)", Find<SnapTurnProvider>(roots) != null);
            Report("Comfort Vignette прив'язано до провайдерів локомоції",
                vignette != null && vignette.locomotionVignetteProviders.Count(p => p.locomotionProvider != null && p.enabled) >= 3);
            Report("Предмети для захоплення (щонайменше 4)", Count<XRGrabInteractable>(roots) >= 4);
            Report("Зона точного розміщення (XRSocketInteractor)", Count<XRSocketInteractor>(roots) >= 1);
            Report("Інформаційна панель HUD", Find<VrHud>(roots) != null);
            Report("XR Interaction Simulator у сцені",
                roots.Any(root => root.name.Contains("XR Interaction Simulator")));
        }

        private static T Find<T>(GameObject[] roots) where T : Component
        {
            return roots.Select(root => root.GetComponentInChildren<T>(true)).FirstOrDefault(found => found != null);
        }

        private static int Count<T>(GameObject[] roots) where T : Component
        {
            return roots.Sum(root => root.GetComponentsInChildren<T>(true).Length);
        }

        private static void Report(string check, bool passed)
        {
            if (passed)
            {
                Debug.Log($"Lab2 [OK] {check}");
            }
            else
            {
                Debug.LogError($"Lab2 [FAIL] {check}");
            }
        }
    }
}
