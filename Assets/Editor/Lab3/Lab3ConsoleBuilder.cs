using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Стаціонарна консоль ділянки: World-Space Canvas на пульті заввишки з руку,
    /// розрахований на прямий дотик пальцем (Direct Poke) і на дистанційний промінь.
    /// </summary>
    public static class Lab3ConsoleBuilder
    {
        private static readonly Vector2 ConsoleSize = new Vector2(820f, 620f);
        private const float ConsoleScale = 0.00085f;

        /// <summary>Елементи консолі, які сцена дозв'язує після створення планшета.</summary>
        public sealed class ConsoleRig
        {
            public ConsoleRig(PipelineConsole console, Text readoutText)
            {
                Console = console;
                ReadoutText = readoutText;
            }

            public PipelineConsole Console { get; }
            public Text ReadoutText { get; }
        }

        public static ConsoleRig Build(PipelineNetwork network, IReadOnlyList<ValveControl> valves)
        {
            Material caseMaterial = Lab3Environment.CreateMaterial("Lab3_Console", new Color(0.22f, 0.24f, 0.28f));

            GameObject root = new GameObject("Pipeline Console");
            root.transform.position = new Vector3(0f, 0f, 0.9f);

            Lab3Environment.CreateBox("Pedestal", new Vector3(0f, 0.33f, 0f), new Vector3(0.9f, 0.66f, 0.35f), caseMaterial, root.transform);

            // Нахилена голова пульта: корпус стоїть позаду екрана, тому не перекриває верх панелі.
            GameObject head = new GameObject("Console Head");
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            head.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);

            Lab3Environment.CreateBox("Case", new Vector3(0f, 0f, 0.09f), new Vector3(0.95f, 0.66f, 0.14f), caseMaterial, head.transform);

            // Канвас читається з боку, протилежного його forward, тому forward дивиться від користувача.
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Console Panel", head.transform, ConsoleSize, ConsoleScale);
            canvas.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            canvas.transform.localRotation = Quaternion.identity;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 12f, new RectOffset(24, 24, 20, 20));

            Lab3UiFactory.CreateText("Title", column, "Консоль ділянки ГТС-14", 28, FontStyle.Bold, TextAnchor.UpperCenter);
            Text readout = Lab3UiFactory.CreateText("Readout", column, "—", 22, FontStyle.Normal, TextAnchor.UpperLeft);

            RectTransform valveRow = Lab3UiFactory.CreateRow("Valves", column, 12f, 96f);
            foreach (ValveControl valve in valves)
            {
                Button button = Lab3UiFactory.CreateButton($"Valve Button {valve.ValveId}", valveRow, valve.ValveId, 22, 96f, out Text label);
                ValveButton valveButton = button.gameObject.AddComponent<ValveButton>();
                Lab3Serialized.For(valveButton)
                    .Reference("valve", valve)
                    .Reference("label", label)
                    .Apply();
            }

            Button leakButton = Lab3UiFactory.CreateButton("Leak Button", column, "Імітувати витік", 22, 84f, out Text leakLabel);

            RectTransform tabletRow = Lab3UiFactory.CreateRow("Tablet Shortcuts", column, 12f, 76f);
            Button wizardButton = Lab3UiFactory.CreateButton("Open Wizard", tabletRow, "Відкрити інструктаж", 20, 76f, out _);
            Button formButton = Lab3UiFactory.CreateButton("Open Form", tabletRow, "Відкрити звіт", 20, 76f, out _);

            Lab3UiFactory.CreateText("Hint", column,
                "Кнопки натискаються пальцем (Direct Poke) або променем контролера.", 19, FontStyle.Italic, TextAnchor.UpperCenter);

            PipelineConsole console = root.AddComponent<PipelineConsole>();
            Lab3Serialized.For(console)
                .Reference("network", network)
                .Reference("leakButton", leakButton)
                .Reference("leakButtonLabel", leakLabel)
                .Reference("openWizardButton", wizardButton)
                .Reference("openFormButton", formButton)
                .Apply();

            return new ConsoleRig(console, readout);
        }
    }
}
