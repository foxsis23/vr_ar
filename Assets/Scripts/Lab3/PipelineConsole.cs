using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Стаціонарна консоль біля труби: імітація витоку, показники тиску
    /// та швидкий виклик потрібного екрана просторового планшета.
    /// Кнопки консолі розміщені на висоті руки — зручно натискати пальцем (Direct Poke).
    /// </summary>
    public class PipelineConsole : MonoBehaviour
    {
        [SerializeField] private PipelineNetwork network;
        [SerializeField] private SpatialTablet tablet;
        [SerializeField] private Button leakButton;
        [SerializeField] private Text leakButtonLabel;
        [SerializeField] private Button openWizardButton;
        [SerializeField] private Button openFormButton;

        private void OnEnable()
        {
            if (leakButton != null)
            {
                leakButton.onClick.AddListener(HandleLeakClick);
            }

            if (openWizardButton != null && tablet != null)
            {
                openWizardButton.onClick.AddListener(tablet.OpenWizard);
            }

            if (openFormButton != null && tablet != null)
            {
                openFormButton.onClick.AddListener(tablet.OpenForm);
            }

            RefreshLeakLabel();
        }

        private void OnDisable()
        {
            if (leakButton != null)
            {
                leakButton.onClick.RemoveListener(HandleLeakClick);
            }

            if (openWizardButton != null && tablet != null)
            {
                openWizardButton.onClick.RemoveListener(tablet.OpenWizard);
            }

            if (openFormButton != null && tablet != null)
            {
                openFormButton.onClick.RemoveListener(tablet.OpenForm);
            }
        }

        private void HandleLeakClick()
        {
            if (network == null)
            {
                return;
            }

            network.ToggleLeak();
            RefreshLeakLabel();
        }

        private void RefreshLeakLabel()
        {
            if (leakButtonLabel == null || network == null)
            {
                return;
            }

            leakButtonLabel.text = network.LeakActive
                ? "Усунути витік\n(скинути аварію)"
                : "Імітувати витік\nна ділянці";
        }
    }
}
