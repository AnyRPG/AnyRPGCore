using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    [System.Serializable]
    public class UINavigationController : ConfiguredMonoBehaviour {

        [Header("DPad Movement")]

        /*
        [Tooltip("If true, attempt to select the button in the same vertical or horizontal position as the currently selected button when moving to a new controller")]
        [SerializeField]
        protected bool moveToSameIndex = false;
        */

        [Tooltip("If the left button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents leftPanel = null;

        [Tooltip("If the left button is passed, switch to the first active controller in this list")]
        [SerializeField]
        protected List<UINavigationController> leftControllers = new List<UINavigationController>();

        [Tooltip("If the right button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents rightPanel = null;

        [Tooltip("If the right button is passed, switch to the first active controller in this list")]
        [SerializeField]
        protected List<UINavigationController> rightControllers = new List<UINavigationController>();

        [Tooltip("If the top button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents upPanel = null;

        [Tooltip("If the top button is passed, switch to the first active controller in this list")]
        [SerializeField]
        protected List<UINavigationController> upControllers = new List<UINavigationController>();

        [Tooltip("If the bottom button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents downPanel = null;

        [Tooltip("If the bottom button is passed, switch to the first active controller in this list")]
        [SerializeField]
        protected List<UINavigationController> downControllers = new List<UINavigationController>();

        [Header("Button Presses")]

        [Tooltip("If the accept button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController acceptController = null;

        [Header("Elements")]

        [SerializeField]
        protected ScrollRect scrollRect = null;

        [Tooltip("Should the active element list be cleared and rebuilt when the window is opened")]
        [SerializeField]
        protected bool updateActiveListOnOpen = true;

        [SerializeField]
        protected bool pruneInactiveElements = true;


        [SerializeField]
        protected List<NavigableElement> navigableButtons = new List<NavigableElement>();

        protected List<NavigableElement> activeNavigableButtons = new List<NavigableElement>();

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        //private int currentIndex = -1;

        protected NavigableElement currentNavigableElement = null;

        protected CloseableWindowContents owner = null;

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        protected int currentIndex = -1;

        bool focused = false;

        // game manager references
        protected ControlsManager controlsManager = null;

        public virtual NavigableElement CurrentNavigableElement {
            get {
                return currentNavigableElement;
            }
        }

        public int CurrentIndex { get => currentIndex; }
        public List<NavigableElement> NavigableButtons { get => navigableButtons; }
        public int ActiveNavigableButtonCount { get => activeNavigableButtons.Count; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (NavigableElement navigableElement in navigableButtons) {
                navigableElement.Configure(systemGameManager);
                navigableElement.SetController(this);
                if (pruneInactiveElements == false || navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            controlsManager = systemGameManager.ControlsManager;
        }

        public void SetOwner(CloseableWindowContents closeableWindowContents) {
            //Debug.Log(gameObject.name + ".UINavigationController.SetOwner(" + closeableWindowContents.name + ")");
            owner = closeableWindowContents;
        }

        public virtual void SetCurrentIndex(int newIndex) {
            currentIndex = newIndex;
        }

        public virtual void SetCurrentButton(NavigableElement navigableElement) {
            //Debug.Log(gameObject.name + ".UINavigationController.SetCurrentButton(" + navigableElement.name + ")");
            for (int i = 0; i < activeNavigableButtons.Count; i++) {
                if (activeNavigableButtons[i] == navigableElement) {
                    SetCurrentIndex(i);
                    currentNavigableElement = navigableElement;
                    break;
                }
            }
        }

        public virtual void UnHightlightButtons(NavigableElement skipButton = null) {
            //Debug.Log(gameObject.name + ".UINavigationController.UnHightlightButtons(" + (skipButton == null ? "null" : skipButton.gameObject.name) + ")");
            foreach (NavigableElement navigableElement in activeNavigableButtons) {
                if (skipButton != navigableElement) {
                    navigableElement.UnHighlightBackground();
                }
            }
        }

        public virtual void UpdateNavigationList() {
            //Debug.Log(gameObject.name + ".UINavigationController.UpdateNavigationList()");

            // deselect the buttons before clearing the list
            foreach (NavigableElement navigableElement in activeNavigableButtons) {
                navigableElement.DeSelect();
                navigableElement.UnHighlightBackground();
            }
            activeNavigableButtons.Clear();
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (pruneInactiveElements == false || navigableElement.Available()) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public virtual void Activate() {
            //Debug.Log(gameObject.name + ".UINavigationController.Activate()");
            if (owner != null) {
                owner.ActivateNavigationController(this);
            }
        }

        public virtual void Focus(bool focusCurrentButton = true) {
            //Debug.Log(gameObject.name + ".UINavigationController.Focus()");
            focused = true;
            // testing - active navigable buttons is needed for lists that are dynamically created (skill buttons etc)
            // regular navigable buttons are needed for lists that are static, but may have temporarily disabled elements (music player)
            // union them both to ensure nothing is missed
            foreach (NavigableElement navigableElement in activeNavigableButtons.Union(navigableButtons)) {
                navigableElement.FocusNavigationController();
            }
            if (focusCurrentButton == true) {
                FocusCurrentButton();
            }
        }

        public virtual void UnFocus() {
            Debug.Log(gameObject.name + ".UINavigationController.Unfocus()");
            focused = false;
            foreach (NavigableElement navigableElement in activeNavigableButtons.Union(navigableButtons)) {
                navigableElement.UnFocus();
                //navigableElement.LeaveElement();
            }
        }

        public virtual void AddActiveButton(NavigableElement navigableElement) {
            activeNavigableButtons.Add(navigableElement);
            navigableElement.SetController(this);
            if (focused) {
                navigableElement.FocusNavigationController();
            }
        }

        public virtual void ClearActiveButtons() {
            activeNavigableButtons.Clear();
        }

        public virtual void ClearActiveButton(NavigableElement clearButton) {
            activeNavigableButtons.Remove(clearButton);
        }

        public virtual void FocusFirstButton() {
            Debug.Log(gameObject.name + ".UINavigationController.FocusFirstButton()");
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            currentIndex = 0;
            currentNavigableElement = activeNavigableButtons[currentIndex];
            SelectCurrentNavigableElement();
        }

        public virtual void SelectCurrentNavigableElement() {
            //Debug.Log(gameObject.name + ".UINavigationController.SelectCurrentNavigableElement()");
            currentNavigableElement.Select();
        }


        public virtual void FocusCurrentButton() {
            Debug.Log(gameObject.name + ".UINavigationController.FocusCurrentButton()");
        }

        public void UpButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.UpButton()");
            if (currentNavigableElement != null && currentNavigableElement.CaptureDPad == true) {
                currentNavigableElement.UpButton();
                return;
            }
            ProcessUpButton();
        }

        public virtual void ProcessUpButton() {
        }

        public virtual bool LeaveUp() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveUp()");
            if (upControllers.Count != 0) {
                LeaveController();
                foreach (UINavigationController uINavigationController in upControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.Activate();
                        return true;
                    }
                }
            }
            if (upPanel != null) {
                LeaveController();
                upPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public void DownButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.DownButton()");
            if (currentNavigableElement != null && currentNavigableElement.CaptureDPad == true) {
                currentNavigableElement.DownButton();
                return;
            }
            ProcessDownButton();
        }

        public virtual void ProcessDownButton() {

        }

        public virtual bool LeaveDown() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveDown()");
            if (downControllers.Count != 0) {
                LeaveController();
                foreach (UINavigationController uINavigationController in downControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.Activate();
                        return true;
                    }
                }
            }
            if (downPanel != null) {
                LeaveController();
                downPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public void LeftButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeftButton()");
            if (currentNavigableElement != null && currentNavigableElement.CaptureDPad == true) {
                currentNavigableElement.LeftButton();
                return;
            }
            ProcessLeftButton();
        }

        public virtual void ProcessLeftButton() {
        }

        public virtual bool LeaveLeft() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveLeft()");
            if (leftControllers.Count != 0) {
                LeaveController();
                foreach (UINavigationController uINavigationController in leftControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.Activate();
                        return true;
                    }
                }
            }
            if (leftPanel != null) {
                LeaveController();
                leftPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public void RightButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.RightButton()");
            if (currentNavigableElement != null && currentNavigableElement.CaptureDPad == true) {
                currentNavigableElement.RightButton();
                return;
            }
            ProcessRightButton();
        }

        public virtual void ProcessRightButton() {
        }

        public virtual bool LeaveRight() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveRight()");
            if (rightControllers.Count != 0) {
                LeaveController();
                foreach (UINavigationController uINavigationController in rightControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.Activate();
                        return true;
                    }
                }
            }
            if (rightPanel != null) {
                LeaveController();
                rightPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void LeaveController() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveController()");
            UnFocus();
            /*
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            */
        }

        public void LBButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.LBButton()");
        }

        public void RBButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.RBButton()");
        }


        public virtual void Accept() {
            Debug.Log(gameObject.name + ".UINavigationController.Accept()");
            if (activeNavigableButtons.Count != 0) {
                if (currentIndex < 0) {
                    currentIndex = 0;
                    currentNavigableElement = activeNavigableButtons[currentIndex];
                    currentNavigableElement.Select();
                    return;
                }
                currentNavigableElement = activeNavigableButtons[currentIndex];
                currentNavigableElement.Accept();
            }
            if (acceptController != null) {
                /*
                if (currentNavigableElement != null) {
                    currentNavigableElement.LeaveElement();
                }
                */
                UnFocus();
                acceptController.Activate();
                return;
            }
        }

        public virtual void Cancel() {
            Debug.Log(gameObject.name + ".UINavigationController.Cancel()");

            // should not automatically unfocus because this may be the only navigation controller on a window that is not closeable
            Debug.Log(gameObject.name + ".UINavigationController.Cancel(): parentpanel: " + (owner.ParentPanel == null ? "null" : owner.ParentPanel.gameObject.name));

            if (owner.UserCloseable == true || owner.ParentPanel != null) {
                UnFocus();
            }

            /*
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
            */
        }

        public virtual void JoystickButton2() {
            Debug.Log(gameObject.name + ".UINavigationController.JoystickButton2()");
            if (activeNavigableButtons.Count != 0 && currentIndex >= 0) {
                currentNavigableElement.JoystickButton2();
            }
        }

        public virtual void JoystickButton3() {
            Debug.Log(gameObject.name + ".UINavigationController.JoystickButton3()");
            if (activeNavigableButtons.Count != 0 && currentIndex >= 0) {
                currentNavigableElement.JoystickButton3();
            }
        }

        public virtual void ReceiveOpenWindowNotification() {
            //Debug.Log(gameObject.name + ".UINavigationController.ReceiveOpenWindowNotification()");
            if (updateActiveListOnOpen) {
                UpdateNavigationList();
                currentIndex = -1;
            }
        }

        public void SetControllerHints(string aOption, string xOption, string yOption, string bOption) {
            Debug.Log(gameObject.name + ".UINavigationController.SetControllerHints()");
            if (owner != null) {
                owner.SetControllerHints(aOption, xOption, yOption, bOption);
            }
        }

        public void HideControllerHints() {
            if (owner != null) {
                owner.HideControllerHints();
            }
        }

    }

}

