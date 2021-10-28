using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListVertical : UINavigationController {

        public override void FocusCurrentButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.FocusCurrentButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0 || currentIndex >= activeNavigableButtons.Count) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void UpButton() {
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

        public override void DownButton() {
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

        public override void LeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.LeftButton()");
            LeaveLeft();
        }

        public override void RightButton() {
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

        public Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, RectTransform child) {
            //Debug.Log(gameObject.name + ".UINavigationListVertical.GetSnapToPositionToBringChildIntoView()");
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(
                //0 - (viewportLocalPosition.x + childLocalPosition.x),
                instance.content.localPosition.x,
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            return result;
        }

    }

}

