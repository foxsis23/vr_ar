using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Lab4.EditorTools
{
    /// <summary>
    /// Підготовка проєкту до лабораторної 4 — природні взаємодії руками.
    /// Крім семплів лабораторної 2 (Starter Assets + XR Interaction Simulator) потрібні
    /// пакет XR Hands, його семпл HandVisualizer та семпл XRI «Hands Interaction Demo»
    /// з ригом XR Origin Hands. Додатково вмикається функція OpenXR «Hand Tracking Subsystem».
    /// Меню: Lab4 -> 1. Setup Hand Tracking
    /// </summary>
    public static class Lab4Setup
    {
        public const string HandsPackageName = "com.unity.xr.hands";
        public const string XriPackageName = "com.unity.xr.interaction.toolkit";
        public const string HandsDemoSample = "Hands Interaction Demo";
        public const string HandVisualizerSample = "HandVisualizer";

        private const string HandTrackingFeatureType = "HandTracking";

        [MenuItem("Lab4/1. Setup Hand Tracking", priority = 1)]
        public static void Setup()
        {
            Lab2.EditorTools.Lab2Setup.Setup();

            ImportSample(XriPackageName, HandsDemoSample);
            ImportSample(HandsPackageName, HandVisualizerSample);

            EnableHandTrackingFeature(BuildTargetGroup.Standalone);
            EnableHandTrackingFeature(BuildTargetGroup.Android);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Lab4: середовище відстеження рук готове. Далі — меню Lab4 -> 2. Build Hand Tracking Scene.");
        }

        [MenuItem("Lab4/4. Open XR Plug-in Management", priority = 4)]
        public static void OpenXrSettings()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }

        private static void ImportSample(string packageName, string displayName)
        {
            foreach (Sample sample in Sample.FindByPackage(packageName, string.Empty))
            {
                if (sample.displayName != displayName)
                {
                    continue;
                }

                if (sample.isImported)
                {
                    Debug.Log($"Lab4: семпл '{displayName}' вже імпортовано.");
                    return;
                }

                if (!sample.Import(Sample.ImportOptions.HideImportWindow))
                {
                    Debug.LogError($"Lab4: не вдалося імпортувати семпл '{displayName}'.");
                }

                return;
            }

            Debug.LogError($"Lab4: семпл '{displayName}' не знайдено в пакеті {packageName}. " +
                           "Перевірте, що пакет встановлено (Window -> Package Manager).");
        }

        /// <summary>
        /// Вмикає функцію OpenXR «Hand Tracking Subsystem» — без неї підсистема рук
        /// не стартує ні на шоломі, ні в симуляторі. Функцію шукаємо за типом, щоб
        /// скрипт компілювався навіть до встановлення пакета XR Hands.
        /// </summary>
        private static void EnableHandTrackingFeature(BuildTargetGroup targetGroup)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
            if (settings == null)
            {
                Debug.LogWarning($"Lab4: налаштування OpenXR для {targetGroup} не знайдено.");
                return;
            }

            foreach (OpenXRFeature feature in settings.GetFeatures())
            {
                if (feature == null || feature.GetType().Name != HandTrackingFeatureType)
                {
                    continue;
                }

                feature.enabled = true;
                EditorUtility.SetDirty(feature);
                Debug.Log($"Lab4: OpenXR Hand Tracking Subsystem увімкнено для {targetGroup}.");
                return;
            }

            Debug.LogWarning($"Lab4: функцію Hand Tracking Subsystem для {targetGroup} не знайдено — " +
                             "увімкніть її вручну у Project Settings -> XR Plug-in Management -> OpenXR.");
        }
    }
}
