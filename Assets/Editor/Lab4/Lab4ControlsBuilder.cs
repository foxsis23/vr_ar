using Lab3.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Пульт стенда опресовки: об'ємний тумблер живлення, слайдер тиску та дві кнопки,
    /// усі розраховані на натискання витягнутим вказівним пальцем (Poke Interaction).
    /// </summary>
    public static class Lab4ControlsBuilder
    {
        private const float DeskHeight = 0.95f;
        private const float DeskZ = 0.85f;
        private const float DeskX = 1.75f;

        /// <summary>Зібрані органи керування — далі їх зв'язує <see cref="Lab4SceneBuilder"/>.</summary>
        public sealed class ControlRig
        {
            public ControlRig(
                GameObject root,
                ToggleSwitch3D powerSwitch,
                PokeSlider pressureSlider,
                PokePushButton startButton,
                PokePushButton resetButton,
                Renderer statusLamp)
            {
                Root = root;
                PowerSwitch = powerSwitch;
                PressureSlider = pressureSlider;
                StartButton = startButton;
                ResetButton = resetButton;
                StatusLamp = statusLamp;
            }

            public GameObject Root { get; }
            public ToggleSwitch3D PowerSwitch { get; }
            public PokeSlider PressureSlider { get; }
            public PokePushButton StartButton { get; }
            public PokePushButton ResetButton { get; }
            public Renderer StatusLamp { get; }
        }

        public static ControlRig Build()
        {
            GameObject root = new GameObject("Pressure Test Console");
            root.transform.position = new Vector3(DeskX, 0f, DeskZ);

            BuildDesk(root.transform);

            ToggleSwitch3D power = BuildPowerSwitch(root.transform, new Vector3(-0.3f, DeskHeight + 0.04f, 0f));
            PokeSlider slider = BuildSlider(root.transform, new Vector3(0.05f, DeskHeight + 0.05f, 0f));
            // Підписи кнопок рознесені по висоті: стоячи збоку, оператор бачить їх одна над одною.
            PokePushButton start = BuildButton(root.transform, "Start Button", "ПУСК опресовки",
                new Vector3(0.38f, DeskHeight + 0.04f, 0.12f), new Color(0.20f, 0.65f, 0.30f), 0.26f);
            PokePushButton reset = BuildButton(root.transform, "Reset Button", "СКИД",
                new Vector3(0.38f, DeskHeight + 0.04f, -0.14f), new Color(0.80f, 0.35f, 0.20f), 0.5f);
            Renderer lamp = BuildStatusLamp(root.transform, new Vector3(-0.55f, DeskHeight + 0.12f, 0f));

            return new ControlRig(root, power, slider, start, reset, lamp);
        }

        private static void BuildDesk(Transform parent)
        {
            Lab3Environment.CreateBox("Desk Top", new Vector3(0f, DeskHeight, 0f),
                new Vector3(1.3f, 0.08f, 0.6f), Lab4Materials.Console, parent);

            foreach (float x in new[] { -0.55f, 0.55f })
            {
                Lab3Environment.CreateBox("Leg", new Vector3(x, DeskHeight * 0.5f, 0f),
                    new Vector3(0.08f, DeskHeight, 0.08f), Lab4Materials.Steel, parent);
            }
        }

        /// <summary>Тумблер: нерухома основа з колайдером дотику та важіль, що перекидається.</summary>
        private static ToggleSwitch3D BuildPowerSwitch(Transform parent, Vector3 position)
        {
            GameObject switchObject = new GameObject("Power Switch");
            switchObject.transform.SetParent(parent, false);
            switchObject.transform.localPosition = position;

            Lab3Environment.CreateBox("Base", Vector3.zero, new Vector3(0.12f, 0.03f, 0.1f),
                Lab4Materials.Console, switchObject.transform);

            GameObject lever = new GameObject("Lever");
            lever.transform.SetParent(switchObject.transform, false);

            GameObject leverBody = Lab3Environment.CreateBox("Body", new Vector3(0f, 0.045f, 0f),
                new Vector3(0.035f, 0.09f, 0.035f), Lab4Materials.Accent, lever.transform);
            Object.DestroyImmediate(leverBody.GetComponent<Collider>());

            Text label = BuildLabel(switchObject.transform, "Живлення стенда: ВИМК.", new Vector3(0f, 0.26f, 0f));

            BoxCollider collider = ClearColliders(switchObject);
            collider.center = new Vector3(0f, 0.05f, 0f);
            collider.size = new Vector3(0.14f, 0.12f, 0.12f);

            XRSimpleInteractable interactable = switchObject.AddComponent<XRSimpleInteractable>();
            AddPokeFilter(switchObject, interactable, collider);

            ToggleSwitch3D component = switchObject.AddComponent<ToggleSwitch3D>();
            Lab3Serialized.For(component)
                .Reference("lever", lever.transform)
                .Reference("label", label)
                .Text("caption", "Живлення стенда")
                .Flag("isOn", false)
                .Apply();

            return component;
        }

        /// <summary>Слайдер: напрямна з повзунком, який їде за кінчиком пальця.</summary>
        private static PokeSlider BuildSlider(Transform parent, Vector3 position)
        {
            GameObject sliderObject = new GameObject("Pressure Slider");
            sliderObject.transform.SetParent(parent, false);
            sliderObject.transform.localPosition = position;

            GameObject rail = Lab3Environment.CreateBox("Rail", Vector3.zero, new Vector3(0.34f, 0.02f, 0.06f),
                Lab4Materials.Console, sliderObject.transform);
            Object.DestroyImmediate(rail.GetComponent<Collider>());

            GameObject handle = Lab3Environment.CreateBox("Handle", new Vector3(0f, 0.03f, 0f),
                new Vector3(0.05f, 0.05f, 0.08f), Lab4Materials.Accent, sliderObject.transform);
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            Text label = BuildLabel(sliderObject.transform, "Тиск опресовки: 4,0 бар", new Vector3(0f, 0.38f, 0f));

            BoxCollider collider = sliderObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.03f, 0f);
            collider.size = new Vector3(0.36f, 0.1f, 0.12f);
            collider.isTrigger = true;

            sliderObject.AddComponent<XRSimpleInteractable>();

            PokeSlider component = sliderObject.AddComponent<PokeSlider>();
            Lab3Serialized.For(component)
                .Reference("handle", handle.transform)
                .Reference("label", label)
                .Text("caption", "Тиск опресовки")
                .Text("units", "бар")
                .Number("travel", 0.3f)
                .Number("minValue", 0f)
                .Number("maxValue", 10f)
                .Number("startValue", 4f)
                .Apply();

            return component;
        }

        /// <summary>Об'ємна кнопка: корпус із колайдером дотику та ковпачок, що просідає.</summary>
        private static PokePushButton BuildButton(
            Transform parent,
            string name,
            string caption,
            Vector3 position,
            Color color,
            float labelHeight)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            buttonObject.transform.localPosition = position;

            GameObject housing = Lab3Environment.CreateBox("Housing", Vector3.zero,
                new Vector3(0.14f, 0.03f, 0.14f), Lab4Materials.Console, buttonObject.transform);
            Object.DestroyImmediate(housing.GetComponent<Collider>());

            GameObject cap = Lab3Environment.CreateCylinder("Cap", new Vector3(0f, 0.03f, 0f), Vector3.zero,
                new Vector3(0.1f, 0.018f, 0.1f), Lab4Materials.Create($"Lab4_{name.Replace(' ', '_')}", color),
                buttonObject.transform);
            Object.DestroyImmediate(cap.GetComponent<Collider>());

            BuildLabel(buttonObject.transform, caption, new Vector3(0f, labelHeight, 0f));

            BoxCollider collider = buttonObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.03f, 0f);
            collider.size = new Vector3(0.14f, 0.06f, 0.14f);

            XRSimpleInteractable interactable = buttonObject.AddComponent<XRSimpleInteractable>();
            AddPokeFilter(buttonObject, interactable, collider);

            PokePushButton component = buttonObject.AddComponent<PokePushButton>();
            Lab3Serialized.For(component)
                .Reference("cap", cap.transform)
                .Number("pressDepth", 0.012f)
                .Apply();

            return component;
        }

        private static Renderer BuildStatusLamp(Transform parent, Vector3 position)
        {
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Status Lamp";
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = position;
            lamp.transform.localScale = Vector3.one * 0.12f;
            lamp.GetComponent<Renderer>().sharedMaterial = Lab4Materials.Indicator;
            Object.DestroyImmediate(lamp.GetComponent<Collider>());

            return lamp.GetComponent<Renderer>();
        }

        /// <summary>
        /// Фільтр дотику: перетворює наближення кінчика пальця до колайдера
        /// у подію вибору інтерактабла. Вісь Y — палець тисне зверху вниз по пульту.
        /// </summary>
        private static void AddPokeFilter(GameObject target, XRBaseInteractable interactable, Collider collider)
        {
            XRPokeFilter filter = target.AddComponent<XRPokeFilter>();

            // Налаштування задаємо до прив'язки інтерактабла: саме його сеттер
            // перезапускає внутрішню логіку фільтра вже з готовою конфігурацією.
            PokeThresholdData configuration = filter.pokeConfiguration.Value;
            configuration.pokeDirection = PokeAxis.Y;
            configuration.interactionDepthOffset = 0.01f;
            configuration.enablePokeAngleThreshold = true;
            configuration.pokeAngleThreshold = 60f;

            filter.pokeCollider = collider;
            filter.pokeInteractable = interactable;
        }

        /// <summary>Підпис органа керування — невеликий World-Space Canvas над ним.</summary>
        private static Text BuildLabel(Transform parent, string caption, Vector3 localPosition)
        {
            Canvas canvas = Lab3UiFactory.CreateWorldCanvas("Label", parent, new Vector2(460f, 130f), 0.00045f);
            canvas.transform.localPosition = localPosition;
            // Канвас у World Space читається з боку, протилежного його forward: нахил по X
            // розвертає підпис до оператора, а зайвий поворот на 180° дав би дзеркальний текст.
            canvas.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Lab3UiFactory.CreatePanel("Background", canvasRect, Lab3UiFactory.PanelColor);

            RectTransform column = Lab3UiFactory.CreateColumn("Content", canvasRect, 6f, new RectOffset(16, 16, 12, 12));
            return Lab3UiFactory.CreateText("Text", column, caption, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        /// <summary>Примітиви приносять власні колайдери — лишаємо один спільний на корені.</summary>
        private static BoxCollider ClearColliders(GameObject target)
        {
            foreach (Collider collider in target.GetComponentsInChildren<Collider>())
            {
                Object.DestroyImmediate(collider);
            }

            return target.AddComponent<BoxCollider>();
        }
    }
}
