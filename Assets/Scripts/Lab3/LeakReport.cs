using System;
using System.Text;

namespace Lab3
{
    /// <summary>
    /// Незмінний звіт про витік на ділянці трубопроводу.
    /// Заповнюється на просторовій формі планшета; передача на сервер — наступна лабораторна.
    /// </summary>
    public sealed class LeakReport
    {
        public const float MinPressureBar = 0f;
        public const float MaxPressureBar = 120f;
        public const int MinSeverity = 1;
        public const int MaxSeverity = 5;

        public LeakReport(
            string segmentId,
            string operatorName,
            bool leakConfirmed,
            bool pressureDrop,
            bool corrosion,
            float pressureBar,
            int severity,
            DateTime createdAtUtc)
        {
            SegmentId = segmentId?.Trim() ?? string.Empty;
            OperatorName = operatorName?.Trim() ?? string.Empty;
            LeakConfirmed = leakConfirmed;
            PressureDrop = pressureDrop;
            Corrosion = corrosion;
            PressureBar = pressureBar;
            Severity = severity;
            CreatedAtUtc = createdAtUtc;
        }

        public string SegmentId { get; }
        public string OperatorName { get; }
        public bool LeakConfirmed { get; }
        public bool PressureDrop { get; }
        public bool Corrosion { get; }
        public float PressureBar { get; }
        public int Severity { get; }
        public DateTime CreatedAtUtc { get; }

        /// <summary>
        /// Перевірка даних на межі системи: порожні або неправдоподібні значення на сервер не йдуть.
        /// Повертає null, якщо звіт валідний, інакше — текст помилки для користувача.
        /// </summary>
        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(SegmentId))
            {
                return "Вкажіть ID ділянки (наприклад, ГТС-14).";
            }

            if (SegmentId.Length > 32)
            {
                return "ID ділянки задовгий — максимум 32 символи.";
            }

            if (string.IsNullOrWhiteSpace(OperatorName))
            {
                return "Вкажіть прізвище оператора.";
            }

            if (PressureBar < MinPressureBar || PressureBar > MaxPressureBar)
            {
                return $"Тиск поза діапазоном {MinPressureBar:F0}–{MaxPressureBar:F0} бар.";
            }

            if (Severity < MinSeverity || Severity > MaxSeverity)
            {
                return $"Критичність поза діапазоном {MinSeverity}–{MaxSeverity}.";
            }

            if (!LeakConfirmed && !PressureDrop && !Corrosion)
            {
                return "Відмітьте хоча б одну ознаку дефекту.";
            }

            return null;
        }

        public string ToSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Ділянка: {SegmentId}    Оператор: {OperatorName}");
            builder.AppendLine($"Тиск: {PressureBar:F1} бар    Критичність: {Severity}/{MaxSeverity}");
            builder.Append("Ознаки: ").Append(BuildSigns());
            return builder.ToString();
        }

        private string BuildSigns()
        {
            StringBuilder signs = new StringBuilder();
            AppendSign(signs, LeakConfirmed, "витік");
            AppendSign(signs, PressureDrop, "падіння тиску");
            AppendSign(signs, Corrosion, "корозія");
            return signs.Length == 0 ? "немає" : signs.ToString();
        }

        private static void AppendSign(StringBuilder target, bool enabled, string label)
        {
            if (!enabled)
            {
                return;
            }

            if (target.Length > 0)
            {
                target.Append(", ");
            }

            target.Append(label);
        }
    }
}
