using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab4
{
    /// <summary>
    /// Змінна деталь вузла заміру тиску: прокладка, манометр, фільтр-патрон або кран.
    /// Береться рукою жестом щипка (XRGrabInteractable + Pinch Grab на руці),
    /// а <see cref="PartId"/> вирішує, у який монтажний паз деталь має стати.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class AssemblyPart : MonoBehaviour
    {
        [SerializeField] private string partId = "part";
        [SerializeField] private string displayName = "Деталь";
        [SerializeField, TextArea] private string specification = "—";

        private Vector3 homePosition;
        private Quaternion homeRotation;

        public string PartId => partId;

        public string DisplayName => displayName;

        public string Specification => specification;

        /// <summary>Деталь уже зафіксована в пазу — тоді лоток її назад не вимагає.</summary>
        public bool IsInstalled { get; set; }

        private void Awake()
        {
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }

        /// <summary>
        /// Повернення деталі на лоток. Швидкості скидаються явно, інакше
        /// Rigidbody продовжує рух уже на новій позиції й деталь злітає з лотка.
        /// </summary>
        public void ReturnToTray()
        {
            IsInstalled = false;

            if (TryGetComponent(out Rigidbody body))
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            transform.SetPositionAndRotation(homePosition, homeRotation);
        }
    }
}
