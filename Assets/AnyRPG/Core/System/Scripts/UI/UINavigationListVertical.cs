using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListVertical : UINavigationController {

        public override void FocusCurrentButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.FocusCurrentButton()");
            base.FocusCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            SetCurrentButton();
            SelectCurrentNavigableElement();
        }

        public override void HighlightCurrentButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.FocusCurrentButton()");
            base.FocusCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            SetCurrentButton();
            HighlightCurrentNavigableElement();
        }


        public override void ProcessUpButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.ProcessUpButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }

            // already at top
            if (currentIndex == 0) {
                if (LeaveUp()) {
                    return;
                }
            }

            // not at top
            if (currentIndex > 0) {
                currentIndex--;
                if (currentNavigableElement != null) {
                    currentNavigableElement.LeaveElement();
                }
                currentNavigableElement = activeNavigableButtons[currentIndex];
                SelectCurrentNavigableElement();
            }
        }

        public override void ProcessDownButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.ProcessDownButton()");

            if (activeNavigableButtons.Count == 0) {
                return;
            }

            // already at bottom
            if (currentIndex == (activeNavigableButtons.Count - 1)) {
                if (LeaveDown()) {
                    return;
                }
            }

            // not at bottom
            if (currentIndex < (activeNavigableButtons.Count - 1)) {
                currentIndex++;
                if (currentNavigableElement != null) {
                    currentNavigableElement.LeaveElement();
                }
                currentNavigableElement = activeNavigableButtons[currentIndex];
                SelectCurrentNavigableElement();
            }
        }

        public override void ProcessLeftButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.LeftButton()");
            LeaveLeft();
        }

        public override void ProcessRightButton() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.RightButton()");
            LeaveRight();
        }


        public override void SelectCurrentNavigableElement() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.SelectCurrentNavigableElement()");
            base.SelectCurrentNavigableElement();
            if (scrollRect != null) {
                scrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(scrollRect, currentNavigableElement.RectTransform);
            }
        }

       

    }

}

