using System.Collections.Generic;
using Lab3.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Монтажний стенд: корпус вузла заміру тиску з чотирма пазами,
    /// лоток змінних деталей і сценарій складання <see cref="AssemblyTask"/>.
    /// </summary>
    public static class Lab4StandBuilder
    {
        public const float BenchHeight = 0.95f;
        public const float BenchZ = 0.8f;

        private const float NodeAxisY = BenchHeight + 0.22f;

        /// <summary>Опис однієї змінної деталі вузла.</summary>
        private sealed class PartSpec
        {
            public PartSpec(string partId, string displayName, string slotName, string specification, float slotX, Color color)
            {
                PartId = partId;
                DisplayName = displayName;
                SlotName = slotName;
                Specification = specification;
                SlotX = slotX;
                Color = color;
            }

            public string PartId { get; }
            public string DisplayName { get; }
            public string SlotName { get; }
            public string Specification { get; }
            public float SlotX { get; }
            public Color Color { get; }
        }

        /// <summary>Зібрані об'єкти стенда — далі їх зв'язує <see cref="Lab4SceneBuilder"/>.</summary>
        public sealed class StandRig
        {
            public StandRig(GameObject root, IReadOnlyList<AssemblySocket> sockets, IReadOnlyList<AssemblyPart> parts)
            {
                Root = root;
                Sockets = sockets;
                Parts = parts;
            }

            public GameObject Root { get; }
            public IReadOnlyList<AssemblySocket> Sockets { get; }
            public IReadOnlyList<AssemblyPart> Parts { get; }
        }

        private static readonly PartSpec[] Specs =
        {
            new PartSpec("gasket", "Прокладка ПОН-Б Ø 57", "Фланцеве з'єднання",
                "Пароніт ПОН-Б, Ø 57 мм, ущільнення фланця імпульсної лінії.",
                -0.45f, new Color(0.15f, 0.17f, 0.20f)),
            new PartSpec("manometer", "Манометр МТИ-1218", "Бобишка манометра",
                "Клас точності 0,6, шкала 0–10 бар, різьба М20×1,5.",
                -0.15f, new Color(0.88f, 0.89f, 0.91f)),
            new PartSpec("filter", "Фільтр-патрон ФП-25", "Корпус фільтра",
                "Тонкість фільтрації 25 мкм, корпус із нержавіючої сталі.",
                0.15f, new Color(0.30f, 0.55f, 0.75f)),
            new PartSpec("valve", "Кран триходовий КТ-15", "Імпульсна лінія",
                "Ду 15, продувка та перевірка манометра без зупинки лінії.",
                0.45f, new Color(0.85f, 0.45f, 0.15f)),
        };

        public static StandRig Build()
        {
            GameObject root = new GameObject("Assembly Stand");

            BuildBench(root.transform);
            GameObject node = BuildNodeBody(root.transform);

            var sockets = new List<AssemblySocket>();
            var parts = new List<AssemblyPart>();

            float trayX = -1.75f;

            for (int index = 0; index < Specs.Length; index++)
            {
                PartSpec spec = Specs[index];
                sockets.Add(BuildSocket(node.transform, spec, index));
                parts.Add(BuildPart(root.transform, spec, trayX + index * 0.32f));
            }

            BuildTray(root.transform, trayX);

            return new StandRig(root, sockets, parts);
        }

        /// <summary>Верстак стенда: стільниця з опорами.</summary>
        private static void BuildBench(Transform parent)
        {
            Material bench = Lab4Materials.Bench;
            Material steel = Lab4Materials.Steel;

            GameObject table = new GameObject("Bench");
            table.transform.SetParent(parent, false);
            table.transform.position = new Vector3(0f, 0f, BenchZ);

            Lab3Environment.CreateBox("Top", new Vector3(0f, BenchHeight, 0f),
                new Vector3(2.0f, 0.08f, 0.9f), bench, table.transform);

            foreach (float x in new[] { -0.9f, 0.9f })
            {
                foreach (float z in new[] { -0.35f, 0.35f })
                {
                    Lab3Environment.CreateBox("Leg", new Vector3(x, BenchHeight * 0.5f, z),
                        new Vector3(0.08f, BenchHeight, 0.08f), steel, table.transform);
                }
            }
        }

        /// <summary>
        /// Корпус вузла заміру тиску — котушка труби на сідлах. На ній розміщені монтажні пази.
        /// </summary>
        private static GameObject BuildNodeBody(Transform parent)
        {
            Material steel = Lab4Materials.Steel;

            GameObject node = new GameObject("Pressure Node");
            node.transform.SetParent(parent, false);
            node.transform.position = new Vector3(0f, NodeAxisY, BenchZ);

            // Циліндр Unity має вісь Y і висоту 2, тому поворот на 90° по Z кладе котушку вздовж X.
            Lab3Environment.CreateCylinder("Spool", Vector3.zero, new Vector3(0f, 0f, 90f),
                new Vector3(0.26f, 0.65f, 0.26f), steel, node.transform);

            foreach (float x in new[] { -0.65f, 0.65f })
            {
                Lab3Environment.CreateCylinder("Flange", new Vector3(x, 0f, 0f), new Vector3(0f, 0f, 90f),
                    new Vector3(0.36f, 0.025f, 0.36f), steel, node.transform);

                Lab3Environment.CreateBox("Cradle", new Vector3(x * 0.75f, -0.22f, 0f),
                    new Vector3(0.12f, 0.16f, 0.3f), steel, node.transform);
            }

            return node;
        }

        /// <summary>
        /// Монтажний паз: <see cref="XRSocketInteractor"/> з кільцем-індикатором і підписом.
        /// Деталь фіксується в позиції attachTransform, щойно її відпускають поруч.
        /// </summary>
        private static AssemblySocket BuildSocket(Transform node, PartSpec spec, int index)
        {
            GameObject socketObject = new GameObject($"Socket {spec.SlotName}");
            socketObject.transform.SetParent(node, false);
            socketObject.transform.localPosition = new Vector3(spec.SlotX, 0.14f, 0f);

            SphereCollider trigger = socketObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.12f;

            GameObject ring = Lab3Environment.CreateCylinder("Ring", Vector3.zero, Vector3.zero,
                new Vector3(0.18f, 0.008f, 0.18f), Lab4Materials.Indicator, socketObject.transform);
            Object.DestroyImmediate(ring.GetComponent<Collider>());

            GameObject attach = new GameObject("Attach");
            attach.transform.SetParent(socketObject.transform, false);
            attach.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            XRSocketInteractor socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.attachTransform = attach.transform;
            socket.showInteractableHoverMeshes = false;
            socket.hoverSocketSnapping = true;

            // Сусідні пази стоять за 0,3 м, тому підписи чергуються по висоті й не перекриваються.
            Text label = BuildSocketLabel(socketObject.transform, spec.SlotName, 0.3f + (index % 2) * 0.13f);

            AssemblySocket component = socketObject.AddComponent<AssemblySocket>();
            Lab3Serialized.For(component)
                .Text("expectedPartId", spec.PartId)
                .Text("slotName", spec.SlotName)
                .Reference("indicator", ring.GetComponent<Renderer>())
                .Reference("label", label)
                .Apply();

            return component;
        }

        private static Text BuildSocketLabel(Transform parent, string slotName, float height)
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Slot Label", parent, new Vector2(440f, 150f), 0.0005f);
            canvas.transform.localPosition = new Vector3(0f, height, -0.08f);
            // Канвас у World Space читається з боку, протилежного його forward: нахил по X
            // розвертає підпис до оператора, а зайвий поворот на 180° дав би дзеркальний текст.
            canvas.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 6f, new RectOffset(16, 16, 12, 12));
            return Lab3UiFactory.CreateText("Text", column, $"{slotName}\nочікується деталь", 24, FontStyle.Bold, TextAnchor.UpperCenter);
        }

        /// <summary>Лоток із деталями: звідси їх беруть жестом щипка.</summary>
        private static void BuildTray(Transform parent, float trayX)
        {
            Material steel = Lab4Materials.Steel;

            GameObject tray = new GameObject("Parts Tray");
            tray.transform.SetParent(parent, false);
            tray.transform.position = new Vector3(trayX + 0.48f, 0f, BenchZ - 0.05f);

            Lab3Environment.CreateBox("Top", new Vector3(0f, BenchHeight, 0f),
                new Vector3(1.4f, 0.06f, 0.6f), Lab4Materials.Bench, tray.transform);

            foreach (float x in new[] { -0.6f, 0.6f })
            {
                Lab3Environment.CreateBox("Leg", new Vector3(x, BenchHeight * 0.5f, 0f),
                    new Vector3(0.07f, BenchHeight, 0.07f), steel, tray.transform);
            }

            Lab3Environment.CreateBox("Rim North", new Vector3(0f, BenchHeight + 0.06f, 0.28f),
                new Vector3(1.4f, 0.06f, 0.04f), steel, tray.transform);
            Lab3Environment.CreateBox("Rim South", new Vector3(0f, BenchHeight + 0.06f, -0.28f),
                new Vector3(1.4f, 0.06f, 0.04f), steel, tray.transform);
        }

        /// <summary>
        /// Змінна деталь: примітив з колайдером, Rigidbody та <see cref="XRGrabInteractable"/>.
        /// Захоплення виконується жестом щипка руки, тому кнопки контролера не потрібні.
        /// </summary>
        private static AssemblyPart BuildPart(Transform parent, PartSpec spec, float x)
        {
            Material material = Lab4Materials.Create($"Lab4_Part_{spec.PartId}", spec.Color);

            GameObject part = new GameObject($"Part {spec.DisplayName}");
            part.transform.SetParent(parent, false);
            part.transform.position = new Vector3(x, BenchHeight + 0.12f, BenchZ - 0.05f);

            BuildPartVisual(part.transform, spec, material);

            BoxCollider collider = part.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.14f, 0.14f, 0.14f);

            Rigidbody body = part.AddComponent<Rigidbody>();
            body.mass = 0.4f;
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            XRGrabInteractable grab = part.AddComponent<XRGrabInteractable>();
            // Instantaneous: дрібна деталь має сидіти точно в пальцях, без «гумового» відставання.
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.useDynamicAttach = true;
            grab.throwOnDetach = false;

            AssemblyPart component = part.AddComponent<AssemblyPart>();
            Lab3Serialized.For(component)
                .Text("partId", spec.PartId)
                .Text("displayName", spec.DisplayName)
                .Text("specification", spec.Specification)
                .Apply();

            return component;
        }

        /// <summary>Кожна деталь має свій силует — інакше їх не відрізнити під час складання.</summary>
        private static void BuildPartVisual(Transform parent, PartSpec spec, Material material)
        {
            switch (spec.PartId)
            {
                case "gasket":
                    Lab3Environment.CreateCylinder("Ring", Vector3.zero, Vector3.zero,
                        new Vector3(0.14f, 0.012f, 0.14f), material, parent);
                    break;

                case "manometer":
                    Lab3Environment.CreateCylinder("Case", new Vector3(0f, 0.03f, 0f), new Vector3(90f, 0f, 0f),
                        new Vector3(0.12f, 0.02f, 0.12f), material, parent);
                    Lab3Environment.CreateCylinder("Stem", new Vector3(0f, -0.04f, 0f), Vector3.zero,
                        new Vector3(0.03f, 0.04f, 0.03f), Lab4Materials.Steel, parent);
                    break;

                case "filter":
                    Lab3Environment.CreateCylinder("Cartridge", Vector3.zero, Vector3.zero,
                        new Vector3(0.09f, 0.09f, 0.09f), material, parent);
                    break;

                default:
                    Lab3Environment.CreateBox("Body", Vector3.zero, new Vector3(0.11f, 0.11f, 0.11f), material, parent);
                    Lab3Environment.CreateCylinder("Handle", new Vector3(0f, 0.08f, 0f), new Vector3(0f, 0f, 90f),
                        new Vector3(0.025f, 0.08f, 0.025f), Lab4Materials.Accent, parent);
                    break;
            }

            foreach (Collider collider in parent.GetComponentsInChildren<Collider>())
            {
                // Колайдер деталі один — на кореневому об'єкті, інакше захоплення
                // й фіксація в пазу рахувалися б по кількох формах одразу.
                Object.DestroyImmediate(collider);
            }
        }
    }
}
