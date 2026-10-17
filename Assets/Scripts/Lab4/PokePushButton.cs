using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab4
{
    /// <summary>
    /// Об'ємна кнопка стенда: натискається витягнутим вказівним пальцем (Poke Interaction).
    /// Натискання приходить від <c>XRPokeInteractor</c> через <c>XRPokeFilter</c>,
    /// кнопки контролера не потрібні. Ковпачок просідає на <see cref="pressDepth"/> метра.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class PokePushButton : MonoBehaviour
    {
        [SerializeField] private Transform cap;
        [SerializeField] private float pressDepth = 0.012f;
        [SerializeField] private float returnSpeed = 0.12f;
        [SerializeField] private UnityEvent pressed = new UnityEvent();

        private XRSimpleInteractable interactable;
        private Vector3 capHome;
        private bool isPressed;

        /// <summary>Подія натискання пальцем.</summary>
        public UnityEvent Pressed => pressed;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();

            if (cap != null)
            {
                capHome = cap.localPosition;
            }
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(HandlePressed);
            interactable.selectExited.AddListener(HandleReleased);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(HandlePressed);
            interactable.selectExited.RemoveListener(HandleReleased);
        }

        private void Update()
        {
            if (cap == null || isPressed)
            {
                return;
            }

            cap.localPosition = Vector3.MoveTowards(cap.localPosition, capHome, returnSpeed * Time.deltaTime);
        }

        private void HandlePressed(SelectEnterEventArgs args)
        {
            isPressed = true;

            if (cap != null)
            {
                cap.localPosition = capHome - new Vector3(0f, pressDepth, 0f);
            }

            pressed.Invoke();
        }

        private void HandleReleased(SelectExitEventArgs args)
        {
            isPressed = false;
        }
    }
}
