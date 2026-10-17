using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Кнопка консолі, що керує одним вентилем. Розрахована і на дистанційний промінь,
    /// і на прямий дотик пальцем (Direct Poke), бо це звичайний UI-елемент World-Space Canvas.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ValveButton : MonoBehaviour
    {
        [SerializeField] private ValveControl valve;
        [SerializeField] private Text label;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClick);
            RefreshLabel();
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (valve == null)
            {
                return;
            }

            valve.Toggle();
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (label == null || valve == null)
            {
                return;
            }

            label.text = valve.IsOpen
                ? $"{valve.ValveId}: відкрито\nперекрити"
                : $"{valve.ValveId}: перекрито\nвідкрити";
        }
    }
}
