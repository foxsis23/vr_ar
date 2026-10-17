using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab4
{
    /// <summary>
    /// Монтажний паз вузла заміру тиску. Приймає лише «свою» деталь: перевірка йде
    /// фільтром вибору (<see cref="IXRSelectFilter"/>), тому чужа деталь просто не
    /// зафіксується в пазу. Наведення деталі підсвічує паз — зелений для правильної
    /// деталі, червоний для чужої, бурштиновий, поки паз порожній.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class AssemblySocket : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
    {
        private static readonly Color EmptyColor = new Color(0.95f, 0.65f, 0.15f);
        private static readonly Color MatchColor = new Color(0.25f, 0.80f, 0.35f);
        private static readonly Color MismatchColor = new Color(0.90f, 0.25f, 0.20f);
        private static readonly Color FilledColor = new Color(0.20f, 0.65f, 0.30f);

        [SerializeField] private string expectedPartId = "part";
        [SerializeField] private string slotName = "Паз";
        [SerializeField] private Renderer indicator;
        [SerializeField] private Text label;

        [SerializeField] private UnityEvent stateChanged = new UnityEvent();

        private XRSocketInteractor socket;
        private MaterialPropertyBlock propertyBlock;

        /// <summary>Назва пазу для списку прогресу складання.</summary>
        public string SlotName => slotName;

        /// <summary>Паз заповнено правильною деталлю.</summary>
        public bool IsFilled => InstalledPart != null;

        /// <summary>Деталь, зафіксована в пазу, або <c>null</c>.</summary>
        public AssemblyPart InstalledPart { get; private set; }

        /// <summary>Подія для <see cref="AssemblyTask"/>: склад вузла змінився.</summary>
        public UnityEvent StateChanged => stateChanged;

        bool IXRSelectFilter.canProcess => isActiveAndEnabled;

        bool IXRHoverFilter.canProcess => isActiveAndEnabled;

        private void Awake()
        {
            socket = GetComponent<XRSocketInteractor>();
            propertyBlock = new MaterialPropertyBlock();
            RefreshVisual(null);
        }

        private void OnEnable()
        {
            socket.selectFilters.Add(this);
            socket.hoverFilters.Add(this);

            socket.hoverEntered.AddListener(HandleHoverEntered);
            socket.hoverExited.AddListener(HandleHoverExited);
            socket.selectEntered.AddListener(HandleSelectEntered);
            socket.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            socket.selectFilters.Remove(this);
            socket.hoverFilters.Remove(this);

            socket.hoverEntered.RemoveListener(HandleHoverEntered);
            socket.hoverExited.RemoveListener(HandleHoverExited);
            socket.selectEntered.RemoveListener(HandleSelectEntered);
            socket.selectExited.RemoveListener(HandleSelectExited);
        }

        /// <summary>Наведення дозволене будь-якій деталі — саме так видно підказку «не та деталь».</summary>
        bool IXRHoverFilter.Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return FindPart(interactable as Component) != null;
        }

        /// <summary>Фіксація дозволена лише деталі з очікуваним ідентифікатором.</summary>
        bool IXRSelectFilter.Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return Matches(FindPart(interactable as Component));
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            if (IsFilled)
            {
                return;
            }

            RefreshVisual(FindPart(args.interactableObject as Component));
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            if (IsFilled)
            {
                return;
            }

            RefreshVisual(null);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            AssemblyPart part = FindPart(args.interactableObject as Component);
            if (!Matches(part))
            {
                return;
            }

            InstalledPart = part;
            part.IsInstalled = true;
            RefreshVisual(part);
            stateChanged.Invoke();
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            AssemblyPart part = FindPart(args.interactableObject as Component);
            if (part == null || part != InstalledPart)
            {
                return;
            }

            part.IsInstalled = false;
            InstalledPart = null;
            RefreshVisual(null);
            stateChanged.Invoke();
        }

        private bool Matches(AssemblyPart part)
        {
            return part != null && part.PartId == expectedPartId;
        }

        private static AssemblyPart FindPart(Component interactable)
        {
            return interactable == null ? null : interactable.GetComponentInParent<AssemblyPart>();
        }

        /// <summary>
        /// Колір кільця пазу й текст підказки. Колір пишеться через
        /// <see cref="MaterialPropertyBlock"/>, щоб не плодити матеріали на кожен паз.
        /// </summary>
        private void RefreshVisual(AssemblyPart candidate)
        {
            Color color;
            string hint;

            if (IsFilled)
            {
                color = FilledColor;
                hint = $"{slotName}\n{InstalledPart.DisplayName} — зафіксовано";
            }
            else if (candidate == null)
            {
                color = EmptyColor;
                hint = $"{slotName}\nочікується деталь";
            }
            else if (Matches(candidate))
            {
                color = MatchColor;
                hint = $"{slotName}\n{candidate.DisplayName} — відпустіть щипок";
            }
            else
            {
                color = MismatchColor;
                hint = $"{slotName}\n{candidate.DisplayName} сюди не монтується";
            }

            if (label != null)
            {
                label.text = hint;
            }

            if (indicator == null)
            {
                return;
            }

            indicator.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            propertyBlock.SetColor("_EmissionColor", color * 0.8f);
            indicator.SetPropertyBlock(propertyBlock);
        }
    }
}
