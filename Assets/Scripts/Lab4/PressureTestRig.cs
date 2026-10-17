using UnityEngine;
using UnityEngine.UI;

namespace Lab4
{
    /// <summary>
    /// Логіка стенда опресовки: тумблер живлення, слайдер тиску та дві об'ємні кнопки
    /// («Пуск» і «Скид») керують перевіркою зібраного вузла заміру тиску.
    /// Опресовка проходить, лише коли вузол зібрано повністю, стенд під живленням,
    /// а тиск лежить у діапазоні <see cref="MinTestBar"/>–<see cref="MaxTestBar"/>.
    /// </summary>
    public class PressureTestRig : MonoBehaviour
    {
        public const float MinTestBar = 6f;
        public const float MaxTestBar = 8f;

        private static readonly Color IdleColor = new Color(0.35f, 0.38f, 0.42f);
        private static readonly Color PassColor = new Color(0.25f, 0.80f, 0.35f);
        private static readonly Color FailColor = new Color(0.90f, 0.25f, 0.20f);

        [SerializeField] private AssemblyTask task;
        [SerializeField] private ToggleSwitch3D powerSwitch;
        [SerializeField] private PokeSlider pressureSlider;
        [SerializeField] private PokePushButton startButton;
        [SerializeField] private PokePushButton resetButton;
        [SerializeField] private Text resultText;
        [SerializeField] private Renderer statusLamp;
        [SerializeField] private AssemblyPart[] parts = new AssemblyPart[0];

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            Report("Стенд готовий. Зберіть вузол і виконайте опресовку.", IdleColor);
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.Pressed.AddListener(RunTest);
            }

            if (resetButton != null)
            {
                resetButton.Pressed.AddListener(ResetStand);
            }

            if (powerSwitch != null)
            {
                powerSwitch.Toggled.AddListener(HandlePowerToggled);
            }

            if (task != null)
            {
                task.Completed.AddListener(HandleAssemblyCompleted);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.Pressed.RemoveListener(RunTest);
            }

            if (resetButton != null)
            {
                resetButton.Pressed.RemoveListener(ResetStand);
            }

            if (powerSwitch != null)
            {
                powerSwitch.Toggled.RemoveListener(HandlePowerToggled);
            }

            if (task != null)
            {
                task.Completed.RemoveListener(HandleAssemblyCompleted);
            }
        }

        /// <summary>Перевірка герметичності зібраного вузла — кнопка «Пуск».</summary>
        public void RunTest()
        {
            if (powerSwitch != null && !powerSwitch.IsOn)
            {
                Report("Стенд знеструмлено — увімкніть тумблер живлення.", FailColor);
                return;
            }

            if (task != null && !task.IsComplete)
            {
                Report($"Вузол не зібрано: {task.InstalledCount}/{task.TotalCount} деталей.", FailColor);
                return;
            }

            float bar = pressureSlider != null ? pressureSlider.Value : 0f;
            if (bar < MinTestBar || bar > MaxTestBar)
            {
                Report($"Тиск {bar:0.0} бар поза діапазоном {MinTestBar:0}–{MaxTestBar:0} бар.", FailColor);
                return;
            }

            Report($"Опресовка {bar:0.0} бар пройдена. Вузол заміру тиску герметичний.", PassColor);
        }

        /// <summary>Скидання стенда — кнопка «Скид»: деталі повертаються на лоток.</summary>
        public void ResetStand()
        {
            foreach (AssemblyPart part in parts)
            {
                if (part != null)
                {
                    part.ReturnToTray();
                }
            }

            if (pressureSlider != null)
            {
                pressureSlider.SetValue(MinTestBar - 2f);
            }

            if (task != null)
            {
                task.Refresh();
            }

            Report("Стенд скинуто. Деталі повернуто на лоток.", IdleColor);
        }

        /// <summary>Повідомлення для <see cref="AssemblyTask.Completed"/> — вузол щойно зібрано.</summary>
        public void HandleAssemblyCompleted()
        {
            Report($"Вузол зібрано. Виставте тиск {MinTestBar:0}–{MaxTestBar:0} бар і натисніть «Пуск».", PassColor);
        }

        private void HandlePowerToggled(bool isOn)
        {
            Report(isOn ? "Стенд під живленням." : "Стенд знеструмлено.", IdleColor);
        }

        private void Report(string message, Color color)
        {
            if (resultText != null)
            {
                resultText.text = message;
                resultText.color = color;
            }

            if (statusLamp == null)
            {
                return;
            }

            statusLamp.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            propertyBlock.SetColor("_EmissionColor", color * 1.5f);
            statusLamp.SetPropertyBlock(propertyBlock);
        }
    }
}
