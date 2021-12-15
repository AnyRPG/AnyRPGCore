using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListHorizontal : UINavigationController {

        public override void FocusCurrentButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.FocusCurrentButton()");
            base.FocusCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            SetCurrentButton();
            SelectCurrentNavigableElement();
        }

        public override void HighlightCurrentButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.HighlightCurrentButton()");
            //base.FocusCurrentButton();
            base.HighlightCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            SetCurrentButton();
            HighlightCurrentNavigableElement();
        }

        public override void ProcessLeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }

            // already at far left
            if (currentIndex == 0) {
                if (leftControllers.Count != 0 || leftPanel != null) {
                    LeaveLeft();
                }
                return;
            }

            // not at far left
            if (currentIndex > 0) {
                currentIndex--;
                if (currentNavigableElement != null) {
                    currentNavigableElement.LeaveElement();
                }
                currentNavigableElement = activeNavigableButtons[currentIndex];
                SelectCurrentNavigableElement();
            }
        }

        public override void ProcessRightButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            // already at right
            if (currentIndex == (activeNavigableButtons.Count - 1)) {
                if (rightControllers.Count != 0 || rightPanel != null) {
                    LeaveRight();
                    return;
                }
            }

            // not at right
            if (currentIndex < (activeNavigableButtons.Count - 1)) {
                currentIndex++;
                if (currentNavigableElement != null) {
                    currentNavigableElement.LeaveElement();
                }
                currentNavigableElement = activeNavigableButtons[currentIndex];
                SelectCurrentNavigableElement();
            }
        }

        public override void ProcessUpButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.UpButton()");
            LeaveUp();
        }

        public override void ProcessDownButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.DownButton()");
            LeaveDown();
        }


    }

}

