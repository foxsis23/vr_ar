using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace Lab3
{
    /// <summary>
    /// Блокує керування, поки користувач друкує в полі введення форми.
    /// У XR Interaction Simulator кожна латинська літера — це окрема дія (W/A/S/D — рух,
    /// M — меню планшета, T — Trigger, G — Grip), тому під час набору тексту симулятор
    /// вимикається цілком, а не лише його переміщення. У шоломі так само зупиняються
    /// провайдери локомоції, щоб стік не ніс користувача під час заповнення звіту.
    /// Вийти з поля: Enter або клік по вільному місцю панелі.
    /// </summary>
    public class FormInputLock : MonoBehaviour
    {
        [SerializeField] private SpatialTablet tablet;
        [SerializeField] private Text hintText;
        [SerializeField, TextArea] private string idleHint =
            "Поле введення: клацніть по ньому променем і друкуйте з клавіатури.";
        [SerializeField, TextArea] private string lockedHint =
            "Режим введення тексту: керування вимкнено. Enter — завершити введення.";

        private readonly List<LocomotionProvider> suspendedProviders = new List<LocomotionProvider>();

        private XRInteractionSimulator simulator;
        private bool simulatorSuspended;
        private bool locked;

        public bool IsLocked => locked;

        private void Start()
        {
            ShowHint(false);
        }

        private void Update()
        {
            bool typing = IsEditingInputField();
            if (typing == locked)
            {
                return;
            }

            locked = typing;

            if (typing)
            {
                Lock();
            }
            else
            {
                Unlock();
            }

            ShowHint(typing);
        }

        private void OnDisable()
        {
            if (locked)
            {
                locked = false;
                Unlock();
            }
        }

        private static bool IsEditingInputField()
        {
            EventSystem events = EventSystem.current;
            GameObject selected = events != null ? events.currentSelectedGameObject : null;
            if (selected == null)
            {
                return false;
            }

            InputField field = selected.GetComponent<InputField>();
            return field != null && field.isFocused;
        }

        private void Lock()
        {
            if (tablet != null)
            {
                // Інакше літера «m» у тексті закрила б сам планшет.
                tablet.SetMenuShortcutEnabled(false);
            }

            XRInteractionSimulator activeSimulator = FindSimulator();
            if (activeSimulator != null && activeSimulator.enabled)
            {
                activeSimulator.enabled = false;
                simulatorSuspended = true;
            }

            suspendedProviders.Clear();
            LocomotionProvider[] providers = FindObjectsByType<LocomotionProvider>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (LocomotionProvider provider in providers)
            {
                if (provider == null || !provider.enabled)
                {
                    continue;
                }

                provider.enabled = false;
                suspendedProviders.Add(provider);
            }
        }

        private void Unlock()
        {
            foreach (LocomotionProvider provider in suspendedProviders)
            {
                if (provider != null)
                {
                    provider.enabled = true;
                }
            }

            suspendedProviders.Clear();

            if (simulatorSuspended)
            {
                simulatorSuspended = false;
                XRInteractionSimulator activeSimulator = FindSimulator();
                if (activeSimulator != null)
                {
                    activeSimulator.enabled = true;
                }
            }

            if (tablet != null)
            {
                tablet.SetMenuShortcutEnabled(true);
            }
        }

        private XRInteractionSimulator FindSimulator()
        {
            if (simulator == null)
            {
                simulator = FindFirstObjectByType<XRInteractionSimulator>(FindObjectsInactive.Include);
            }

            return simulator;
        }

        private void ShowHint(bool isLocked)
        {
            if (hintText == null)
            {
                return;
            }

            hintText.text = isLocked ? lockedHint : idleHint;
            hintText.color = isLocked ? new Color(0.95f, 0.75f, 0.30f) : new Color(0.62f, 0.68f, 0.76f);
        }
    }
}
