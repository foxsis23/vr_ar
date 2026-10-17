using UnityEngine;

namespace Lab3
{
    /// <summary>
    /// Ділянка трубопроводу з власним тиском. Колір труби візуалізує стан:
    /// синій — тиск впав, зелений — номінал, червоний — перевищення.
    /// </summary>
    public class PipelineSegment : MonoBehaviour, IEquipmentInfo
    {
        public const float NominalBar = 62f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        [SerializeField] private string segmentId = "ГТС-14/1";
        [SerializeField] private string description = "Магістральна ділянка Ø 530 мм.";
        [SerializeField] private Renderer[] pipeRenderers;

        private MaterialPropertyBlock propertyBlock;

        public string SegmentId => segmentId;

        public float PressureBar { get; private set; } = NominalBar;

        public bool HasLeak { get; private set; }

        public string InfoTitle => $"Ділянка {segmentId}";

        public string BuildInfoBody()
        {
            return
                $"{description}\n" +
                $"Тиск: {PressureBar:F1} бар (номінал {NominalBar:F0})\n" +
                $"Стан: {StateLabel()}";
        }

        /// <summary>
        /// Встановлює тиск ділянки та перефарбовує труби через MaterialPropertyBlock,
        /// щоб не мутувати спільний матеріал інших ділянок.
        /// </summary>
        public void SetPressure(float bar, bool hasLeak)
        {
            PressureBar = bar;
            HasLeak = hasLeak;
            Repaint();
        }

        private void Awake()
        {
            Repaint();
        }

        private void Repaint()
        {
            if (pipeRenderers == null)
            {
                return;
            }

            // Блок може знадобитися до Awake, якщо тиск виставили одразу після генерації сцени.
            propertyBlock ??= new MaterialPropertyBlock();

            Color color = PressureColor(PressureBar);

            foreach (Renderer pipeRenderer in pipeRenderers)
            {
                if (pipeRenderer == null)
                {
                    continue;
                }

                pipeRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(LegacyColorId, color);
                pipeRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private static Color PressureColor(float bar)
        {
            Color low = new Color(0.20f, 0.45f, 0.90f);
            Color nominal = new Color(0.25f, 0.75f, 0.35f);
            Color high = new Color(0.90f, 0.25f, 0.20f);

            if (bar < NominalBar)
            {
                return Color.Lerp(low, nominal, Mathf.InverseLerp(0f, NominalBar, bar));
            }

            return Color.Lerp(nominal, high, Mathf.InverseLerp(NominalBar, NominalBar * 1.4f, bar));
        }

        private string StateLabel()
        {
            if (HasLeak)
            {
                return "ВИТІК — потрібен звіт";
            }

            if (PressureBar < NominalBar * 0.5f)
            {
                return "ділянку перекрито";
            }

            if (PressureBar > NominalBar * 1.2f)
            {
                return "перевищення тиску";
            }

            return "норма";
        }
    }
}
