using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationGrid : UINavigationController {

        [SerializeField]
        private int numRows = 0;

        [SerializeField]
        private int numColumns = 0;

        private int currentRow = 0;
        private int currentColumn = 0;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }


        public override void FocusCurrentButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.FocusCurrentButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
                currentRow = 0;
                currentColumn = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void SetCurrentIndex(int newIndex) {
            base.SetCurrentIndex(newIndex);
            CalculatePosition();
        }

        private void CalculatePosition() {
            currentRow = Mathf.FloorToInt(currentIndex / numColumns);
            currentColumn = currentIndex - (currentRow * numColumns);
        }

        private void CalculateCurrentIndex() {
            currentIndex = (currentRow * numColumns) + currentColumn;
        }

        public override void LeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentColumn--;
            if (currentColumn < 0) {
                if (leftControllers.Count != 0 || leftPanel != null) {
                    currentColumn++;
                    LeaveLeft();
                    return;
                } else {
                    currentColumn++;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            CalculateCurrentIndex();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void RightButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentColumn++;
            if (currentColumn >= numColumns) {
                if (rightControllers.Count != 0 || rightPanel != null) {
                    currentColumn--;
                    LeaveRight();
                    return;
                } else {
                    currentColumn--;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            CalculateCurrentIndex();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void UpButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.UpButton()");
            currentRow--;
            if (currentRow < 0) {
                if (upControllers.Count != 0 || upPanel != null) {
                    currentRow++;
                    LeaveUp();
                    return;
                } else {
                    currentRow++;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            CalculateCurrentIndex();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void DownButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.DownButton()");
            currentRow++;
            if (currentRow >= numRows) {
                if (downControllers.Count != 0 || downPanel != null) {
                    currentRow--;
                    LeaveDown();
                    return;
                } else {
                    currentRow--;
                }
            }
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            CalculateCurrentIndex();
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Select();
        }

        public override void FocusFirstButton() {
            Debug.Log(gameObject.name + ".UINavigationGrid.FocusFirstButton()");
            base.FocusFirstButton();
            CalculatePosition();
        }

    }

}

