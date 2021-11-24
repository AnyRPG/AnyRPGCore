using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UINavigationGrid : UINavigationController {

        [SerializeField]
        protected int numRows = 0;

        [SerializeField]
        protected int numColumns = 0;

        [Tooltip("If true, column 1 is on the right side instead of the left")]
        [SerializeField]
        protected bool reverseLeftRight = false;

        protected int currentRow = 0;
        protected int currentColumn = 0;

        public int NumRows { get => numRows; set => numRows = value; }
        public int NumColumns { get => numColumns; set => numColumns = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }


        public override void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationGrid.FocusCurrentButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0 || currentIndex >= activeNavigableButtons.Count || activeNavigableButtons[currentIndex].gameObject.activeInHierarchy == false) {
                currentIndex = 0;
                currentRow = 0;
                currentColumn = 0;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
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

        public override void ProcessLeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (!reverseLeftRight) {
                LessColumn();
            } else {
                MoreColumn();
            }
        }

        private void LessColumn() {
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
            if (activeNavigableButtons[currentIndex].gameObject.activeInHierarchy != true) {
                currentColumn++;
                CalculateCurrentIndex();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessRightButton() {
            //Debug.Log(gameObject.name + ".UINavigationGrid.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (!reverseLeftRight) {
                MoreColumn();
            } else {
                LessColumn();
            }
        }

        private void MoreColumn() {
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
            if (currentIndex >= activeNavigableButtons.Count || activeNavigableButtons[currentIndex].gameObject.activeInHierarchy != true) {
                currentColumn--;
                CalculateCurrentIndex();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessUpButton() {
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
            if (activeNavigableButtons[currentIndex].gameObject.activeInHierarchy != true) {
                currentRow++;
                CalculateCurrentIndex();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void ProcessDownButton() {
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
            if (currentIndex >= activeNavigableButtons.Count || activeNavigableButtons[currentIndex].gameObject.activeInHierarchy != true) {
                currentRow--;
                CalculateCurrentIndex();
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public override void FocusFirstButton() {
            Debug.Log(gameObject.name + ".UINavigationGrid.FocusFirstButton()");
            base.FocusFirstButton();
            CalculatePosition();
        }

        public override void SelectCurrentNavigableElement() {
            Debug.Log(gameObject.name + ".UINavigationListVertical.SelectCurrentNavigableElement()");
            base.SelectCurrentNavigableElement();
            if (scrollRect != null) {
                scrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(scrollRect, currentNavigableElement.RectTransform);
            }
        }

    }

}

