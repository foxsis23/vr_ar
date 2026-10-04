using UnityEditor;
using UnityEngine;

namespace Lab3.EditorTools
{
    /// <summary>
    /// Підготовка проєкту до лабораторної 3. Вимоги ті самі, що й у лабораторній 2
    /// (семпли Starter Assets + XR Interaction Simulator, Interaction Layer 31 = Teleport),
    /// тому імпорт делегується наявному інсталятору, а не дублюється.
    /// Меню: Lab3 -> 1. Setup XR Project
    /// </summary>
    public static class Lab3Setup
    {
        [MenuItem("Lab3/1. Setup XR Project", priority = 1)]
        public static void Setup()
        {
            Lab2.EditorTools.Lab2Setup.Setup();
            Debug.Log("Lab3: середовище готове. Далі — меню Lab3 -> 2. Build Spatial UI Scene.");
        }

        [MenuItem("Lab3/4. Open XR Plug-in Management", priority = 4)]
        public static void OpenXrSettings()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
    }
}
