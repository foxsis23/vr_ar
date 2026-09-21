using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Lab2
{
    /// <summary>
    /// Пише в HUD факт телепортації через Teleportation Area або Teleportation Anchor.
    /// </summary>
    [RequireComponent(typeof(BaseTeleportationInteractable))]
    public class TeleportReporter : MonoBehaviour
    {
        [SerializeField] private string destinationName;

        private BaseTeleportationInteractable teleportInteractable;
        private VrHud hud;

        private void Awake()
        {
            teleportInteractable = GetComponent<BaseTeleportationInteractable>();

            if (string.IsNullOrWhiteSpace(destinationName))
            {
                destinationName = gameObject.name;
            }
        }

        private void OnEnable()
        {
            teleportInteractable.teleporting.AddListener(HandleTeleporting);
        }

        private void OnDisable()
        {
            teleportInteractable.teleporting.RemoveListener(HandleTeleporting);
        }

        private void HandleTeleporting(TeleportingEventArgs args)
        {
            if (hud == null)
            {
                hud = FindFirstObjectByType<VrHud>();
            }

            hud?.ReportTeleport(destinationName);
        }
    }
}
