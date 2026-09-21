using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Знімки VR-сцени з фіксованих ракурсів — для звіту та швидкої перевірки вигляду сцени.
    /// Меню: Lab2 -> 5. Capture Screenshots
    /// </summary>
    public static class Lab2Screenshot
    {
        private const string ScenePath = "Assets/Scenes/Lab2_VR.unity";
        private const string OutputFolder = "Screenshots";
        private const int Width = 1280;
        private const int Height = 720;

        [MenuItem("Lab2/5. Capture Screenshots", priority = 5)]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(OutputFolder);

            CaptureShot("lab2_hud", new Vector3(0f, 1.6f, -2.5f), new Vector3(5f, 0f, 0f));
            CaptureShot("lab2_table", new Vector3(0f, 1.6f, 0.2f), new Vector3(20f, 0f, 0f));
            CaptureShot("lab2_room", new Vector3(-3.2f, 2.2f, -3.2f), new Vector3(15f, 45f, 0f));

            Debug.Log($"Lab2: скриншоти збережено у {Path.GetFullPath(OutputFolder)}");
        }

        private static void CaptureShot(string fileName, Vector3 position, Vector3 eulerAngles)
        {
            GameObject cameraObject = new GameObject("Lab2 Capture Camera");
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
