using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Примітиви просторового інтерфейсу: World-Space Canvas, панелі, тексти, кнопки,
    /// перемикачі, слайдери та поля введення. Кожен канвас отримує
    /// <see cref="TrackedDeviceGraphicRaycaster"/>, без якого XR-промінь і дотик по UI не працюють.
    /// </summary>
    public static class Lab3UiFactory
    {
        public static readonly Color PanelColor = new Color(0.07f, 0.09f, 0.13f, 0.94f);
        public static readonly Color SurfaceColor = new Color(0.13f, 0.16f, 0.21f, 1f);
        public static readonly Color ButtonColor = new Color(0.18f, 0.21f, 0.27f, 1f);
        public static readonly Color AccentColor = new Color(0.20f, 0.52f, 0.85f, 1f);
        public static readonly Color TextColor = new Color(0.93f, 0.95f, 0.97f, 1f);

        /// <summary>
        /// Канвас у світовому просторі. Розмір задається в пікселях, масштаб переводить їх у метри
        /// (0.001 означає «1000 px = 1 м»).
        /// </summary>
        public static Canvas CreateWorldCanvas(string name, Transform parent, Vector2 size, float metersPerPixel)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one * metersPerPixel;

            return canvas;
        }

        public static Image CreatePanel(string name, RectTransform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            Stretch(panel.GetComponent<RectTransform>());
            return panel.GetComponent<Image>();
        }

        /// <summary>Вертикальна колонка: дочірні блоки самі розсуваються за висотою вмісту.</summary>
        public static RectTransform CreateColumn(string name, RectTransform parent, float spacing, RectOffset padding)
        {
            GameObject column = new GameObject(name, typeof(RectTransform));
            column.transform.SetParent(parent, false);

            RectTransform rect = column.GetComponent<RectTransform>();
            Stretch(rect);

            VerticalLayoutGroup layout = column.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return rect;
        }

        public static RectTransform CreateRow(string name, RectTransform parent, float spacing, float height)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            SetPreferredHeight(row, height);
            return row.GetComponent<RectTransform>();
        }

        public static Text CreateText(
            string name,
            RectTransform parent,
            string content,
            int fontSize,
            FontStyle style = FontStyle.Normal,
            TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            ApplyFont(text, fontSize, style, anchor);
            text.text = content;

            // Висота блоку рахується з реального тексту, тому сусідні блоки не перекриваються.
            ContentSizeFitter fitter = textObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return text;
        }

        public static Button CreateButton(string name, RectTransform parent, string caption, int fontSize, float height, out Text label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            buttonObject.transform.SetParent(parent, false);

            Image background = buttonObject.GetComponent<Image>();
            background.color = ButtonColor;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;

            label = CreateText("Label", buttonObject.GetComponent<RectTransform>(), caption, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
            Object.DestroyImmediate(label.GetComponent<ContentSizeFitter>());
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12f, 6f);
            label.rectTransform.offsetMax = new Vector2(-12f, -6f);

            SetPreferredHeight(buttonObject, height);
            return button;
        }

        public static Toggle CreateToggle(string name, RectTransform parent, string caption, int fontSize = 24, float height = 52f)
        {
            GameObject toggleObject = new GameObject(name, typeof(RectTransform));
            toggleObject.transform.SetParent(parent, false);

            Toggle toggle = toggleObject.AddComponent<Toggle>();
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();

            GameObject box = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            box.transform.SetParent(toggleRect, false);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(6f, 0f);
            boxRect.sizeDelta = new Vector2(38f, 38f);
            Image boxImage = box.GetComponent<Image>();
            boxImage.color = SurfaceColor;

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            check.transform.SetParent(boxRect, false);
            RectTransform checkRect = check.GetComponent<RectTransform>();
            Stretch(checkRect);
            checkRect.offsetMin = new Vector2(7f, 7f);
            checkRect.offsetMax = new Vector2(-7f, -7f);
            Image checkImage = check.GetComponent<Image>();
            checkImage.color = AccentColor;

            Text label = CreateText("Label", toggleRect, caption, fontSize, FontStyle.Normal, TextAnchor.MiddleLeft);
            Object.DestroyImmediate(label.GetComponent<ContentSizeFitter>());
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(56f, 0f);
            label.rectTransform.offsetMax = new Vector2(-8f, 0f);

            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;

            SetPreferredHeight(toggleObject, height);
            return toggle;
        }

        public static Slider CreateSlider(string name, RectTransform parent, float minValue, float maxValue, bool wholeNumbers, float height = 48f)
        {
            GameObject sliderObject = new GameObject(name, typeof(RectTransform));
            sliderObject.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(sliderRect, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.35f);
            backgroundRect.anchorMax = new Vector2(1f, 0.65f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = SurfaceColor;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRect, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
            fillAreaRect.offsetMin = new Vector2(14f, 0f);
            fillAreaRect.offsetMax = new Vector2(-14f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillAreaRect, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect);
            fill.GetComponent<Image>().color = AccentColor;

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderRect, false);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            Stretch(handleAreaRect);
            handleAreaRect.offsetMin = new Vector2(14f, 0f);
            handleAreaRect.offsetMax = new Vector2(-14f, 0f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleAreaRect, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            Stretch(handleRect);
            handleRect.sizeDelta = new Vector2(32f, 0f);
            Image handleImage = handle.GetComponent<Image>();
            handleImage.color = TextColor;

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.wholeNumbers = wholeNumbers;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = Mathf.Lerp(minValue, maxValue, 0.5f);

            SetPreferredHeight(sliderObject, height);
            return slider;
        }

        public static InputField CreateInputField(string name, RectTransform parent, string placeholderText, int fontSize = 24, float height = 58f)
        {
            GameObject fieldObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fieldObject.transform.SetParent(parent, false);

            Image background = fieldObject.GetComponent<Image>();
            background.color = SurfaceColor;

            RectTransform fieldRect = fieldObject.GetComponent<RectTransform>();

            Text placeholder = CreateText("Placeholder", fieldRect, placeholderText, fontSize, FontStyle.Italic, TextAnchor.MiddleLeft);
            Object.DestroyImmediate(placeholder.GetComponent<ContentSizeFitter>());
            PadToFill(placeholder.rectTransform);
            placeholder.color = new Color(0.60f, 0.65f, 0.72f, 1f);

            Text content = CreateText("Text", fieldRect, string.Empty, fontSize, FontStyle.Normal, TextAnchor.MiddleLeft);
            Object.DestroyImmediate(content.GetComponent<ContentSizeFitter>());
            PadToFill(content.rectTransform);
            content.supportRichText = false;
            content.horizontalOverflow = HorizontalWrapMode.Overflow;

            InputField field = fieldObject.AddComponent<InputField>();
            field.targetGraphic = background;
            field.textComponent = content;
            field.placeholder = placeholder;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 32;

            SetPreferredHeight(fieldObject, height);
            return field;
        }

        public static LayoutElement SetPreferredHeight(GameObject target, float height)
        {
            LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            element.flexibleHeight = 0f;
            return element;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void PadToFill(RectTransform rect)
        {
            Stretch(rect);
            rect.offsetMin = new Vector2(14f, 4f);
            rect.offsetMax = new Vector2(-14f, -4f);
        }

        private static void ApplyFont(Text text, int fontSize, FontStyle style, TextAnchor anchor)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
