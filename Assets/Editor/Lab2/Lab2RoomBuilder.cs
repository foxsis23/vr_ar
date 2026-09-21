using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Геометрія синтетичної кімнати для VR-сцени: підлога, стіни, стеля, меблі,
    /// предмети для захоплення, зона точного розміщення та інформаційна панель.
    /// </summary>
    public static class Lab2RoomBuilder
    {
        public const string MaterialsFolder = "Assets/Materials/Lab2";

        public const float RoomSize = 8f;
        public const float WallHeight = 3f;
        public const float TableHeight = 0.75f;

        public static GameObject BuildRoom()
        {
            Material floorMaterial = CreateMaterial("Lab2_Floor", new Color(0.62f, 0.58f, 0.52f));
            Material wallMaterial = CreateMaterial("Lab2_Wall", new Color(0.82f, 0.80f, 0.76f));
            Material ceilingMaterial = CreateMaterial("Lab2_Ceiling", new Color(0.92f, 0.92f, 0.94f));

            GameObject room = new GameObject("Synthetic Room");

            GameObject floor = CreateBox("Floor", new Vector3(0f, -0.05f, 0f),
                new Vector3(RoomSize, 0.1f, RoomSize), floorMaterial, room.transform);

            float half = RoomSize * 0.5f;
            float wallCenter = WallHeight * 0.5f;
            CreateBox("Wall North", new Vector3(0f, wallCenter, half), new Vector3(RoomSize, WallHeight, 0.1f), wallMaterial, room.transform);
            CreateBox("Wall South", new Vector3(0f, wallCenter, -half), new Vector3(RoomSize, WallHeight, 0.1f), wallMaterial, room.transform);
            CreateBox("Wall East", new Vector3(half, wallCenter, 0f), new Vector3(0.1f, WallHeight, RoomSize), wallMaterial, room.transform);
            CreateBox("Wall West", new Vector3(-half, wallCenter, 0f), new Vector3(0.1f, WallHeight, RoomSize), wallMaterial, room.transform);
            CreateBox("Ceiling", new Vector3(0f, WallHeight, 0f), new Vector3(RoomSize, 0.1f, RoomSize), ceilingMaterial, room.transform);

            floor.name = "Floor";
            return room;
        }

        public static GameObject FindFloor(GameObject room)
        {
            return room.transform.Find("Floor").gameObject;
        }

        public static void BuildLights()
        {
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.1f;
            sunLight.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            CreateLamp("Lamp Left", new Vector3(-2f, 2.6f, 1.5f));
            CreateLamp("Lamp Right", new Vector3(2f, 2.6f, -1.5f));
        }

        /// <summary>
        /// Робоча поверхня, на якій стоять предмети і розміщена зона фіксації.
        /// </summary>
        public static Transform BuildTable(Vector3 position)
        {
            Material woodMaterial = CreateMaterial("Lab2_Wood", new Color(0.55f, 0.36f, 0.20f));

            GameObject table = new GameObject("Work Table");
            table.transform.position = position;

            CreateBox("Table Top", new Vector3(0f, TableHeight, 0f), new Vector3(1.6f, 0.08f, 0.9f), woodMaterial, table.transform);

            float legX = 0.7f;
            float legZ = 0.35f;
            foreach (Vector3 offset in new[]
            {
                new Vector3(legX, TableHeight * 0.5f, legZ),
                new Vector3(-legX, TableHeight * 0.5f, legZ),
                new Vector3(legX, TableHeight * 0.5f, -legZ),
                new Vector3(-legX, TableHeight * 0.5f, -legZ),
            })
            {
                CreateBox("Table Leg", offset, new Vector3(0.08f, TableHeight, 0.08f), woodMaterial, table.transform);
            }

            return table.transform;
        }

        public static void BuildShelf(Vector3 position)
        {
            Material shelfMaterial = CreateMaterial("Lab2_Shelf", new Color(0.45f, 0.45f, 0.50f));

            GameObject shelf = new GameObject("Wall Shelf");
            shelf.transform.position = position;
            CreateBox("Shelf Board", Vector3.zero, new Vector3(1.4f, 0.06f, 0.35f), shelfMaterial, shelf.transform);
            CreateBox("Shelf Bracket", new Vector3(-0.6f, -0.15f, 0f), new Vector3(0.06f, 0.3f, 0.3f), shelfMaterial, shelf.transform);
            CreateBox("Shelf Bracket", new Vector3(0.6f, -0.15f, 0f), new Vector3(0.06f, 0.3f, 0.3f), shelfMaterial, shelf.transform);
        }

        /// <summary>
        /// Предмет, який можна взяти віртуальною рукою, утримувати та кинути.
        /// </summary>
        public static GameObject BuildGrabbable(string name, PrimitiveType shape, Vector3 position, Vector3 scale, Color color)
        {
            GameObject grabbable = GameObject.CreatePrimitive(shape);
            grabbable.name = name;
            grabbable.transform.position = position;
            grabbable.transform.localScale = scale;
            grabbable.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Lab2_{name}", color);

            Rigidbody body = grabbable.AddComponent<Rigidbody>();
            body.mass = 0.7f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            XRGrabInteractable grab = grabbable.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            grab.useDynamicAttach = true;
            grab.smoothPosition = true;
            grab.smoothRotation = true;
            grab.throwOnDetach = true;
            grab.throwVelocityScale = 1.5f;
            grab.throwAngularVelocityScale = 1f;

            grabbable.AddComponent<GrabReporter>();
            return grabbable;
        }

        /// <summary>
        /// Зона точного розміщення (Socket Interactor): предмет підноситься — і фіксується на поверхні.
        /// </summary>
        public static GameObject BuildSnapZone(Vector3 position, Transform parent)
        {
            GameObject zone = new GameObject("Snap Zone");
            zone.transform.SetParent(parent, false);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(0.35f, 0.35f, 0.35f);

            GameObject attach = new GameObject("Attach");
            attach.transform.SetParent(zone.transform, false);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Zone Marker";
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(zone.transform, false);
            marker.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            marker.transform.localScale = new Vector3(0.32f, 0.005f, 0.32f);
            marker.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial("Lab2_SnapZone", new Color(0.25f, 0.55f, 0.95f), transparent: true);

            XRSocketInteractor socket = zone.AddComponent<XRSocketInteractor>();
            socket.attachTransform = attach.transform;
            socket.showInteractableHoverMeshes = true;
            socket.hoverSocketSnapping = true;
            socket.socketSnappingRadius = 0.15f;
            socket.recycleDelayTime = 0.2f;

            SnapZone indicator = zone.AddComponent<SnapZone>();
            SerializedObject serialized = new SerializedObject(indicator);
            serialized.FindProperty("zoneRenderer").objectReferenceValue = marker.GetComponent<Renderer>();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return zone;
        }

        /// <summary>
        /// Інформаційна панель на стіні: підказки керування, лічильники та журнал подій.
        /// </summary>
        public static VrHud BuildHud(Vector3 position, Quaternion rotation)
        {
            GameObject canvasObject = new GameObject("VR HUD");
            canvasObject.transform.SetPositionAndRotation(position, rotation);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.AddComponent<CanvasScaler>();

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(600f, 400f);
            canvasRect.localScale = Vector3.one * 0.0035f;

            GameObject background = CreateUiImage("Background", canvasRect, new Color(0.08f, 0.10f, 0.14f, 0.85f));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            Text title = CreateUiText("Title", canvasRect, new Vector2(0f, 160f), new Vector2(560f, 60f), 34, FontStyle.Bold);
            title.text = "Лабораторна 2 — VR-сцена";

            Text hints = CreateUiText("Hints", canvasRect, new Vector2(0f, 60f), new Vector2(560f, 140f), 24, FontStyle.Normal);
            Text status = CreateUiText("Status", canvasRect, new Vector2(0f, -60f), new Vector2(560f, 80f), 24, FontStyle.Bold);
            Text log = CreateUiText("Log", canvasRect, new Vector2(0f, -150f), new Vector2(560f, 120f), 20, FontStyle.Normal);

            VrHud hud = canvasObject.AddComponent<VrHud>();
            SerializedObject serialized = new SerializedObject(hud);
            serialized.FindProperty("hintsText").objectReferenceValue = hints;
            serialized.FindProperty("statusText").objectReferenceValue = status;
            serialized.FindProperty("logText").objectReferenceValue = log;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            hints.text =
                "Ліва/права рука: тригер — взяти предмет, відпустити — кинути\n" +
                "Стік уперед + тригер — промінь телепортації (Teleport Area / Anchor)\n" +
                "Лівий стік — плавний рух із затіненням периферії (Comfort Vignette)\n" +
                "Правий стік — поворот ривками";

            return hud;
        }

        public static Material CreateMaterial(string name, Color color, bool transparent = false)
        {
            EnsureFolder();

            string path = $"{MaterialsFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);

            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.renderQueue = 3000;
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                color.a = 0.45f;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static GameObject CreateBox(string name, Vector3 localPosition, Vector3 size, Material material, Transform parent)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private static void CreateLamp(string name, Vector3 position)
        {
            GameObject lamp = new GameObject(name);
            lamp.transform.position = position;

            Light light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 9f;
            light.intensity = 2.2f;
            light.color = new Color(1f, 0.93f, 0.82f);
            light.shadows = LightShadows.Soft;
        }

        private static GameObject CreateUiImage(string name, RectTransform parent, Color color)
        {
            GameObject image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            image.transform.SetParent(parent, false);
            image.GetComponent<Image>().color = color;
            return image;
        }

        private static Text CreateUiText(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, FontStyle style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.UpperCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Lab2");
            }
        }
    }
}
