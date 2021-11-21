using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableSlider : HighlightButton {

        [Header("Slider")]

        [SerializeField]
        Slider slider = null;

        protected bool interacting = false;


        public override bool CaptureCancelButton {
            get {
                if (interacting == true) {
                    return true;
                }
                return base.CaptureCancelButton;
            }
        }

        public override bool CaptureDPad {
            get {
                if (interacting == true) {
                    return true;
                }
                return base.CaptureDPad;
            }
        }

        /*
        public override void Interact() {
            base.Interact();
            interacting = true;
            //inputField.ActivateInputField();
            dropDown.Show();
        }
        */

        public override void LeftButton() {
            base.LeftButton();
            if (slider.value > 0f) {
                slider.value -= 0.05f;
                //slider.OnDrag(new PointerEventData(EventSystem.current));
                //new PointerEventData(EventSystem.current);
            }
        }

        public override void RightButton() {
            base.RightButton();
            if (slider.value < 1f) {
                slider.value += 0.05f;
                //slider.OnDrag(new PointerEventData(EventSystem.current));
            }
        }

        public override void Cancel() {
            base.Cancel();
            interacting = false;
            UnHighlightBackground();
        }

        public override void Accept() {
            base.Accept();
            if (!interacting) {
                interacting = true;
                HighlightBackground();
            } else {
                interacting = false;
                UnHighlightBackground();
            }
        }

    }

}