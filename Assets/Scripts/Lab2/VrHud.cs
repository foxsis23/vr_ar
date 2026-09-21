using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lab2
{
    /// <summary>
    /// Інформаційна панель VR-сцени: підказки керування, лічильники та журнал подій взаємодії.
    /// Панель висить на стіні кімнати (World Space Canvas) і читається прямо в шоломі.
    /// </summary>
    public class VrHud : MonoBehaviour
    {
        private const int MaxLogLines = 6;

        [SerializeField] private Text hintsText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text logText;

        private readonly Queue<string> logLines = new Queue<string>();
        private int grabCount;
        private int throwCount;
        private int teleportCount;
        private bool socketFilled;

        public void SetHints(string hints)
        {
            if (hintsText != null)
            {
                hintsText.text = hints;
            }
        }

        public void LogEvent(string message)
        {
            logLines.Enqueue(message);
            while (logLines.Count > MaxLogLines)
            {
                logLines.Dequeue();
            }

            if (logText != null)
            {
                logText.text = string.Join("\n", logLines);
            }
        }

        public void ReportGrab(string objectName)
        {
            grabCount++;
            LogEvent($"Взято: {objectName}");
            RefreshStatus();
        }

        public void ReportRelease(string objectName, float speed)
        {
            bool thrown = speed >= 1.5f;
            if (thrown)
            {
                throwCount++;
            }

            LogEvent(thrown
                ? $"Кинуто: {objectName} ({speed:F1} м/с)"
                : $"Відпущено: {objectName}");
            RefreshStatus();
        }

        public void ReportSocket(string objectName, bool filled)
        {
            socketFilled = filled;
            LogEvent(filled ? $"Зафіксовано в зоні: {objectName}" : $"Знято із зони: {objectName}");
            RefreshStatus();
        }

        public void ReportTeleport(string destination)
        {
            teleportCount++;
            LogEvent($"Телепорт: {destination}");
            RefreshStatus();
        }

        private void Start()
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            string socketState = socketFilled ? "зайнята" : "вільна";
            statusText.text =
                $"Взято об'єктів: {grabCount}    Кидків: {throwCount}\n" +
                $"Телепортів: {teleportCount}    Зона фіксації: {socketState}";
        }
    }
}
