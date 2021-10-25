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

        [Tooltip("If the left button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents leftPanel = null;

        [Tooltip("If the left button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController leftController = null;

        [Tooltip("If the right button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents rightPanel = null;

        [Tooltip("If the righ button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController rightController = null;

        [Tooltip("If the top button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents upPanel = null;

        [Tooltip("If the top button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController upController = null;

        [Tooltip("If the bottom button is passed, switch to the first controller on this panel")]
        [SerializeField]
        protected CloseableWindowContents downPanel = null;

        [Tooltip("If the bottom button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController downController = null;

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

        //public NavigationControllerDirection NavigationDirection { get => navigationDirection; }

        protected ControlsManager controlsManager = null;

        public virtual NavigableElement CurrentNavigableElement {
            get {
                return currentNavigableElement;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            controlsManager = systemGameManager.ControlsManager;
        }

        public void SetOwner(CloseableWindowContents closeableWindowContents) {
            Debug.Log(gameObject.name + ".UINavigationController.SetOwner(" + closeableWindowContents.name + ")");
            owner = closeableWindowContents;
        }

        public virtual void SetActive() {
            Debug.Log(gameObject.name + ".UINavigationController.SetActive()");
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
            Debug.Log(gameObject.name + "UINavigationController.FocusInitialButton()");
        }

        public virtual void FocusCurrentButton() {
            Debug.Log(gameObject.name + "UINavigationController.FocusCurrentButton()");
        }

        public virtual void UpButton() {
            Debug.Log(gameObject.name + "UINavigationController.UpButton()");
        }

        public virtual bool LeaveUp() {
            Debug.Log(gameObject.name + "UINavigationController.LeaveUp()");
            if (upController != null) {
                currentNavigableElement.LeaveElement();
                upController.SetActive();
                return true;
            }
            if (upPanel != null) {
                currentNavigableElement.LeaveElement();
                upPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void DownButton() {
            Debug.Log(gameObject.name + "UINavigationController.DownButton()");
        }

        public virtual bool LeaveDown() {
            Debug.Log(gameObject.name + "UINavigationController.LeaveDown()");
            if (downController != null) {
                currentNavigableElement.LeaveElement();
                downController.SetActive();
                return true;
            }
            if (downPanel != null) {
                currentNavigableElement.LeaveElement();
                downPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void LeftButton() {
            Debug.Log(gameObject.name + "UINavigationController.LeftButton()");
        }

        public virtual bool LeaveLeft() {
            Debug.Log(gameObject.name + "UINavigationController.LeaveLeft()");
            if (leftController != null) {
                currentNavigableElement.LeaveElement();
                leftController.SetActive();
                return true;
            }
            if (leftPanel != null) {
                currentNavigableElement.LeaveElement();
                leftPanel.ChooseFocus();
                return true;
            }
            return false;
        }

        public virtual void RightButton() {
            Debug.Log(gameObject.name + "UINavigationController.RightButton()");
        }

        public virtual bool LeaveRight() {
            Debug.Log(gameObject.name + "UINavigationController.LeaveRight()");
            if (rightController != null) {
                currentNavigableElement.LeaveElement();
                rightController.SetActive();
                return true;
            }
            if (rightPanel != null) {
                currentNavigableElement.LeaveElement();
                rightPanel.ChooseFocus();
                return true;
            }
            return false;
        }


        public virtual void Accept() {
            Debug.Log(gameObject.name + "UINavigationController.Accept()");
            if (acceptController != null) {
                acceptController.SetActive();
            }
        }

        public virtual void Cancel() {
            Debug.Log(gameObject.name + "UINavigationController.Cancel()");
            if (currentNavigableElement != null) {
                currentNavigableElement.LeaveElement();
            }
        }

        public virtual void UpdateNavigationList() {
            Debug.Log(gameObject.name + "UINavigationController.UpdateNavigationList()");
            activeNavigableButtons.Clear();
            foreach (NavigableElement navigableElement in navigableButtons) {
                if (navigableElement.gameObject.activeSelf == true) {
                    activeNavigableButtons.Add(navigableElement);
                }
            }
        }

        public virtual void ReceiveOpenWindowNotification() {
            Debug.Log(gameObject.name + "UINavigationController.ReceiveOpenWindowNotification()");
            UpdateNavigationList();
        }



    }

}

