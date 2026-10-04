using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Просторовий планшет (World-Space Canvas), який відкривається на запит:
    /// кнопка Menu на контролері або клавіша M у XR Interaction Simulator.
    /// При відкритті панель ставиться перед користувачем на відстані витягнутої руки,
    /// щоб працювали і дистанційний промінь, і прямий дотик пальцем.
    /// </summary>
    public class SpatialTablet : MonoBehaviour
    {
        private const float ToggleCooldown = 0.25f;

        [SerializeField] private GameObject panel;
        [SerializeField] private Canvas panelCanvas;
        [SerializeField] private GameObject wizardScreen;
        [SerializeField] private GameObject formScreen;
        [SerializeField] private Button wizardTabButton;
        [SerializeField] private Button formTabButton;
        [SerializeField] private Text tabHintText;
        [SerializeField] private float distance = 0.7f;
        [SerializeField] private float verticalOffset = -0.1f;

        private InputAction toggleAction;
        private float lastToggleTime = float.NegativeInfinity;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (panel == null)
            {
                return;
            }

            PlaceInFrontOfViewer();
            panel.SetActive(true);
        }

        public void Close()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void ShowWizard()
        {
            SetScreen(wizard: true);
        }

        public void ShowForm()
        {
            SetScreen(wizard: false);
        }

        /// <summary>Відкрити планшет одразу на потрібному екрані — викликається з консолі та візарда.</summary>
        public void OpenForm()
        {
            Open();
            ShowForm();
        }

        public void OpenWizard()
        {
            Open();
            ShowWizard();
        }

        private void Awake()
        {
            toggleAction = BuildToggleAction();
            SetScreen(wizard: true);
            Close();
        }

        private void OnEnable()
        {
            toggleAction.performed += HandleTogglePerformed;
            toggleAction.Enable();

            if (wizardTabButton != null)
            {
                wizardTabButton.onClick.AddListener(ShowWizard);
            }

            if (formTabButton != null)
            {
                formTabButton.onClick.AddListener(ShowForm);
            }
        }

        private void OnDisable()
        {
            toggleAction.performed -= HandleTogglePerformed;
            toggleAction.Disable();

            if (wizardTabButton != null)
            {
                wizardTabButton.onClick.RemoveListener(ShowWizard);
            }

            if (formTabButton != null)
            {
                formTabButton.onClick.RemoveListener(ShowForm);
            }
        }

        private void OnDestroy()
        {
            toggleAction?.Dispose();
        }

        /// <summary>
        /// Дія створюється кодом, щоб сцена не залежала від конкретного .inputactions-ассета.
        /// Клавіатурна прив'язка потрібна, коли симулятор керує головою, а не руками.
        /// </summary>
        private static InputAction BuildToggleAction()
        {
            InputAction action = new InputAction("Lab3/ToggleTablet", InputActionType.Button);
            action.AddBinding("<XRController>{LeftHand}/menuButton");
            action.AddBinding("<XRController>{RightHand}/menuButton");
            action.AddBinding("<Keyboard>/m");
            return action;
        }

        private void HandleTogglePerformed(InputAction.CallbackContext context)
        {
            // Симулятор транслює M і на клавіатуру, і на menuButton — без вікна очікування
            // панель перемкнулася б двічі за кадр і візуально не змінилася б.
            if (Time.unscaledTime - lastToggleTime < ToggleCooldown)
            {
                return;
            }

            lastToggleTime = Time.unscaledTime;
            Toggle();
        }

        private void PlaceInFrontOfViewer()
        {
            Camera viewer = Camera.main;
            if (viewer == null)
            {
                return;
            }

            if (panelCanvas != null)
            {
                panelCanvas.worldCamera = viewer;
            }

            Vector3 forward = viewer.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            Vector3 position = viewer.transform.position + forward * distance + Vector3.up * verticalOffset;
            panel.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
        }

        private void SetScreen(bool wizard)
        {
            if (wizardScreen != null)
            {
                wizardScreen.SetActive(wizard);
            }

            if (formScreen != null)
            {
                formScreen.SetActive(!wizard);
            }

            if (tabHintText != null)
            {
                tabHintText.text = wizard
                    ? "Інструктаж і перевірка знань"
                    : "Звіт про витік — заповнення даних";
            }
        }
    }
}
