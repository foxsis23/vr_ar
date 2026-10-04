using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Технологічна нитка газопроводу: три ділянки, два регулювальні вентилі,
    /// точка витоку та спливаючі інформаційні картки обладнання.
    /// </summary>
    public static class Lab3PipelineBuilder
    {
        public const float PipeAxisZ = 2.6f;
        public const float PipeAxisY = 1.3f;
        private const float PipeRadius = 0.15f;
        private const float SegmentLength = 2.4f;

        /// <summary>Зібрані об'єкти нитки — далі їх зв'язує <see cref="Lab3SceneBuilder"/>.</summary>
        public sealed class PipelineRig
        {
            public PipelineRig(
                GameObject root,
                IReadOnlyList<PipelineSegment> segments,
                IReadOnlyList<ValveControl> valves,
                GameObject leakIndicator)
            {
                Root = root;
                Segments = segments;
                Valves = valves;
                LeakIndicator = leakIndicator;
            }

            public GameObject Root { get; }
            public IReadOnlyList<PipelineSegment> Segments { get; }
            public IReadOnlyList<ValveControl> Valves { get; }
            public GameObject LeakIndicator { get; }
        }

        public static PipelineRig Build()
        {
            Material steel = Lab3Environment.CreateMaterial("Lab3_Steel", new Color(0.55f, 0.57f, 0.60f));
            Material valveBody = Lab3Environment.CreateMaterial("Lab3_ValveBody", new Color(0.30f, 0.32f, 0.36f));
            Material wheel = Lab3Environment.CreateMaterial("Lab3_Handwheel", new Color(0.85f, 0.45f, 0.15f));
            Material leak = Lab3Environment.CreateMaterial("Lab3_Leak", new Color(0.95f, 0.25f, 0.20f), emissive: true);

            GameObject root = new GameObject("Pipeline");

            var segments = new List<PipelineSegment>
            {
                BuildSegment(root.transform, 1, "ГТС-14/1", -3.0f, "Вхідна ділянка, Ø 530 мм, зварні шви №1–№4."),
                BuildSegment(root.transform, 2, "ГТС-14/2", 0.0f, "Аварійно-небезпечна ділянка з переходом Ø 530/426 мм."),
                BuildSegment(root.transform, 3, "ГТС-14/3", 3.0f, "Вихідна ділянка на газорозподільний пункт."),
            };

            var valves = new List<ValveControl>
            {
                BuildValve(root.transform, "V-1", -1.5f, valveBody, wheel, steel),
                BuildValve(root.transform, "V-2", 1.5f, valveBody, wheel, steel),
            };

            BuildSupports(root.transform, steel);
            GameObject leakIndicator = BuildLeakIndicator(root.transform, leak);

            return new PipelineRig(root, segments, valves, leakIndicator);
        }

        private static PipelineSegment BuildSegment(Transform parent, int index, string segmentId, float centerX, string description)
        {
            // Окремий матеріал на ділянку: у грі колір перебиває MaterialPropertyBlock,
            // а на статичних рендерах для звіту одразу видно номінальний тиск.
            Material steel = Lab3Environment.CreateMaterial($"Lab3_Pipe_{index}", new Color(0.25f, 0.75f, 0.35f));

            GameObject segment = new GameObject($"Segment {segmentId}");
            segment.transform.SetParent(parent, false);
            segment.transform.position = new Vector3(centerX, PipeAxisY, PipeAxisZ);

            // Циліндр Unity має вісь Y і висоту 2, тому поворот на 90° по Z кладе трубу вздовж X.
            GameObject pipe = Lab3Environment.CreateCylinder("Pipe", Vector3.zero, new Vector3(0f, 0f, 90f),
                new Vector3(PipeRadius * 2f, SegmentLength * 0.5f, PipeRadius * 2f), steel, segment.transform);

            float halfLength = SegmentLength * 0.5f;
            GameObject flangeA = Lab3Environment.CreateCylinder("Flange A", new Vector3(-halfLength, 0f, 0f), new Vector3(0f, 0f, 90f),
                new Vector3(PipeRadius * 2.5f, 0.03f, PipeRadius * 2.5f), steel, segment.transform);
            GameObject flangeB = Lab3Environment.CreateCylinder("Flange B", new Vector3(halfLength, 0f, 0f), new Vector3(0f, 0f, 90f),
                new Vector3(PipeRadius * 2.5f, 0.03f, PipeRadius * 2.5f), steel, segment.transform);

            segment.AddComponent<XRSimpleInteractable>();
            PipelineSegment component = segment.AddComponent<PipelineSegment>();

            Lab3Serialized.For(component)
                .Text("segmentId", segmentId)
                .Text("description", description)
                .References("pipeRenderers",
                    pipe.GetComponent<Renderer>(),
                    flangeA.GetComponent<Renderer>(),
                    flangeB.GetComponent<Renderer>())
                .Apply();

            AttachInfoCard(segment, new Vector3(0f, 0.75f, 0f));
            return component;
        }

        private static ValveControl BuildValve(Transform parent, string valveId, float x, Material bodyMaterial, Material wheelMaterial, Material steel)
        {
            GameObject valve = new GameObject($"Valve {valveId}");
            valve.transform.SetParent(parent, false);
            valve.transform.position = new Vector3(x, PipeAxisY, PipeAxisZ);

            Lab3Environment.CreateBox("Body", Vector3.zero, new Vector3(0.42f, 0.42f, 0.42f), bodyMaterial, valve.transform);
            Lab3Environment.CreateCylinder("Stem", new Vector3(0f, 0.3f, 0f), Vector3.zero,
                new Vector3(0.07f, 0.18f, 0.07f), steel, valve.transform);

            // Порожній півот обертається навколо Y; візуальні деталі лежать усередині зі своїм поворотом.
            GameObject handwheel = new GameObject("Handwheel");
            handwheel.transform.SetParent(valve.transform, false);
            handwheel.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            Lab3Environment.CreateCylinder("Rim", Vector3.zero, Vector3.zero,
                new Vector3(0.46f, 0.02f, 0.46f), wheelMaterial, handwheel.transform);
            Lab3Environment.CreateBox("Spoke A", Vector3.zero, new Vector3(0.44f, 0.03f, 0.05f), wheelMaterial, handwheel.transform);
            Lab3Environment.CreateBox("Spoke B", Vector3.zero, new Vector3(0.05f, 0.03f, 0.44f), wheelMaterial, handwheel.transform);
            // Мітка на ободі — за нею видно, що штурвал справді провертається.
            Lab3Environment.CreateBox("Marker", new Vector3(0.21f, 0.02f, 0f), new Vector3(0.07f, 0.05f, 0.07f), steel, handwheel.transform);

            // Інтерактабл додаємо першим: ValveControl вимагає його через RequireComponent.
            valve.AddComponent<XRSimpleInteractable>();
            ValveControl control = valve.AddComponent<ValveControl>();

            Lab3Serialized.For(control)
                .Text("valveId", valveId)
                .Reference("handwheel", handwheel.transform)
                .Flag("isOpen", true)
                .Apply();

            AttachInfoCard(valve, new Vector3(0f, 0.95f, 0f));
            return control;
        }

        private static void BuildSupports(Transform parent, Material steel)
        {
            foreach (float x in new[] { -4.0f, -1.5f, 1.5f, 4.0f })
            {
                GameObject support = new GameObject("Pipe Support");
                support.transform.SetParent(parent, false);
                support.transform.position = new Vector3(x, 0f, PipeAxisZ);

                Lab3Environment.CreateBox("Post", new Vector3(0f, PipeAxisY * 0.5f, 0f),
                    new Vector3(0.12f, PipeAxisY, 0.12f), steel, support.transform);
                Lab3Environment.CreateBox("Saddle", new Vector3(0f, PipeAxisY - 0.1f, 0f),
                    new Vector3(0.4f, 0.06f, 0.4f), steel, support.transform);
            }
        }

        private static GameObject BuildLeakIndicator(Transform parent, Material leakMaterial)
        {
            GameObject indicator = new GameObject("Leak Point");
            indicator.transform.SetParent(parent, false);
            indicator.transform.position = new Vector3(0.6f, PipeAxisY + 0.2f, PipeAxisZ);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Leak Glow";
            sphere.transform.SetParent(indicator.transform, false);
            sphere.transform.localScale = Vector3.one * 0.22f;
            sphere.GetComponent<Renderer>().sharedMaterial = leakMaterial;
            Object.DestroyImmediate(sphere.GetComponent<Collider>());

            Light light = indicator.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.35f, 0.25f);
            light.range = 3f;
            light.intensity = 3f;

            indicator.SetActive(false);
            return indicator;
        }

        /// <summary>
        /// Спливаюче інформаційне вікно над обладнанням: при наведенні променем
        /// або дотиком картка вмикається і наповнюється актуальним станом.
        /// </summary>
        private static void AttachInfoCard(GameObject owner, Vector3 localOffset)
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Info Card", owner.transform, new Vector2(620f, 300f), 0.0009f);
            canvas.transform.localPosition = localOffset;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 12f, new RectOffset(24, 24, 20, 20));
            Text title = Lab3UiFactory.CreateText("Title", column, "—", 32, FontStyle.Bold, TextAnchor.UpperLeft);
            Text body = Lab3UiFactory.CreateText("Body", column, "—", 24, FontStyle.Normal, TextAnchor.UpperLeft);

            EquipmentInfoCard card = owner.AddComponent<EquipmentInfoCard>();
            Lab3Serialized.For(card)
                .Reference("card", canvas.gameObject)
                .Reference("titleText", title)
                .Reference("bodyText", body)
                .Apply();

            canvas.gameObject.SetActive(false);
        }
    }
}
