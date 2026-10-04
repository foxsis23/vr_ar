using UnityEngine;
using UnityEngine.UI;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Просторовий планшет оператора: World-Space Canvas з двома екранами —
    /// покроковий інструктаж (візард) і форма звіту про витік.
    /// </summary>
    public static class Lab3TabletBuilder
    {
        private static readonly Vector2 PanelSize = new Vector2(900f, 1320f);
        private const float PanelScale = 0.00055f;

        public static SpatialTablet Build(PipelineNetwork network)
        {
            GameObject root = new GameObject("Spatial Tablet");

            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Panel", root.transform, PanelSize, PanelScale);
            canvas.transform.position = new Vector3(0f, 1.4f, 0.3f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 14f, new RectOffset(28, 28, 24, 24));

            Lab3UiFactory.CreateText("Title", column, "Планшет оператора — ділянка ГТС-14", 32, FontStyle.Bold, TextAnchor.UpperCenter);

            RectTransform tabRow = Lab3UiFactory.CreateRow("Tabs", column, 12f, 72f);
            Button wizardTab = Lab3UiFactory.CreateButton("Tab Wizard", tabRow, "Інструктаж", 24, 72f, out _);
            Button formTab = Lab3UiFactory.CreateButton("Tab Form", tabRow, "Звіт", 24, 72f, out _);
            Button closeTab = Lab3UiFactory.CreateButton("Tab Close", tabRow, "Закрити", 24, 72f, out _);

            Text tabHint = Lab3UiFactory.CreateText("Tab Hint", column, string.Empty, 22, FontStyle.Italic, TextAnchor.UpperCenter);

            GameObject wizardScreen = BuildWizardScreen(column, out InspectionWizard wizard);
            GameObject formScreen = BuildFormScreen(column, network);

            SpatialTablet tablet = root.AddComponent<SpatialTablet>();
            Lab3Serialized.For(tablet)
                .Reference("panel", canvas.gameObject)
                .Reference("panelCanvas", canvas)
                .Reference("wizardScreen", wizardScreen)
                .Reference("formScreen", formScreen)
                .Reference("wizardTabButton", wizardTab)
                .Reference("formTabButton", formTab)
                .Reference("tabHintText", tabHint)
                .Apply();

            Lab3Serialized.For(wizard).Reference("tablet", tablet).Apply();

            // Кнопка «Закрити» ховає панель — її ж повертає кнопка Menu на контролері.
            CloseButtonBinder binder = closeTab.gameObject.AddComponent<CloseButtonBinder>();
            Lab3Serialized.For(binder).Reference("tablet", tablet).Apply();

            canvas.gameObject.SetActive(false);
            return tablet;
        }

        private static GameObject BuildWizardScreen(RectTransform parent, out InspectionWizard wizard)
        {
            RectTransform screen = Lab3UiFactory.CreateColumn("Wizard Screen", parent, 12f, new RectOffset(0, 0, 0, 0));

            RectTransform stepView = Lab3UiFactory.CreateColumn("Step View", screen, 12f, new RectOffset(0, 0, 0, 0));
            Text progress = Lab3UiFactory.CreateText("Progress", stepView, "Крок 1", 22, FontStyle.Normal, TextAnchor.UpperLeft);
            Text title = Lab3UiFactory.CreateText("Step Title", stepView, "—", 28, FontStyle.Bold, TextAnchor.UpperLeft);
            Text body = Lab3UiFactory.CreateText("Step Body", stepView, "—", 23, FontStyle.Normal, TextAnchor.UpperLeft);
            Text feedback = Lab3UiFactory.CreateText("Feedback", stepView, string.Empty, 22, FontStyle.Italic, TextAnchor.UpperLeft);

            Button answerA = Lab3UiFactory.CreateButton("Answer A", stepView, "A", 22, 92f, out Text labelA);
            Button answerB = Lab3UiFactory.CreateButton("Answer B", stepView, "B", 22, 92f, out Text labelB);
            Button answerC = Lab3UiFactory.CreateButton("Answer C", stepView, "C", 22, 92f, out Text labelC);

            RectTransform navRow = Lab3UiFactory.CreateRow("Navigation", stepView, 12f, 74f);
            Button back = Lab3UiFactory.CreateButton("Back", navRow, "◂ Назад", 24, 74f, out _);
            Button restartInStep = Lab3UiFactory.CreateButton("Restart", navRow, "Спочатку", 24, 74f, out _);
            Button next = Lab3UiFactory.CreateButton("Next", navRow, "Далі ▸", 24, 74f, out Text nextLabel);

            RectTransform resultView = Lab3UiFactory.CreateColumn("Result View", screen, 14f, new RectOffset(0, 0, 0, 0));
            Text result = Lab3UiFactory.CreateText("Result", resultView, "—", 26, FontStyle.Bold, TextAnchor.UpperLeft);
            RectTransform resultRow = Lab3UiFactory.CreateRow("Result Actions", resultView, 12f, 74f);
            Button restartInResult = Lab3UiFactory.CreateButton("Restart Again", resultRow, "Пройти ще раз", 24, 74f, out _);
            Button goToForm = Lab3UiFactory.CreateButton("Go To Form", resultRow, "Перейти до звіту", 24, 74f, out _);

            wizard = screen.gameObject.AddComponent<InspectionWizard>();
            Lab3Serialized.For(wizard)
                .Reference("stepView", stepView.gameObject)
                .Reference("resultView", resultView.gameObject)
                .Reference("progressText", progress)
                .Reference("titleText", title)
                .Reference("bodyText", body)
                .Reference("feedbackText", feedback)
                .References("answerButtons", answerA, answerB, answerC)
                .References("answerLabels", labelA, labelB, labelC)
                .Reference("backButton", back)
                .Reference("nextButton", next)
                .Reference("nextButtonLabel", nextLabel)
                .References("restartButtons", restartInStep, restartInResult)
                .Reference("resultText", result)
                .Reference("goToFormButton", goToForm)
                .Apply();

            return screen.gameObject;
        }

        private static GameObject BuildFormScreen(RectTransform parent, PipelineNetwork network)
        {
            RectTransform screen = Lab3UiFactory.CreateColumn("Form Screen", parent, 10f, new RectOffset(0, 0, 0, 0));

            Lab3UiFactory.CreateText("Form Title", screen, "Звіт про витік", 28, FontStyle.Bold, TextAnchor.UpperLeft);

            Lab3UiFactory.CreateText("Segment Label", screen, "ID ділянки", 22, FontStyle.Normal, TextAnchor.UpperLeft);
            InputField segmentInput = Lab3UiFactory.CreateInputField("Segment Input", screen, "наприклад, ГТС-14/2");

            Lab3UiFactory.CreateText("Operator Label", screen, "Оператор", 22, FontStyle.Normal, TextAnchor.UpperLeft);
            InputField operatorInput = Lab3UiFactory.CreateInputField("Operator Input", screen, "прізвище та ініціали");

            Toggle leakToggle = Lab3UiFactory.CreateToggle("Leak Toggle", screen, "Витік підтверджено газоаналізатором");
            Toggle pressureToggle = Lab3UiFactory.CreateToggle("Pressure Toggle", screen, "Падіння тиску на ділянці");
            Toggle corrosionToggle = Lab3UiFactory.CreateToggle("Corrosion Toggle", screen, "Корозія або дефект зварного шва");

            Lab3UiFactory.CreateText("Pressure Label", screen, "Тиск на ділянці, бар", 22, FontStyle.Normal, TextAnchor.UpperLeft);
            Slider pressureSlider = Lab3UiFactory.CreateSlider("Pressure Slider", screen,
                LeakReport.MinPressureBar, LeakReport.MaxPressureBar, wholeNumbers: false);
            Text pressureValue = Lab3UiFactory.CreateText("Pressure Value", screen, "—", 22, FontStyle.Bold, TextAnchor.UpperRight);

            Lab3UiFactory.CreateText("Severity Label", screen, "Критичність дефекту", 22, FontStyle.Normal, TextAnchor.UpperLeft);
            Slider severitySlider = Lab3UiFactory.CreateSlider("Severity Slider", screen,
                LeakReport.MinSeverity, LeakReport.MaxSeverity, wholeNumbers: true);
            Text severityValue = Lab3UiFactory.CreateText("Severity Value", screen, "—", 22, FontStyle.Bold, TextAnchor.UpperRight);

            RectTransform actions = Lab3UiFactory.CreateRow("Form Actions", screen, 12f, 76f);
            Button read = Lab3UiFactory.CreateButton("Read", actions, "Зчитати з мережі", 22, 76f, out _);
            Button reset = Lab3UiFactory.CreateButton("Reset", actions, "Скинути", 22, 76f, out _);
            Button save = Lab3UiFactory.CreateButton("Save", actions, "Зберегти", 22, 76f, out _);

            Text status = Lab3UiFactory.CreateText("Status", screen, string.Empty, 22, FontStyle.Normal, TextAnchor.UpperLeft);

            LeakReportForm form = screen.gameObject.AddComponent<LeakReportForm>();
            Lab3Serialized.For(form)
                .Reference("segmentIdInput", segmentInput)
                .Reference("operatorInput", operatorInput)
                .Reference("leakToggle", leakToggle)
                .Reference("pressureDropToggle", pressureToggle)
                .Reference("corrosionToggle", corrosionToggle)
                .Reference("pressureSlider", pressureSlider)
                .Reference("pressureValueText", pressureValue)
                .Reference("severitySlider", severitySlider)
                .Reference("severityValueText", severityValue)
                .Reference("saveButton", save)
                .Reference("resetButton", reset)
                .Reference("readFromNetworkButton", read)
                .Reference("statusText", status)
                .Reference("network", network)
                .Apply();

            return screen.gameObject;
        }
    }
}
