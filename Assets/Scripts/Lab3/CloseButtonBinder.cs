using UnityEngine;
using UnityEngine.UI;

namespace Lab3
{
    /// <summary>
    /// Кнопка «Закрити» просторового планшета. Окремий компонент потрібен тому,
    /// що сцена генерується скриптом і зв'язки подій зручніше ставити посиланням, а не в інспекторі.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CloseButtonBinder : MonoBehaviour
    {
        [SerializeField] private SpatialTablet tablet;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (tablet != null)
            {
                button.onClick.AddListener(tablet.Close);
            }
        }

        private void OnDisable()
        {
            if (tablet != null)
            {
                button.onClick.RemoveListener(tablet.Close);
            }
        }
    }
}
