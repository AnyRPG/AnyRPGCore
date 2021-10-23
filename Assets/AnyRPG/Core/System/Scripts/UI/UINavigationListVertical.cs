using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListVertical : UINavigationController {

        /*
        [SerializeField]
        private List<NavigableElement> navigableButtons = new List<NavigableElement>();

        private List<NavigableElement> activeNavigableButtons = new List<NavigableElement>();
        */

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        private int currentIndex = -1;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public override void UpdateNavigationList() {
            activeNavigableButtons.Clear();
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public override void FocusFirstButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.FocusFirstButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex = 0;
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.FocusCurrentButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void UpButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.UpButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex > 0) {
                currentNavigableElement.LeaveElement();
            }
            currentIndex--;
            if (currentIndex < 0) {
                if (upController != null) {
                    currentIndex = 0;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    currentNavigableElement.LeaveElement();
                    owner.SetNavigationController(upController);
                    return;
                } else {
                    currentIndex = activeNavigableButtons.Count - 1;
                }
            }
            //EventSystem.current.SetSelectedGameObject(activeNavigableButtons[currentIndex].gameObject);
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void DownButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.DownButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < activeNavigableButtons.Count - 1) {
                currentNavigableElement.LeaveElement();
            }
            currentIndex++;
            Debug.Log("currentIndex: " + currentIndex + "; downController: " + (downController == null ? "null" : downController.gameObject.name) + "; buttonCount: " + activeNavigableButtons.Count);
            if (currentIndex >= activeNavigableButtons.Count) {
                if (downController != null) {
                    currentIndex = activeNavigableButtons.Count - 1;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    currentNavigableElement.LeaveElement();
                    owner.SetNavigationController(downController);
                    return;
                } else {
                    currentIndex = 0;
                }
            }
            //EventSystem.current.SetSelectedGameObject(activeNavigableButtons[currentIndex].gameObject);
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void LeftButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.LeftButton()");
            if (leftController != null) {
                currentNavigableElement.LeaveElement();
                owner.SetNavigationController(leftController);
            }
        }

        public override void RightButton() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.RightButton()");
            if (rightController != null) {
                currentNavigableElement.LeaveElement();
                owner.SetNavigationController(rightController);
            }
        }

        public override void Accept() {
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Accept();
        }

    }

}

