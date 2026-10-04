using UnityEditor;
using UnityEngine;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Приміщення газорозподільної станції: підлога, стіни, стеля та освітлення.
    /// Також спільні утиліти створення матеріалів і примітивів для сцени лабораторної 3.
    /// </summary>
    public static class Lab3Environment
    {
        public const string MaterialsFolder = "Assets/Materials/Lab3";

        public const float HallWidth = 12f;
        public const float HallDepth = 10f;
        public const float WallHeight = 3.5f;

        public static GameObject BuildHall()
        {
            Material floorMaterial = CreateMaterial("Lab3_Floor", new Color(0.38f, 0.39f, 0.41f));
            Material wallMaterial = CreateMaterial("Lab3_Wall", new Color(0.70f, 0.71f, 0.69f));
            Material ceilingMaterial = CreateMaterial("Lab3_Ceiling", new Color(0.86f, 0.87f, 0.89f));

            GameObject hall = new GameObject("Compressor Hall");

            GameObject floor = CreateBox("Floor", new Vector3(0f, -0.05f, 0f),
                new Vector3(HallWidth, 0.1f, HallDepth), floorMaterial, hall.transform);
            floor.name = "Floor";

            float halfWidth = HallWidth * 0.5f;
            float halfDepth = HallDepth * 0.5f;
            float wallCenter = WallHeight * 0.5f;

            CreateBox("Wall North", new Vector3(0f, wallCenter, halfDepth), new Vector3(HallWidth, WallHeight, 0.1f), wallMaterial, hall.transform);
            CreateBox("Wall South", new Vector3(0f, wallCenter, -halfDepth), new Vector3(HallWidth, WallHeight, 0.1f), wallMaterial, hall.transform);
            CreateBox("Wall East", new Vector3(halfWidth, wallCenter, 0f), new Vector3(0.1f, WallHeight, HallDepth), wallMaterial, hall.transform);
            CreateBox("Wall West", new Vector3(-halfWidth, wallCenter, 0f), new Vector3(0.1f, WallHeight, HallDepth), wallMaterial, hall.transform);
            CreateBox("Ceiling", new Vector3(0f, WallHeight, 0f), new Vector3(HallWidth, 0.1f, HallDepth), ceilingMaterial, hall.transform);

            return hall;
        }

        public static GameObject FindFloor(GameObject hall)
        {
            return hall.transform.Find("Floor").gameObject;
        }

        public static void BuildLights()
        {
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 0.6f;
            sunLight.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreateLamp("Lamp West", new Vector3(-3.5f, 3.1f, 1.5f));
            CreateLamp("Lamp East", new Vector3(3.5f, 3.1f, 1.5f));
            CreateLamp("Lamp South", new Vector3(0f, 3.1f, -2.5f));
        }

        public static Material CreateMaterial(string name, Color color, bool emissive = false)
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

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2f);
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

        public static GameObject CreateCylinder(string name, Vector3 localPosition, Vector3 localEuler, Vector3 scale, Material material, Transform parent)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = Quaternion.Euler(localEuler);
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            return cylinder;
        }

        private static void CreateLamp(string name, Vector3 position)
        {
            GameObject lamp = new GameObject(name);
            lamp.transform.position = position;

            Light light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 0.9f;
            light.color = new Color(1f, 0.95f, 0.88f);
            light.shadows = LightShadows.Soft;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Lab3");
            }
        }
    }
}
