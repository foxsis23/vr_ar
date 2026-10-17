using UnityEditor;
using UnityEngine;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Матеріали сцени лабораторної 4. Логіка створення та ж, що в лабораторній 3,
    /// але окрема тека — щоб кольори стенда не перетиналися з матеріалами попередньої сцени.
    /// </summary>
    public static class Lab4Materials
    {
        public const string Folder = "Assets/Materials/Lab4";

        public static Material Steel => Create("Lab4_Steel", new Color(0.55f, 0.57f, 0.60f));

        public static Material Bench => Create("Lab4_Bench", new Color(0.32f, 0.36f, 0.42f));

        public static Material Console => Create("Lab4_Console", new Color(0.22f, 0.25f, 0.30f));

        public static Material Accent => Create("Lab4_Accent", new Color(0.85f, 0.45f, 0.15f));

        public static Material Indicator => Create("Lab4_Indicator", new Color(0.95f, 0.65f, 0.15f), emissive: true);

        public static Material Create(string name, Color color, bool emissive = false)
        {
            EnsureFolder();

            string path = $"{Folder}/{name}.mat";
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
                material.SetColor("_EmissionColor", color * 1.5f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Lab4");
            }
        }
    }
}
