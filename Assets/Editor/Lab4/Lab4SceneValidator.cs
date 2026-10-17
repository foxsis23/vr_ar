using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Перевірка сцени за пунктами завдання лабораторної 4.
    /// Меню: Lab4 -> 3. Validate Scene
    /// </summary>
    public static class Lab4SceneValidator
    {
        [MenuItem("Lab4/3. Validate Scene", priority = 3)]
        public static void Validate()
        {
            if (EditorSceneManager.GetActiveScene().path != Lab4SceneBuilder.ScenePath)
            {
                EditorSceneManager.OpenScene(Lab4SceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            var report = new List<string>();

            Check(report, "Пакет XR Hands встановлено", IsHandsPackageInstalled());

            Check(report, "Риг із підтримкою рук (XRInputModalityManager)",
                Object.FindFirstObjectByType<XRInputModalityManager>(FindObjectsInactive.Include) != null);

            Check(report, "Прямий дотик пальцем (XRPokeInteractor на ригу)",
                Object.FindFirstObjectByType<XRPokeInteractor>(FindObjectsInactive.Include) != null);

            Check(report, "Обробка позицій пальців і жесту щипка (HandTrackingMonitor)",
                Object.FindFirstObjectByType<HandTrackingMonitor>(FindObjectsInactive.Include) != null);

            int parts = Count<AssemblyPart>();
            Check(report, $"Змінні деталі з захопленням щипком: {parts}", parts >= 4);
            Check(report, $"Деталі є XRGrabInteractable: {Count<XRGrabInteractable>()}", Count<XRGrabInteractable>() >= parts);

            int sockets = Count<AssemblySocket>();
            Check(report, $"Монтажні пази з перевіркою деталі: {sockets}", sockets >= 4);
            Check(report, $"Пази є XRSocketInteractor: {Count<XRSocketInteractor>()}", Count<XRSocketInteractor>() >= sockets);

            Check(report, "Сценарій збирання вузла (AssemblyTask)",
                Object.FindFirstObjectByType<AssemblyTask>(FindObjectsInactive.Include) != null);

            Check(report, $"Об'ємні кнопки під палець: {Count<PokePushButton>()}", Count<PokePushButton>() >= 2);
            Check(report, $"Тумблери під палець: {Count<ToggleSwitch3D>()}", Count<ToggleSwitch3D>() >= 1);
            Check(report, $"Слайдери під палець: {Count<PokeSlider>()}", Count<PokeSlider>() >= 1);
            Check(report, $"Фільтри дотику XRPokeFilter: {Count<XRPokeFilter>()}", Count<XRPokeFilter>() >= 3);

            Check(report, "Стенд опресовки зв'язано (PressureTestRig)",
                Object.FindFirstObjectByType<PressureTestRig>(FindObjectsInactive.Include) != null);

            Check(report, "Симулятор для тестування без шолома",
                GameObject.Find("XR Interaction Simulator") != null);

            StringBuilder output = new StringBuilder("Lab4: перевірка сцени\n");
            foreach (string line in report)
            {
                output.AppendLine(line);
            }

            Debug.Log(output.ToString());
        }

        /// <summary>
        /// Пакет шукаємо за типом підсистеми рук: якщо складання пройшло, тип присутній,
        /// отже й пакет на місці — це надійніше, ніж читати manifest.json.
        /// </summary>
        private static bool IsHandsPackageInstalled()
        {
            return System.Type.GetType("UnityEngine.XR.Hands.XRHandSubsystem, Unity.XR.Hands") != null;
        }

        private static int Count<T>() where T : Component
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }

        private static void Check(ICollection<string> report, string label, bool passed)
        {
            report.Add($"{(passed ? "OK  " : "НЕМА")} — {label}");
        }
    }
}
