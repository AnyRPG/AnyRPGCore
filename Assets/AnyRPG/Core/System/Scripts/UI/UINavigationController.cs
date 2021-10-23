using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    [System.Serializable]
    public class UINavigationController : ConfiguredMonoBehaviour {

        [Tooltip("If the far left of the buttons is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController leftController = null;

        [Tooltip("If the far right of the buttons is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController rightController = null;

        [Tooltip("If the top button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController upController = null;

        [Tooltip("If the bottom button is passed, switch to this controller")]
        [SerializeField]
        protected UINavigationController downController = null;

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

        public virtual void AddActiveButton(NavigableElement navigableElement) {
            activeNavigableButtons.Add(navigableElement);
        }

        public virtual void ClearActiveButtons() {
            activeNavigableButtons.Clear();
        }

        public virtual void FocusFirstButton() {
            Debug.Log("UINavigationController.FocusInitialButton()");
        }

        public virtual void FocusCurrentButton() {
            Debug.Log("UINavigationController.FocusCurrentButton()");
        }

        public virtual void UpButton() {
            Debug.Log("UINavigationController.UpButton()");
        }

        public virtual void DownButton() {
            Debug.Log("UINavigationController.DownButton()");
        }

        public virtual void LeftButton() {
            Debug.Log("UINavigationController.LeftButton()");
        }

        public virtual void RightButton() {
            Debug.Log("UINavigationController.RightButton()");
        }


        public virtual void Accept() {
            Debug.Log("UINavigationController.Accept()");
        }

        public virtual void UpdateNavigationList() {
            Debug.Log("UINavigationController.UpdateNavigationList()");
        }

        public virtual void ReceiveOpenWindowNotification() {
            Debug.Log("UINavigationController.ReceiveOpenWindowNotification()");
            UpdateNavigationList();
        }



    }

}

