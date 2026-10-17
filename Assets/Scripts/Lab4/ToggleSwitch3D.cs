using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab4
{
    /// <summary>
    /// Об'ємний тумблер живлення стенда. Перемикається дотиком пальця (Poke Interaction):
    /// важіль повертається між двома кутами, стан іде у <see cref="Toggled"/>.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ToggleSwitch3D : MonoBehaviour
    {
        /// <summary>UnityEvent&lt;T&gt; серіалізується лише через власний підклас.</summary>
        [System.Serializable]
        public class StateEvent : UnityEvent<bool>
        {
        }

        [SerializeField] private Transform lever;
        [SerializeField] private Text label;
        [SerializeField] private string caption = "Живлення стенда";
        [SerializeField] private float onAngle = -30f;
        [SerializeField] private float offAngle = 30f;
        [SerializeField] private bool isOn;
        [SerializeField] private StateEvent toggled = new StateEvent();

        private XRSimpleInteractable interactable;

        /// <summary>Поточний стан тумблера.</summary>
        public bool IsOn => isOn;

        /// <summary>Подія перемикання — передає новий стан.</summary>
        public StateEvent Toggled => toggled;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            ApplyState();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(HandleSelected);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(HandleSelected);
        }

        private void HandleSelected(SelectEnterEventArgs args)
        {
            isOn = !isOn;
            ApplyState();
            toggled.Invoke(isOn);
        }

        private void ApplyState()
        {
            if (lever != null)
            {
                lever.localRotation = Quaternion.Euler(isOn ? onAngle : offAngle, 0f, 0f);
            }

            if (label != null)
            {
                label.text = isOn ? $"{caption}: УВІМК." : $"{caption}: ВИМК.";
            }
        }
    }
}
