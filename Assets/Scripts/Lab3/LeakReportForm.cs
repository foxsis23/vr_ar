using System;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Просторова форма звіту про витік: перемикачі ознак дефекту, слайдери тиску й критичності,
    /// поля введення та кнопки збереження/скидання. Дані валідуються перед збереженням.
    /// </summary>
    public class LeakReportForm : MonoBehaviour
    {
        private const float DefaultPressureBar = PipelineSegment.NominalBar;
        private const int DefaultSeverity = 3;

        [SerializeField] private InputField segmentIdInput;
        [SerializeField] private InputField operatorInput;
        [SerializeField] private Toggle leakToggle;
        [SerializeField] private Toggle pressureDropToggle;
        [SerializeField] private Toggle corrosionToggle;
        [SerializeField] private Slider pressureSlider;
        [SerializeField] private Text pressureValueText;
        [SerializeField] private Slider severitySlider;
        [SerializeField] private Text severityValueText;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button readFromNetworkButton;
        [SerializeField] private Text statusText;
        [SerializeField] private PipelineNetwork network;

        /// <summary>Останній успішно збережений звіт. Передача на сервер — наступна лабораторна.</summary>
        public LeakReport SavedReport { get; private set; }

        public void Save()
        {
            LeakReport report = BuildReport();
            string error = report.Validate();

            if (error != null)
            {
                SetStatus($"Помилка: {error}", warning: true);
                return;
            }

            SavedReport = report;
            SetStatus($"Звіт збережено локально {report.CreatedAtUtc.ToLocalTime():HH:mm:ss}\n{report.ToSummary()}", warning: false);
        }

        public void ResetForm()
        {
            SetText(segmentIdInput, string.Empty);
            SetText(operatorInput, string.Empty);
            SetToggle(leakToggle, false);
            SetToggle(pressureDropToggle, false);
            SetToggle(corrosionToggle, false);
            SetSlider(pressureSlider, DefaultPressureBar);
            SetSlider(severitySlider, DefaultSeverity);
            SetStatus("Форму очищено. Заповніть поля та збережіть звіт.", warning: false);
        }

        /// <summary>
        /// Підтягує фактичні показники аварійної ділянки з моделі трубопроводу,
        /// щоб оператор не переписував значення з манометра вручну.
        /// </summary>
        public void ReadFromNetwork()
        {
            if (network == null)
            {
                SetStatus("Мережа трубопроводу недоступна.", warning: true);
                return;
            }

            PipelineSegment segment = network.LeakSegment;
            if (segment == null)
            {
                SetStatus("Аварійну ділянку не визначено.", warning: true);
                return;
            }

            SetText(segmentIdInput, segment.SegmentId);
            SetSlider(pressureSlider, segment.PressureBar);
            SetToggle(leakToggle, network.LeakActive);
            SetToggle(pressureDropToggle, segment.PressureBar < PipelineSegment.NominalBar * 0.85f);
            SetStatus($"Зчитано з мережі: {segment.SegmentId}, {segment.PressureBar:F1} бар.", warning: false);
        }

        private void Awake()
        {
            ConfigureSliders();
            ResetForm();
        }

        private void OnEnable()
        {
            Bind(saveButton, Save, add: true);
            Bind(resetButton, ResetForm, add: true);
            Bind(readFromNetworkButton, ReadFromNetwork, add: true);

            if (pressureSlider != null)
            {
                pressureSlider.onValueChanged.AddListener(HandlePressureChanged);
            }

            if (severitySlider != null)
            {
                severitySlider.onValueChanged.AddListener(HandleSeverityChanged);
            }

            RefreshSliderLabels();
        }

        private void OnDisable()
        {
            Bind(saveButton, Save, add: false);
            Bind(resetButton, ResetForm, add: false);
            Bind(readFromNetworkButton, ReadFromNetwork, add: false);

            if (pressureSlider != null)
            {
                pressureSlider.onValueChanged.RemoveListener(HandlePressureChanged);
            }

            if (severitySlider != null)
            {
                severitySlider.onValueChanged.RemoveListener(HandleSeverityChanged);
            }
        }

        private LeakReport BuildReport()
        {
            return new LeakReport(
                segmentIdInput != null ? segmentIdInput.text : string.Empty,
                operatorInput != null ? operatorInput.text : string.Empty,
                leakToggle != null && leakToggle.isOn,
                pressureDropToggle != null && pressureDropToggle.isOn,
                corrosionToggle != null && corrosionToggle.isOn,
                pressureSlider != null ? pressureSlider.value : DefaultPressureBar,
                severitySlider != null ? Mathf.RoundToInt(severitySlider.value) : DefaultSeverity,
                DateTime.UtcNow);
        }

        private void ConfigureSliders()
        {
            if (pressureSlider != null)
            {
                pressureSlider.minValue = LeakReport.MinPressureBar;
                pressureSlider.maxValue = LeakReport.MaxPressureBar;
                pressureSlider.wholeNumbers = false;
            }

            if (severitySlider != null)
            {
                severitySlider.minValue = LeakReport.MinSeverity;
                severitySlider.maxValue = LeakReport.MaxSeverity;
                severitySlider.wholeNumbers = true;
            }
        }

        private void HandlePressureChanged(float value) => RefreshSliderLabels();

        private void HandleSeverityChanged(float value) => RefreshSliderLabels();

        private void RefreshSliderLabels()
        {
            if (pressureValueText != null && pressureSlider != null)
            {
                pressureValueText.text = $"{pressureSlider.value:F1} бар";
            }

            if (severityValueText != null && severitySlider != null)
            {
                severityValueText.text = $"{Mathf.RoundToInt(severitySlider.value)} / {LeakReport.MaxSeverity}";
            }
        }

        private void SetStatus(string message, bool warning)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = message;
            statusText.color = warning ? new Color(0.95f, 0.55f, 0.35f) : new Color(0.70f, 0.90f, 0.75f);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action, bool add)
        {
            if (button == null)
            {
                return;
            }

            if (add)
            {
                button.onClick.AddListener(action);
            }
            else
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void SetText(InputField field, string value)
        {
            if (field != null)
            {
                field.text = value;
            }
        }

        private static void SetToggle(Toggle toggle, bool value)
        {
            if (toggle != null)
            {
                toggle.isOn = value;
            }
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
            }
        }
    }
}
