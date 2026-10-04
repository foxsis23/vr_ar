using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Модель тиску трубопроводу: ділянки впорядковані за течією, між сусідніми стоїть вентиль.
    /// Перекритий вентиль відсікає все, що нижче за течією, і піднімає тиск перед собою.
    /// </summary>
    public class PipelineNetwork : MonoBehaviour
    {
        private const float ResidualBar = 4f;
        private const float BackPressureFactor = 1.35f;
        private const float LeakDropBar = 24f;

        [SerializeField] private float sourcePressureBar = PipelineSegment.NominalBar;
        [SerializeField] private PipelineSegment[] segments;
        [SerializeField] private ValveControl[] valves;
        [SerializeField] private int leakSegmentIndex = 1;
        [SerializeField] private GameObject leakIndicator;
        [SerializeField] private Text readoutText;

        public bool LeakActive { get; private set; }

        public PipelineSegment LeakSegment =>
            segments != null && leakSegmentIndex >= 0 && leakSegmentIndex < segments.Length
                ? segments[leakSegmentIndex]
                : null;

        public void SetLeak(bool active)
        {
            LeakActive = active;
            Recalculate();
        }

        public void ToggleLeak()
        {
            SetLeak(!LeakActive);
        }

        /// <summary>
        /// Перераховує тиск усіх ділянок за станом вентилів і наявністю витоку.
        /// </summary>
        public void Recalculate()
        {
            if (segments == null)
            {
                return;
            }

            bool blocked = false;

            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0 && !IsValveOpen(i - 1))
                {
                    blocked = true;
                }

                PipelineSegment segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                bool leakHere = LeakActive && i >= leakSegmentIndex;
                float pressure = blocked ? ResidualBar : sourcePressureBar;

                if (!blocked && leakHere)
                {
                    pressure = Mathf.Max(ResidualBar, pressure - LeakDropBar);
                }

                // Перед перекритим вентилем середовище підпирає — тиск зростає.
                if (!blocked && i < segments.Length - 1 && !IsValveOpen(i))
                {
                    pressure *= BackPressureFactor;
                }

                segment.SetPressure(pressure, LeakActive && i == leakSegmentIndex);
            }

            if (leakIndicator != null)
            {
                leakIndicator.SetActive(LeakActive);
            }

            RefreshReadout();
        }

        private void Start()
        {
            Recalculate();
        }

        private bool IsValveOpen(int valveIndex)
        {
            if (valves == null || valveIndex < 0 || valveIndex >= valves.Length)
            {
                return true;
            }

            ValveControl valve = valves[valveIndex];
            return valve == null || valve.IsOpen;
        }

        private void RefreshReadout()
        {
            if (readoutText == null)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(LeakActive ? "СТАН: АВАРІЯ — виявлено витік" : "СТАН: штатний режим");

            foreach (PipelineSegment segment in segments)
            {
                if (segment != null)
                {
                    builder.AppendLine($"{segment.SegmentId}: {segment.PressureBar:F1} бар");
                }
            }

            if (valves != null)
            {
                foreach (ValveControl valve in valves)
                {
                    if (valve != null)
                    {
                        builder.AppendLine($"{valve.ValveId}: {(valve.IsOpen ? "відкрито" : "перекрито")}");
                    }
                }
            }

            readoutText.text = builder.ToString().TrimEnd();
        }
    }
}
