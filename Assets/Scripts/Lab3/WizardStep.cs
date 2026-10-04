using System;
using System.Collections.Generic;

namespace Lab3
{
    /// <summary>
    /// Незмінний крок покрокового візарда. Крок без варіантів відповіді — інструктаж,
    /// крок з варіантами — контрольне питання з однією правильною відповіддю.
    /// </summary>
    public sealed class WizardStep
    {
        private static readonly IReadOnlyList<string> NoOptions = Array.Empty<string>();

        private WizardStep(string title, string body, IReadOnlyList<string> options, int correctIndex)
        {
            Title = title;
            Body = body;
            Options = options;
            CorrectIndex = correctIndex;
        }

        public string Title { get; }
        public string Body { get; }
        public IReadOnlyList<string> Options { get; }
        public int CorrectIndex { get; }

        public bool IsQuestion => Options.Count > 0;

        public static WizardStep Instruction(string title, string body)
        {
            return new WizardStep(title, body, NoOptions, -1);
        }

        public static WizardStep Question(string title, string body, int correctIndex, params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                throw new ArgumentException("Питання має містити хоча б один варіант відповіді.", nameof(options));
            }

            if (correctIndex < 0 || correctIndex >= options.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(correctIndex), "Індекс правильної відповіді поза межами списку.");
            }

            return new WizardStep(title, body, Array.AsReadOnly((string[])options.Clone()), correctIndex);
        }
    }
}
