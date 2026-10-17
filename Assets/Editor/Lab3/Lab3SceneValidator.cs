using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Перевірка сцени за пунктами завдання лабораторної 3.
    /// Меню: Lab3 -> 3. Validate Scene
    /// </summary>
    public static class Lab3SceneValidator
    {
        [MenuItem("Lab3/3. Validate Scene", priority = 3)]
        public static void Validate()
        {
            if (EditorSceneManager.GetActiveScene().path != Lab3SceneBuilder.ScenePath)
            {
                EditorSceneManager.OpenScene(Lab3SceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            var report = new List<string>();

            SpatialTablet tablet = Object.FindFirstObjectByType<SpatialTablet>(FindObjectsInactive.Include);
            Check(report, "Просторовий планшет (World-Space Canvas) за запитом", tablet != null);

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int worldCanvases = 0;
            int raycasters = 0;
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.WorldSpace)
                {
                    continue;
                }

                worldCanvases++;
                if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() != null)
                {
                    raycasters++;
                }
            }

            Check(report, $"World-Space Canvas у сцені: {worldCanvases}", worldCanvases >= 3);
            Check(report, $"TrackedDeviceGraphicRaycaster на кожному канвасі: {raycasters}/{worldCanvases}",
                worldCanvases > 0 && raycasters == worldCanvases);

            Check(report, "EventSystem з XRUIInputModule",
                Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null &&
                Object.FindFirstObjectByType<XRUIInputModule>(FindObjectsInactive.Include) != null);

            Check(report, "Дистанційний вказівник (Near-Far / Ray Interactor)",
                Object.FindFirstObjectByType<NearFarInteractor>(FindObjectsInactive.Include) != null ||
                Object.FindFirstObjectByType<XRRayInteractor>(FindObjectsInactive.Include) != null);

            Check(report, "Прямий дотик (XRPokeInteractor)",
                Object.FindFirstObjectByType<XRPokeInteractor>(FindObjectsInactive.Include) != null);

            EquipmentInfoCard[] cards = Object.FindObjectsByType<EquipmentInfoCard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Check(report, $"Спливаючі інформаційні картки обладнання: {cards.Length}", cards.Length >= 3);

            LeakReportForm form = Object.FindFirstObjectByType<LeakReportForm>(FindObjectsInactive.Include);
            Check(report, "Форма звіту з полями введення, перемикачами та слайдерами", form != null);
            Check(report, $"Перемикачі (Toggle): {Count<Toggle>()}", Count<Toggle>() >= 3);
            Check(report, $"Слайдери (Slider): {Count<Slider>()}", Count<Slider>() >= 2);
            Check(report, $"Поля введення (InputField): {Count<InputField>()}", Count<InputField>() >= 2);
            Check(report, $"Кнопки (Button): {Count<Button>()}", Count<Button>() >= 8);

            InspectionWizard wizard = Object.FindFirstObjectByType<InspectionWizard>(FindObjectsInactive.Include);
            Check(report, $"Покроковий візард: кроків {InspectionProgram.Steps.Count}, питань {InspectionProgram.QuestionCount}",
                wizard != null && InspectionProgram.QuestionCount >= 3);

            Check(report, "Модель тиску трубопроводу",
                Object.FindFirstObjectByType<PipelineNetwork>(FindObjectsInactive.Include) != null);
            Check(report, $"Регулювальні вентилі: {Count<ValveControl>()}", Count<ValveControl>() >= 2);

            StringBuilder output = new StringBuilder("Lab3: перевірка сцени\n");
            foreach (string line in report)
            {
                output.AppendLine(line);
            }

            Debug.Log(output.ToString());
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
