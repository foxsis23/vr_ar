using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Lab2
{
    /// <summary>
    /// Візуальний індикатор зони точного розміщення (Socket Interactor / Snap Zone):
    /// підсвічує майданчик на столі під час наведення та фіксації предмета.
    /// </summary>
    [RequireComponent(typeof(XRSocketInteractor))]
    public class SnapZone : MonoBehaviour
    {
        [SerializeField] private Renderer zoneRenderer;
        [SerializeField] private Color idleColor = new Color(0.25f, 0.55f, 0.95f, 0.35f);
        [SerializeField] private Color hoverColor = new Color(0.95f, 0.80f, 0.20f, 0.55f);
        [SerializeField] private Color filledColor = new Color(0.25f, 0.90f, 0.45f, 0.65f);

        private XRSocketInteractor socket;
        private VrHud hud;

        private void Awake()
        {
            socket = GetComponent<XRSocketInteractor>();

            if (zoneRenderer == null)
            {
                zoneRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void OnEnable()
        {
            socket.hoverEntered.AddListener(HandleHoverEntered);
            socket.hoverExited.AddListener(HandleHoverExited);
            socket.selectEntered.AddListener(HandleSelectEntered);
            socket.selectExited.AddListener(HandleSelectExited);
            Paint(idleColor);
        }

        private void OnDisable()
        {
            socket.hoverEntered.RemoveListener(HandleHoverEntered);
            socket.hoverExited.RemoveListener(HandleHoverExited);
            socket.selectEntered.RemoveListener(HandleSelectEntered);
            socket.selectExited.RemoveListener(HandleSelectExited);
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            if (!socket.hasSelection)
            {
                Paint(hoverColor);
            }
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            Paint(socket.hasSelection ? filledColor : idleColor);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            Paint(filledColor);
            Hud()?.ReportSocket(args.interactableObject.transform.name, true);
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            Paint(idleColor);
            Hud()?.ReportSocket(args.interactableObject.transform.name, false);
        }

        private void Paint(Color color)
        {
            if (zoneRenderer == null)
            {
                return;
            }

            Material material = Application.isPlaying ? zoneRenderer.material : zoneRenderer.sharedMaterial;
            if (material != null)
            {
                material.color = color;
            }
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
