using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab4
{
    /// <summary>
    /// Об'ємний слайдер тиску опресовки. Повзунок їде за кінчиком пальця, поки палець
    /// тримається в зоні повзунка: беремо позицію <c>attachTransform</c> інтерактора
    /// (для <c>XRPokeInteractor</c> це кінчик вказівного пальця) і проєктуємо її на вісь напрямної.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class PokeSlider : MonoBehaviour
    {
        /// <summary>UnityEvent&lt;T&gt; серіалізується лише через власний підклас.</summary>
        [System.Serializable]
        public class ValueEvent : UnityEvent<float>
        {
        }

        [SerializeField] private Transform handle;
        [SerializeField] private Text label;
        [SerializeField] private string caption = "Тиск опресовки";
        [SerializeField] private string units = "бар";
        [SerializeField] private float travel = 0.3f;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 10f;
        [SerializeField] private float startValue = 4f;
        [SerializeField] private ValueEvent valueChanged = new ValueEvent();

        private XRSimpleInteractable interactable;
        private Transform follow;

        /// <summary>Поточне значення в одиницях шкали.</summary>
        public float Value { get; private set; }

        /// <summary>Подія зміни значення.</summary>
        public ValueEvent ValueChanged => valueChanged;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            SetValue(startValue);
        }

        private void OnEnable()
        {
            interactable.hoverEntered.AddListener(HandleHoverEntered);
            interactable.hoverExited.AddListener(HandleHoverExited);
        }

        private void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            interactable.hoverExited.RemoveListener(HandleHoverExited);
        }

        private void Update()
        {
            if (follow == null)
            {
                return;
            }

            // Напрямна лежить уздовж локальної осі X слайдера, тому позицію пальця
            // переводимо в локальні координати й беремо лише X.
            Vector3 local = transform.InverseTransformPoint(follow.position);
            float half = travel * 0.5f;
            float normalized = Mathf.InverseLerp(-half, half, Mathf.Clamp(local.x, -half, half));

            SetValue(Mathf.Lerp(minValue, maxValue, normalized));
        }

        /// <summary>Задає значення й пересуває повзунок — використовується і кнопкою скидання.</summary>
        public void SetValue(float value)
        {
            float clamped = Mathf.Clamp(value, minValue, maxValue);
            if (!Mathf.Approximately(clamped, Value))
            {
                Value = clamped;
                valueChanged.Invoke(Value);
            }

            float normalized = Mathf.InverseLerp(minValue, maxValue, Value);

            if (handle != null)
            {
                Vector3 position = handle.localPosition;
                position.x = Mathf.Lerp(-travel * 0.5f, travel * 0.5f, normalized);
                handle.localPosition = position;
            }

            if (label != null)
            {
                label.text = $"{caption}: {Value:0.0} {units}";
            }
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactorObject is XRPokeInteractor poke)
            {
                follow = poke.attachTransform != null ? poke.attachTransform : poke.transform;
            }
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            if (args.interactorObject is XRPokeInteractor)
            {
                follow = null;
            }
        }
    }
}
