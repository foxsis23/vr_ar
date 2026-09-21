using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Підготовка проєкту до лабораторної 2: імпорт семплів XR Interaction Toolkit
    /// (Starter Assets + XR Interaction Simulator) та резервування Interaction Layer 31 для телепортації.
    /// Меню: Lab2 -> 1. Setup XR Project
    /// </summary>
    public static class Lab2Setup
    {
        public const string PackageName = "com.unity.xr.interaction.toolkit";
        public const string StarterAssetsSample = "Starter Assets";
        public const string SimulatorSample = "XR Interaction Simulator";

        private const int TeleportLayerIndex = 31;
        private const string TeleportLayerName = "Teleport";

        [MenuItem("Lab2/1. Setup XR Project", priority = 1)]
        public static void Setup()
        {
            ImportSample(StarterAssetsSample);
            ImportSample(SimulatorSample);
            ReserveTeleportInteractionLayer();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Lab2: семпли XRI імпортовано. Далі — меню Lab2 -> 2. Build VR Scene.");
        }

        private static void ImportSample(string displayName)
        {
            foreach (Sample sample in Sample.FindByPackage(PackageName, string.Empty))
            {
                if (sample.displayName != displayName)
                {
                    continue;
                }

                if (sample.isImported)
                {
                    Debug.Log($"Lab2: семпл '{displayName}' вже імпортовано.");
                    return;
                }

                if (!sample.Import(Sample.ImportOptions.HideImportWindow))
                {
                    Debug.LogError($"Lab2: не вдалося імпортувати семпл '{displayName}'.");
                }

                return;
            }

            Debug.LogError($"Lab2: семпл '{displayName}' не знайдено в пакеті {PackageName}.");
        }

        /// <summary>
        /// Interaction Layer 31 зарезервований семплами XRI під телепортацію.
        /// Тип налаштувань internal, тому доступ через рефлексію.
        /// </summary>
        private static void ReserveTeleportInteractionLayer()
        {
            Type settingsType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.FullName == "UnityEngine.XR.Interaction.Toolkit.InteractionLayerSettings");

            if (settingsType == null)
            {
                Debug.LogWarning("Lab2: InteractionLayerSettings не знайдено — задайте Interaction Layer 31 = Teleport вручну.");
                return;
            }

            PropertyInfo instanceProperty = settingsType.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            object settings = instanceProperty?.GetValue(null);

            MethodInfo setLayerName = settingsType.GetMethod(
                "SetLayerNameAt",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (settings == null || setLayerName == null)
            {
                Debug.LogWarning("Lab2: не вдалося налаштувати Interaction Layer 31 — задайте 'Teleport' вручну.");
                return;
            }

            setLayerName.Invoke(settings, new object[] { TeleportLayerIndex, TeleportLayerName });
            EditorUtility.SetDirty((UnityEngine.Object)settings);
            Debug.Log($"Lab2: Interaction Layer {TeleportLayerIndex} = '{TeleportLayerName}'.");
        }

        [MenuItem("Lab2/4. Open XR Plug-in Management", priority = 4)]
        public static void OpenXrSettings()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
    }
}
