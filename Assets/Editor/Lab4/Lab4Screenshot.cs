using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Знімки сцени лабораторної 4 з фіксованих ракурсів — для звіту.
    /// Меню: Lab4 -> 5. Capture Screenshots
    /// </summary>
    public static class Lab4Screenshot
    {
        private const string OutputFolder = "Screenshots";
        private const int Width = 1280;
        private const int Height = 720;

        [MenuItem("Lab4/5. Capture Screenshots", priority = 5)]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(Lab4SceneBuilder.ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(OutputFolder);

            CaptureShot("lab4_area", new Vector3(0f, 1.7f, -3.2f), new Vector3(6f, 0f, 0f));
            CaptureShot("lab4_node", new Vector3(0f, 1.5f, -0.35f), new Vector3(18f, 0f, 0f));
            CaptureShot("lab4_console", new Vector3(1.75f, 1.45f, -0.1f), new Vector3(28f, 8f, 0f));

            Debug.Log($"Lab4: скриншоти збережено у {Path.GetFullPath(OutputFolder)}");
        }

        private static void CaptureShot(string fileName, Vector3 position, Vector3 eulerAngles)
        {
            GameObject cameraObject = new GameObject("Lab4 Capture Camera");
            cameraObject.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            // Без даних URP камера рендерить сцену вбудованим конвеєром і кольори матеріалів губляться.
            cameraObject.AddComponent<UniversalAdditionalCameraData>();

            RenderTexture target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;

            try
            {
                camera.targetTexture = target;
                // Перший кадр після відкриття сцени малюється до прогріву шейдерів URP
                // і виходить знебарвленим, тому рендеримо двічі й зберігаємо другий кадр.
                camera.Render();
                camera.Render();

                RenderTexture.active = target;
                image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                image.Apply();

                File.WriteAllBytes(Path.Combine(OutputFolder, $"{fileName}.png"), image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(image);
                target.Release();
                Object.DestroyImmediate(target);
            }
        }
    }
}
