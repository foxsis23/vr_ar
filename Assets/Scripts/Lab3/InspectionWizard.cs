using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Покроковий візард (кінцевий автомат) інструктажу з огляду трубопроводу:
    /// послідовне перемикання екранів, перевірка відповідей і фіксація підсумкового результату.
    /// </summary>
    public class InspectionWizard : MonoBehaviour
    {
        /// <summary>Стани автомата візарда.</summary>
        public enum WizardState
        {
            Idle,
            Step,
            Finished
        }

        [SerializeField] private GameObject stepView;
        [SerializeField] private GameObject resultView;
        [SerializeField] private Text progressText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private Text[] answerLabels;
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Text nextButtonLabel;
        [SerializeField] private Button[] restartButtons;
        [SerializeField] private Text resultText;
        [SerializeField] private Button goToFormButton;
        [SerializeField] private SpatialTablet tablet;

        private readonly List<int> answers = new List<int>();
        private int currentIndex;

        public WizardState State { get; private set; } = WizardState.Idle;

        public int CorrectCount { get; private set; }

        public bool IsPassed => InspectionProgram.QuestionCount > 0 &&
                                (float)CorrectCount / InspectionProgram.QuestionCount >= InspectionProgram.PassRatio;

        public void Restart()
        {
            answers.Clear();
            for (int i = 0; i < InspectionProgram.Steps.Count; i++)
            {
                answers.Add(-1);
            }

            currentIndex = 0;
            CorrectCount = 0;
            State = WizardState.Step;
            RenderStep();
        }

        public void Next()
        {
            if (State != WizardState.Step)
            {
                return;
            }

            WizardStep step = InspectionProgram.Steps[currentIndex];
            if (step.IsQuestion && answers[currentIndex] < 0)
            {
                SetFeedback("Оберіть варіант відповіді, щоб рухатись далі.", warning: true);
                return;
            }

            if (currentIndex >= InspectionProgram.Steps.Count - 1)
            {
                Finish();
                return;
            }

            currentIndex++;
            RenderStep();
        }

        public void Back()
        {
            if (State != WizardState.Step || currentIndex == 0)
            {
                return;
            }

            currentIndex--;
            RenderStep();
        }

        public void Choose(int optionIndex)
        {
            if (State != WizardState.Step)
            {
                return;
            }

            WizardStep step = InspectionProgram.Steps[currentIndex];
            if (!step.IsQuestion || optionIndex < 0 || optionIndex >= step.Options.Count)
            {
                return;
            }

            answers[currentIndex] = optionIndex;
            bool correct = optionIndex == step.CorrectIndex;
            SetFeedback(correct ? "Відповідь правильна." : "Відповідь неправильна — перегляньте інструктаж.", !correct);
            RefreshAnswerHighlight(step);
        }

        private void Awake()
        {
            Restart();
        }

        private void OnEnable()
        {
            BindNavigation(add: true);
        }

        private void OnDisable()
        {
            BindNavigation(add: false);
        }

        private void BindNavigation(bool add)
        {
            Bind(nextButton, Next, add);
            Bind(backButton, Back, add);

            if (restartButtons != null)
            {
                foreach (Button restart in restartButtons)
                {
                    Bind(restart, Restart, add);
                }
            }

            if (goToFormButton != null && tablet != null)
            {
                if (add)
                {
                    goToFormButton.onClick.AddListener(tablet.ShowForm);
                }
                else
                {
                    goToFormButton.onClick.RemoveListener(tablet.ShowForm);
                }
            }

            if (answerButtons == null)
            {
                return;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int optionIndex = i;
                Button button = answerButtons[i];
                if (button == null)
                {
                    continue;
                }

                if (add)
                {
                    button.onClick.AddListener(() => Choose(optionIndex));
                }
                else
                {
                    button.onClick.RemoveAllListeners();
                }
            }
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

        private void Finish()
        {
            CorrectCount = CountCorrect();
            State = WizardState.Finished;
            RenderResult();
        }

        private int CountCorrect()
        {
            int correct = 0;
            for (int i = 0; i < InspectionProgram.Steps.Count; i++)
            {
                WizardStep step = InspectionProgram.Steps[i];
                if (step.IsQuestion && answers[i] == step.CorrectIndex)
                {
                    correct++;
                }
            }

            return correct;
        }

        private void RenderStep()
        {
            SetView(step: true);

            WizardStep step = InspectionProgram.Steps[currentIndex];

            if (progressText != null)
            {
                progressText.text = $"Крок {currentIndex + 1} з {InspectionProgram.Steps.Count}";
            }

            if (titleText != null)
            {
                titleText.text = step.Title;
            }

            if (bodyText != null)
            {
                bodyText.text = step.Body;
            }

            if (nextButtonLabel != null)
            {
                nextButtonLabel.text = currentIndex >= InspectionProgram.Steps.Count - 1 ? "Завершити" : "Далі ▸";
            }

            if (backButton != null)
            {
                backButton.interactable = currentIndex > 0;
            }

            SetFeedback(step.IsQuestion ? "Оберіть один варіант." : string.Empty, warning: false);
            RenderAnswers(step);
        }

        private void RenderAnswers(WizardStep step)
        {
            if (answerButtons == null)
            {
                return;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                bool used = i < step.Options.Count;
                if (answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(used);
                }

                if (used && answerLabels != null && i < answerLabels.Length && answerLabels[i] != null)
                {
                    answerLabels[i].text = step.Options[i];
                }
            }

            RefreshAnswerHighlight(step);
        }

        private void RefreshAnswerHighlight(WizardStep step)
        {
            if (answerButtons == null)
            {
                return;
            }

            int chosen = answers[currentIndex];

            for (int i = 0; i < answerButtons.Length; i++)
            {
                Button button = answerButtons[i];
                if (button == null || i >= step.Options.Count)
                {
                    continue;
                }

                Image background = button.targetGraphic as Image;
                if (background != null)
                {
                    background.color = i == chosen
                        ? new Color(0.20f, 0.45f, 0.75f)
                        : new Color(0.18f, 0.21f, 0.27f);
                }
            }
        }

        private void RenderResult()
        {
            SetView(step: false);

            if (resultText == null)
            {
                return;
            }

            int total = InspectionProgram.QuestionCount;
            string verdict = IsPassed ? "ІНСТРУКТАЖ ЗАРАХОВАНО" : "ІНСТРУКТАЖ НЕ ЗАРАХОВАНО";

            resultText.text =
                $"{verdict}\n\n" +
                $"Правильних відповідей: {CorrectCount} з {total}\n" +
                $"Прохідний бал: {Mathf.CeilToInt(total * InspectionProgram.PassRatio)} з {total}\n\n" +
                (IsPassed
                    ? "Допуск до огляду відкрито. Перейдіть на вкладку «Звіт»."
                    : "Повторіть інструктаж і пройдіть перевірку ще раз.");
        }

        private void SetView(bool step)
        {
            if (stepView != null)
            {
                stepView.SetActive(step);
            }

            if (resultView != null)
            {
                resultView.SetActive(!step);
            }
        }

        private void SetFeedback(string message, bool warning)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
            feedbackText.color = warning ? new Color(0.95f, 0.65f, 0.25f) : new Color(0.75f, 0.85f, 0.95f);
        }
    }
}
