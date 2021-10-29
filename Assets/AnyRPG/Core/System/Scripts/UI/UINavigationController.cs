using AnyRPG;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField]
        protected List<NavigableElement> navigableButtons = new List<NavigableElement>();

        protected List<NavigableElement> activeNavigableButtons = new List<NavigableElement>();

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        //private int currentIndex = -1;

        protected NavigableElement currentNavigableElement = null;

        protected CloseableWindowContents owner = null;

        // setting index to -1 so that if gamepad isn't default, the first down press will highlight the first button instead of the second one
        protected int currentIndex = -1;

        // game manager references
        protected ControlsManager controlsManager = null;

        public virtual NavigableElement CurrentNavigableElement {
            get {
                return currentNavigableElement;
            }
        }

        public int CurrentIndex { get => currentIndex; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (NavigableElement navigableElement in navigableButtons) {
                navigableElement.Configure(systemGameManager);
                if (navigableElement.gameObject.activeSelf == true) {
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

        public virtual void UpdateNavigationList() {
            //Debug.Log(gameObject.name + ".UINavigationController.UpdateNavigationList()");
            activeNavigableButtons.Clear();
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public virtual void SetActive() {
            //Debug.Log(gameObject.name + ".UINavigationController.SetActive()");
            if (owner != null) {
                owner.SetActiveSubPanel(null);
                owner.SetNavigationController(this);
                if (owner.ParentPanel != null) {
                    owner.ParentPanel.SetActiveSubPanel(owner);
                }

            }
        }

        public virtual void AddActiveButton(NavigableElement navigableElement) {
            activeNavigableButtons.Add(navigableElement);
        }

        public virtual void ClearActiveButtons() {
            activeNavigableButtons.Clear();
        }

        public virtual void FocusFirstButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.FocusFirstButton()");
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
            //Debug.Log(gameObject.name + ".UINavigationController.FocusCurrentButton()");
        }

        public virtual void UpButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.UpButton()");
        }

        public virtual bool LeaveUp() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveUp()");
            if (upControllers.Count != 0) {
                currentNavigableElement.LeaveElement();
                foreach (UINavigationController uINavigationController in upControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.SetActive();
                        return true;
                    }
                }
            }
            if (upPanel != null) {
                currentNavigableElement.LeaveElement();
                upPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void DownButton() {
            //Debug.Log(gameObject.name + ".UINavigationController.DownButton()");
        }

        public virtual bool LeaveDown() {
            //Debug.Log(gameObject.name + ".UINavigationController.LeaveDown()");
            if (downControllers.Count != 0) {
                currentNavigableElement.LeaveElement();
                foreach (UINavigationController uINavigationController in downControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.SetActive();
                        return true;
                    }
                }
            }
            if (downPanel != null) {
                currentNavigableElement.LeaveElement();
                downPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void LeftButton() {
            Debug.Log(gameObject.name + ".UINavigationController.LeftButton()");
        }

        public virtual bool LeaveLeft() {
            Debug.Log(gameObject.name + ".UINavigationController.LeaveLeft()");
            if (leftControllers.Count != 0) {
                currentNavigableElement.LeaveElement();
                foreach (UINavigationController uINavigationController in downControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.SetActive();
                        return true;
                    }
                }
            }
            if (leftPanel != null) {
                currentNavigableElement.LeaveElement();
                leftPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void RightButton() {
            Debug.Log(gameObject.name + ".UINavigationController.RightButton()");
        }

        public virtual bool LeaveRight() {
            Debug.Log(gameObject.name + ".UINavigationController.LeaveRight()");
            if (rightControllers.Count != 0) {
                currentNavigableElement.LeaveElement();
                foreach (UINavigationController uINavigationController in downControllers) {
                    if (uINavigationController.gameObject.activeInHierarchy == true) {
                        uINavigationController.SetActive();
                        return true;
                    }
                }
            }
            if (rightPanel != null) {
                currentNavigableElement.LeaveElement();
                rightPanel.ChooseFocus();
                return true;
            }
            return false;
        }


        public virtual void Accept() {
            Debug.Log(gameObject.name + ".UINavigationController.Accept()");
            if (acceptController != null) {
                acceptController.SetActive();
                return;
            }
            if (activeNavigableButtons.Count == 0) {
                return;
            }
            if (currentIndex < 0) {
                currentIndex = 0;
                currentNavigableElement = activeNavigableButtons[currentIndex];
                currentNavigableElement.Select();
                return;
            }
            currentNavigableElement = activeNavigableButtons[currentIndex];
            currentNavigableElement.Accept();
        }

        public virtual void Cancel() {
            Debug.Log(gameObject.name + ".UINavigationController.Cancel()");
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
        }

        public virtual void ReceiveOpenWindowNotification() {
            //Debug.Log(gameObject.name + ".UINavigationController.ReceiveOpenWindowNotification()");
            UpdateNavigationList();
        }



    }

}

