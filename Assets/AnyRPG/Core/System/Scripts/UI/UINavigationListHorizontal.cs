using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListHorizontal : UINavigationController {

        public override void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.FocusCurrentButton()");
            base.FocusCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void ProcessLeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex--;
            if (currentIndex < 0) {
                if (leftControllers.Count != 0) {
                    currentIndex = 0;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveLeft();
                    return;
                } else {
                    currentIndex = activeNavigableButtons.Count - 1;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void ProcessRightButton() {
            //Debug.Log(gameObject.name + ".UINavigationListHorizontal.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex++;
            if (currentIndex >= activeNavigableButtons.Count) {
                if (rightControllers.Count != 0 || rightPanel != null) {
                    currentIndex = activeNavigableButtons.Count - 1;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveRight();
                    return;
                } else {
                    currentIndex = 0;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
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

