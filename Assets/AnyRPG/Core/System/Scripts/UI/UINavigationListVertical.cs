using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListVertical : UINavigationController {

        public override void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.FocusCurrentButton()");
            base.FocusCurrentButton();
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0 || currentIndex >= activeNavigableButtons.Count) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessUpButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.UpButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex--;
            if (currentIndex < 0) {
                if (upControllers.Count != 0 || upPanel != null) {
                    currentIndex = 0;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveUp();
                    return;
                } else {
                    currentIndex = activeNavigableButtons.Count - 1;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessDownButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.DownButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex++;
            if (currentIndex >= activeNavigableButtons.Count) {
                if (downControllers.Count != 0 || downPanel != null) {
                    currentIndex = activeNavigableButtons.Count - 1;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveDown();
                    return;
                } else {
                    currentIndex = 0;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessLeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.LeftButton()");
            LeaveLeft();
        }

        public override void ProcessRightButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.RightButton()");
            LeaveRight();
        }


        public override void SelectCurrentNavigableElement() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.SelectCurrentNavigableElement()");
            base.SelectCurrentNavigableElement();
            if (scrollRect != null) {
                scrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(scrollRect, currentNavigableElement.RectTransform);
            }
        }

       

    }

}

