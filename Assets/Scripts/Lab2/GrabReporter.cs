using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Lab2
{
    /// <summary>
    /// Відстежує взаємодію з предметом, який можна взяти віртуальною рукою:
    /// захоплення, утримання, відпускання та кидок (швидкість Rigidbody у момент відпускання).
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabReporter : MonoBehaviour
    {
        [SerializeField] private string displayName;

        private XRGrabInteractable grabInteractable;
        private Rigidbody body;
        private VrHud hud;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = gameObject.name;
            }
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(HandleSelectEntered);
            grabInteractable.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
            grabInteractable.selectExited.RemoveListener(HandleSelectExited);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            {
                return;
            }

            Hud()?.ReportGrab(displayName);
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            {
                return;
            }

            float speed = body != null ? body.linearVelocity.magnitude : 0f;
            Hud()?.ReportRelease(displayName, speed);
        }

        private VrHud Hud()
        {
            if (hud == null)
            {
                hud = FindFirstObjectByType<VrHud>();
            }

            return hud;
        }
    }
}
