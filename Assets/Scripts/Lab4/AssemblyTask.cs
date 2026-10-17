using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Lab4
{
    /// <summary>
    /// Сценарій збирання вузла заміру тиску: стежить за всіма монтажними пазами,
    /// веде список прогресу й повідомляє, коли вузол зібрано повністю.
    /// </summary>
    public class AssemblyTask : MonoBehaviour
    {
        [SerializeField] private AssemblySocket[] sockets = new AssemblySocket[0];
        [SerializeField] private Text progressText;
        [SerializeField] private UnityEvent completed = new UnityEvent();

        /// <summary>Кількість зафіксованих деталей.</summary>
        public int InstalledCount { get; private set; }

        /// <summary>Усього монтажних пазів у вузлі.</summary>
        public int TotalCount => sockets.Length;

        /// <summary>Усі пази заповнені правильними деталями.</summary>
        public bool IsComplete => TotalCount > 0 && InstalledCount == TotalCount;

        /// <summary>Подія завершення складання — спрацьовує один раз на кожне повне збирання.</summary>
        public UnityEvent Completed => completed;

        private bool wasComplete;

        private void OnEnable()
        {
            foreach (AssemblySocket socket in sockets)
            {
                if (socket != null)
                {
                    socket.StateChanged.AddListener(Refresh);
                }
            }

            Refresh();
        }

        private void OnDisable()
        {
            foreach (AssemblySocket socket in sockets)
            {
                if (socket != null)
                {
                    socket.StateChanged.RemoveListener(Refresh);
                }
            }
        }

        /// <summary>Перераховує прогрес і оновлює список деталей на табло.</summary>
        public void Refresh()
        {
            StringBuilder builder = new StringBuilder();
            int installed = 0;

            foreach (AssemblySocket socket in sockets)
            {
                if (socket == null)
                {
                    continue;
                }

                if (socket.IsFilled)
                {
                    installed++;
                    builder.AppendLine($"[+] {socket.SlotName}: {socket.InstalledPart.DisplayName}");
                }
                else
                {
                    builder.AppendLine($"[ ] {socket.SlotName}: деталь не встановлено");
                }
            }

            InstalledCount = installed;

            if (progressText != null)
            {
                progressText.text = $"Складання вузла: {InstalledCount}/{TotalCount}\n{builder}";
            }

            // Подія потрібна один раз на перехід у повний склад, інакше вона
            // повторювалася б щоразу, коли деталь переставляють у вже зібраному вузлі.
            if (IsComplete && !wasComplete)
            {
                completed.Invoke();
            }

            wasComplete = IsComplete;
        }
    }
}
