﻿using System.Collections.Generic;
using UnityEngine;

namespace EVRC
{
    using System;
    using ButtonPress = ActionsController.ButtonPress;
    using Hand = TrackedHand.Hand;

    public class ControllerInteractionPoint : MonoBehaviour
    {
        public TooltipDisplay tooltipDisplay;

        private TrackedHand trackedHand;
        private HashSet<IGrabable> intersectingGrababales = new HashSet<IGrabable>();
        private HashSet<BaseButton> intersectingButtons = new HashSet<BaseButton>();
        private HashSet<IGrabable> grabbing = new HashSet<IGrabable>();
        private ITooltip tooltip;

        public Hand Hand
        {
            get
            {
                return trackedHand.hand;
            }
        }

        void Start()
        {
            trackedHand = GetComponentInParent<TrackedHand>();
        }

        void OnEnable()
        {
            ActionsController.TriggerPress.Listen(OnTriggerPress);
            ActionsController.TriggerUnpress.Listen(OnTriggerUnpress);
            ActionsController.GrabPress.Listen(OnGrabPress);
            ActionsController.GrabUnpress.Listen(OnGrabUnpress);
            Tooltip.TooltipUpdated.Listen(OnTooltipUpdate);
        }

        void OnDisable()
        {
            ActionsController.TriggerPress.Remove(OnTriggerPress);
            ActionsController.TriggerUnpress.Remove(OnTriggerUnpress);
            ActionsController.GrabPress.Remove(OnGrabPress);
            ActionsController.GrabUnpress.Remove(OnGrabPress);
            Tooltip.TooltipUpdated.Remove(OnTooltipUpdate);
        }

        private void OnTooltipUpdate(ITooltip tooltip, string text)
        {
            if (this.tooltip == tooltip && tooltipDisplay.text != text)
            {
                tooltipDisplay.text = text;
                tooltipDisplay.Refresh();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var grabables = other.GetComponents<IGrabable>();
            foreach (var grabable in grabables)
            {
                intersectingGrababales.Add(grabable);
            }

            var button = other.GetComponent<BaseButton>();
            if (button != null)
            {
                intersectingButtons.Add(button);
            }

            var tt = other.GetComponent<ITooltip>();
            if (tt != null)
            {
                tooltip = tt;
                if (tooltipDisplay)
                {
                    tooltipDisplay.text = tooltip.GetTooltipText();
                    tooltipDisplay.enabled = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var grabables = other.GetComponents<IGrabable>();
            foreach (var grabable in grabables)
            {
                intersectingGrababales.Remove(grabable);
            }

            var button = other.GetComponent<BaseButton>();
            if (button != null)
            {
                intersectingButtons.Remove(button);
            }

            var tt = other.GetComponent<ITooltip>();
            if (tt == tooltip)
            {
                tooltip = null;
                tooltipDisplay.enabled = false;
            }
        }

        private bool IsSameHand(Hand tHand, ActionsController.Hand bHand)
        {
            if (tHand == Hand.Left)
            {
                return bHand == ActionsController.Hand.Left;
            }
            else if (tHand == Hand.Right)
            {
                return bHand == ActionsController.Hand.Right;
            }
            return false;
        }

        private void OnTriggerPress(ButtonPress btn)
        {
            if (!IsSameHand(trackedHand.hand, btn.hand)) return;

            foreach (BaseButton button in intersectingButtons)
            {
                button.Activate();
            }
        }

        private void OnTriggerUnpress(ButtonPress btn)
        {
        }

        private void OnGrabPress(ButtonPress btn)
        {
            if (!IsSameHand(trackedHand.hand, btn.hand)) return;

            foreach (IGrabable surface in intersectingGrababales)
            {
                if (surface.Grabbed(this))
                {
                    grabbing.Add(surface);
                }
            }
        }

        private void OnGrabUnpress(ButtonPress btn)
        {
            if (!IsSameHand(trackedHand.hand, btn.hand)) return;

            foreach (IGrabable surface in grabbing)
            {
                surface.Ungrabbed(this);
            }

            grabbing.Clear();
        }
    }
}