using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab3
{
    /// <summary>
    /// Регулювальний вентиль: вибір променем або дотиком перемикає стан «відкрито/перекрито»
    /// і змушує мережу перерахувати тиск по ділянках.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ValveControl : MonoBehaviour, IEquipmentInfo
    {
        private const float OpenAngle = 0f;
        private const float ClosedAngle = 90f;
        private const float TurnSpeed = 220f;

        [SerializeField] private string valveId = "V-1";
        [SerializeField] private Transform handwheel;
        [SerializeField] private bool isOpen = true;
        [SerializeField] private PipelineNetwork network;

        private XRSimpleInteractable interactable;

        public string ValveId => valveId;

        public bool IsOpen => isOpen;

        public string InfoTitle => $"Вентиль {valveId}";

        public string BuildInfoBody()
        {
            return
                "Кульовий кран DN 500, ручний привід.\n" +
                $"Стан: {(isOpen ? "відкрито" : "ПЕРЕКРИТО")}\n" +
                "Натисніть на вентиль променем або дотиком, щоб перемкнути.";
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            if (network != null)
            {
                network.Recalculate();
            }
        }

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(HandleSelectEntered);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(HandleSelectEntered);
        }

        private void Update()
        {
            if (handwheel == null)
            {
                return;
            }

            float target = isOpen ? OpenAngle : ClosedAngle;
            Quaternion targetRotation = Quaternion.Euler(0f, target, 0f);
            handwheel.localRotation = Quaternion.RotateTowards(
                handwheel.localRotation, targetRotation, TurnSpeed * Time.deltaTime);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            Toggle();
        }
    }
}
