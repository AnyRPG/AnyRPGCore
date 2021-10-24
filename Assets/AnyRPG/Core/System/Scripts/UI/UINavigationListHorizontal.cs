using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationListHorizontal : UINavigationController {

        /*
        [SerializeField]
        private List<NavigableElement> navigableButtons = new List<NavigableElement>();

        private List<NavigableElement> activeNavigableButtons = new List<NavigableElement>();
        */


        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        private int currentIndex = -1;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            UpdateNavigationList();
        }

        public override void UpdateNavigationList() {
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public override void FocusFirstButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.FocusFirstButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex = 0;
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.FocusCurrentButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void LeftButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex--;
            if (currentIndex < 0) {
                if (leftController != null) {
                    currentIndex = 0;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveLeft();
                    return;
                } else {
                    currentIndex = activeNavigableButtons.Count - 1;
                }
            }
            currentNavigableElement.LeaveElement();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void RightButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex++;
            if (currentIndex >= activeNavigableButtons.Count) {
                if (rightController != null || rightPanel != null) {
                    currentIndex = activeNavigableButtons.Count - 1;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    LeaveRight();
                    return;
                } else {
                    currentIndex = 0;
                }
            }
            currentNavigableElement.LeaveElement();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void UpButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.UpButton()");
            LeaveUp();
        }

        public override void DownButton() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.DownButton()");
            LeaveDown();
        }

        public override void Accept() {
            Debug.Log(gameObject.name + ".UINavigationListHorizontal.Accept()");
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

