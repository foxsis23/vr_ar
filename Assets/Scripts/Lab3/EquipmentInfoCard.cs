using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab3
{
    /// <summary>
    /// Спливаюча інформаційна картка обладнання: з'являється при наведенні променя
    /// (або дотику) на 3D-об'єкт і лишається, поки об'єкт виділено.
    /// Текст беруть з <see cref="IEquipmentInfo"/> того ж об'єкта, тому він завжди актуальний.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class EquipmentInfoCard : MonoBehaviour
    {
        [SerializeField] private GameObject card;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private string fallbackTitle = "Обладнання";
        [SerializeField, TextArea] private string fallbackBody = "Опис недоступний.";

        private XRBaseInteractable interactable;
        private IEquipmentInfo info;
        private Transform viewer;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            info = GetComponent<IEquipmentInfo>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            interactable.hoverEntered.AddListener(HandleHoverEntered);
            interactable.hoverExited.AddListener(HandleHoverExited);
            interactable.selectEntered.AddListener(HandleSelectEntered);
            interactable.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            interactable.hoverExited.RemoveListener(HandleHoverExited);
            interactable.selectEntered.RemoveListener(HandleSelectEntered);
            interactable.selectExited.RemoveListener(HandleSelectExited);
        }

        private void LateUpdate()
        {
            if (card == null || !card.activeSelf)
            {
                return;
            }

            if (viewer == null)
            {
                Camera main = Camera.main;
                if (main == null)
                {
                    return;
                }

                viewer = main.transform;
            }

            // Картка завжди розвернута до користувача, інакше в VR її читати неможливо.
            Vector3 toViewer = card.transform.position - viewer.position;
            toViewer.y = 0f;
            if (toViewer.sqrMagnitude > 0.0001f)
            {
                card.transform.rotation = Quaternion.LookRotation(toViewer, Vector3.up);
            }
        }

        private void HandleHoverEntered(HoverEnterEventArgs args) => Refresh(true);

        private void HandleHoverExited(HoverExitEventArgs args) => Refresh(interactable.isSelected);

        private void HandleSelectEntered(SelectEnterEventArgs args) => Refresh(true);

        private void HandleSelectExited(SelectExitEventArgs args) => Refresh(interactable.isHovered);

        private void Refresh(bool visible)
        {
            if (visible)
            {
                if (titleText != null)
                {
                    titleText.text = info != null ? info.InfoTitle : fallbackTitle;
                }

                if (bodyText != null)
                {
                    bodyText.text = info != null ? info.BuildInfoBody() : fallbackBody;
                }
            }

            SetVisible(visible);
        }

        private void SetVisible(bool visible)
        {
            if (card != null)
            {
                card.SetActive(visible);
            }
        }
    }
}
