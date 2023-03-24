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

        protected int currentNumRows = 0;
        protected int currentNumColumns = 0;
        protected int currentRow = 0;
        protected int currentColumn = 0;

        public int NumRows { get => currentNumRows; }
        public int NumColumns { get => currentNumColumns; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currentNumRows = numRows;
            currentNumColumns = numColumns;
        }


        public override void FocusCurrentButton() {
            //Debug.Log($"{gameObject.name}.UINavigationGrid.FocusCurrentButton()");
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
            currentRow = Mathf.FloorToInt(currentIndex / currentNumColumns);
            currentColumn = currentIndex - (currentRow * currentNumColumns);
        }

        private void CalculateCurrentIndex() {
            currentIndex = (currentRow * currentNumColumns) + currentColumn;
        }

        public override void ProcessLeftButton() {
            //Debug.Log($"{gameObject.name}.UINavigationGrid.LeftButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (reverseLeftRight == false) {
                LessColumn();
            } else {
                MoreColumn();
            }
        }

        private void LessColumn() {
            currentColumn--;
            if (currentColumn < 0) {
                currentColumn++;
                if (LeaveLeft()) {
                    return;
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
            //Debug.Log($"{gameObject.name}.UINavigationGrid.RightButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (reverseLeftRight == false) {
                MoreColumn();
            } else {
                LessColumn();
            }
        }

        private void MoreColumn() {
            currentColumn++;
            if (currentColumn >= currentNumColumns) {
                currentColumn--;
                if (LeaveRight()) {
                    return;
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
            //Debug.Log($"{gameObject.name}.UINavigationGrid.UpButton()");
            currentRow--;
            if (currentRow < 0) {
                currentRow++;
                if (LeaveUp()) {
                    return;
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
            //Debug.Log($"{gameObject.name}.UINavigationGrid.DownButton()");
            currentRow++;
            if (currentRow >= currentNumRows) {
                currentRow--;
                if (LeaveDown()) {
                    return;
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
            //Debug.Log($"{gameObject.name}.UINavigationGrid.FocusFirstButton()");
            base.FocusFirstButton();
            CalculatePosition();
        }

        public override void SelectCurrentNavigableElement() {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.SelectCurrentNavigableElement()");
            base.SelectCurrentNavigableElement();
            if (scrollRect != null) {
                scrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(scrollRect, currentNavigableElement.RectTransform);
            }
        }

        public void ReCalculateSize() {
            if (numRows == 0) {
                CalculateNumRows();
            }
        }

        public override void AddActiveButton(NavigableElement navigableElement) {
            base.AddActiveButton(navigableElement);
            ReCalculateSize();
        }

        public override void ClearActiveButton(NavigableElement clearButton) {
            base.ClearActiveButton(clearButton);
            ReCalculateSize();
        }

        public override void ClearActiveButtons() {
            base.ClearActiveButtons();
            ReCalculateSize();
        }

        public void CalculateNumRows() {
            currentNumRows = Mathf.CeilToInt((float)(ActiveNavigableButtonCount) / (float)numColumns);
        }

    }

}

