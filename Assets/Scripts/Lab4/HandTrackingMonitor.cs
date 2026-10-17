using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

namespace Lab4
{
    /// <summary>
    /// Обробка позицій пальців відстежуваних рук: бере <see cref="XRHandSubsystem"/>,
    /// рахує відстань «великий палець — вказівний» для кожної руки, визначає жест щипка
    /// з гістерезисом і виводить стан відстеження на табло стенда.
    /// Жест щипка тут лише вимірюється й показується — захоплення деталей виконує
    /// сам XRI-рig (XR Origin Hands) без жодної кнопки контролера.
    /// </summary>
    public class HandTrackingMonitor : MonoBehaviour
    {
        /// <summary>UnityEvent&lt;T&gt; серіалізується лише через власний підклас.</summary>
        [System.Serializable]
        public class PinchEvent : UnityEvent<Handedness>
        {
        }

        private const float PinchEnterDistance = 0.02f;
        private const float PinchExitDistance = 0.035f;

        private static readonly List<XRHandSubsystem> SubsystemBuffer = new List<XRHandSubsystem>();

        [SerializeField] private Text statusText;
        [SerializeField] private PinchEvent pinchStarted = new PinchEvent();

        private XRHandSubsystem subsystem;
        private bool leftPinching;
        private bool rightPinching;

        /// <summary>Кількість зафіксованих щипків за сеанс — показник того, що жест розпізнається.</summary>
        public int PinchCount { get; private set; }

        /// <summary>Подія початку щипка із зазначенням руки.</summary>
        public PinchEvent PinchStarted => pinchStarted;

        private void Update()
        {
            if (subsystem == null || !subsystem.running)
            {
                AcquireSubsystem();
            }

            if (statusText == null)
            {
                return;
            }

            statusText.text = BuildReport();
        }

        /// <summary>
        /// Підсистема рук з'являється лише після старту XR-провайдера (OpenXR Hand Tracking
        /// Subsystem або симулятор у режимі рук), тому шукаємо її щокадру, доки не знайдемо.
        /// </summary>
        private void AcquireSubsystem()
        {
            SubsystemManager.GetSubsystems(SubsystemBuffer);
            subsystem = SubsystemBuffer.Count > 0 ? SubsystemBuffer[0] : null;
        }

        private string BuildReport()
        {
            StringBuilder builder = new StringBuilder("Відстеження рук (Hand Tracking)\n");

            if (subsystem == null)
            {
                builder.AppendLine("Підсистема рук не запущена.");
                builder.AppendLine("У симуляторі: [ або ] двічі — режим рук.");
                return builder.ToString();
            }

            builder.AppendLine(subsystem.running ? "Підсистема рук: активна" : "Підсистема рук: зупинена");
            AppendHand(builder, "Ліва", subsystem.leftHand, ref leftPinching);
            AppendHand(builder, "Права", subsystem.rightHand, ref rightPinching);
            builder.AppendLine($"Жестів щипка за сеанс: {PinchCount}");

            return builder.ToString();
        }

        private void AppendHand(StringBuilder builder, string title, XRHand hand, ref bool wasPinching)
        {
            if (!hand.isTracked)
            {
                builder.AppendLine($"{title} рука: не відстежується");
                wasPinching = false;
                return;
            }

            int trackedJoints = CountTrackedJoints(hand);

            if (!TryGetPinchDistance(hand, out float distance))
            {
                builder.AppendLine($"{title} рука: суглобів {trackedJoints}, пози пальців недоступні");
                return;
            }

            bool isPinching = wasPinching
                ? distance < PinchExitDistance
                : distance < PinchEnterDistance;

            if (isPinching && !wasPinching)
            {
                PinchCount++;
                pinchStarted.Invoke(hand.handedness);
            }

            wasPinching = isPinching;

            string gesture = isPinching ? "щипок" : "відкрита";
            builder.AppendLine($"{title} рука: суглобів {trackedJoints}, зазор {distance * 1000f:0} мм, {gesture}");
        }

        /// <summary>Відстань між кінчиками великого та вказівного пальців — основа жесту щипка.</summary>
        private static bool TryGetPinchDistance(XRHand hand, out float distance)
        {
            distance = 0f;

            if (!hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumb) ||
                !hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose index))
            {
                return false;
            }

            distance = Vector3.Distance(thumb.position, index.position);
            return true;
        }

        /// <summary>Скільки суглобів руки має валідну позу — показник якості відстеження.</summary>
        private static int CountTrackedJoints(XRHand hand)
        {
            int tracked = 0;

            for (int index = XRHandJointID.BeginMarker.ToIndex(); index < XRHandJointID.EndMarker.ToIndex(); index++)
            {
                XRHandJoint joint = hand.GetJoint(XRHandJointIDUtility.FromIndex(index));
                if (joint.TryGetPose(out _))
                {
                    tracked++;
                }
            }

            return tracked;
        }
    }
}
