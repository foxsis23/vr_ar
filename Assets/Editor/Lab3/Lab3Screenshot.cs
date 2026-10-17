using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Знімки сцени лабораторної 3 з фіксованих ракурсів — для звіту.
    /// Меню: Lab3 -> 5. Capture Screenshots
    /// </summary>
    public static class Lab3Screenshot
    {
        private const string OutputFolder = "Screenshots";
        private const int Width = 1280;
        private const int Height = 720;

        [MenuItem("Lab3/5. Capture Screenshots", priority = 5)]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(Lab3SceneBuilder.ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(OutputFolder);

            CaptureShot("lab3_hall", new Vector3(0f, 1.7f, -3.6f), new Vector3(6f, 0f, 0f));
            CaptureShot("lab3_console", new Vector3(0f, 1.5f, -0.4f), new Vector3(16f, 0f, 0f));
            CaptureShot("lab3_pipeline", new Vector3(-3.4f, 1.8f, 0.4f), new Vector3(8f, 38f, 0f));

            Debug.Log($"Lab3: скриншоти збережено у {Path.GetFullPath(OutputFolder)}");
        }

        private static void CaptureShot(string fileName, Vector3 position, Vector3 eulerAngles)
        {
            GameObject cameraObject = new GameObject("Lab3 Capture Camera");
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
