using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CloseableWindowContents : ConfiguredMonoBehaviour, ICloseableWindowContents {

        public virtual event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        protected Image backGroundImage;

        [SerializeField]
        protected List<ColoredUIElement> coloredUIElements = new List<ColoredUIElement>();

        [Header("Navigation")]

        [Tooltip("Set this field to false for a base window that should not be closed by the player.  This does not prevent the window from being closed by the system when necessary")]
        [SerializeField]
        protected bool userCloseable = true;

        [SerializeField]
        protected bool focusFirstButtonOnOpen = true;

        [SerializeField]
        protected List<UINavigationController> uINavigationControllers = new List<UINavigationController>();

        protected UINavigationController currentNavigationController = null;

        protected RectTransform rectTransform;

        protected CloseableWindow closeableWindow = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected AudioManager audioManager = null;
        protected ControlsManager controlsManager = null;

        public Image BackGroundImage { get => backGroundImage; set => backGroundImage = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".WindowContentController.Init()");
            base.Configure(systemGameManager);
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
            foreach (ColoredUIElement coloredUIElement in coloredUIElements) {
                coloredUIElement.Configure(systemGameManager);
            }
            if (uINavigationControllers.Count != 0) {
                foreach (UINavigationController uINavigationController in uINavigationControllers) {
                    uINavigationController.Configure(systemGameManager);
                    uINavigationController.SetOwner(this);
                }
                currentNavigationController = uINavigationControllers[0];
            }
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public virtual void SetWindow(CloseableWindow closeableWindow) {
            this.closeableWindow = closeableWindow;
        }

        public virtual void SetNavigationController(UINavigationController uINavigationController) {
            if (uINavigationControllers.Contains(uINavigationController)) {
                currentNavigationController = uINavigationController;
                currentNavigationController.FocusCurrentButton();
            }
        }

        public void Accept() {
            if (currentNavigationController != null) {
                currentNavigationController.Accept();
            }
        }

        public void Cancel() {
            if (userCloseable == true) {
                Close();
            }
        }

        public void UpButton() {
            if (currentNavigationController != null) {
                currentNavigationController.UpButton();
            }
        }

        public void DownButton() {
            if (currentNavigationController != null) {
                currentNavigationController.DownButton();
            }
        }

        public void LeftButton() {
            if (currentNavigationController != null) {
                currentNavigationController.LeftButton();
            }
        }

        public void RightButton() {
            if (currentNavigationController != null) {
                currentNavigationController.RightButton();
            }
        }

        public virtual void Close() {
            if (closeableWindow != null) {
                closeableWindow.CloseWindow();
            }
        }

        protected virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        protected virtual void OnDestroy() {
            //Debug.Log("WindowContentController.OnDestroy()");
            CleanupEventSubscriptions();
        }

        public virtual void OnHoverSound() {
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

        public virtual void ReceiveOpenWindowNotification() {
            //Debug.Log(gameObject.name + "WindowContentController.ReceiveOpenWindowNotification()");
            controlsManager.AddWindow(this);
            if (currentNavigationController != null) {
                currentNavigationController.ReceiveOpenWindowNotification();
                if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad && focusFirstButtonOnOpen == true) {
                    currentNavigationController.FocusFirstButton();
                }
            } else {
                Debug.Log("No navigation controller for " + gameObject.name);
            }
        }

        public virtual void RecieveClosedWindowNotification() {
            controlsManager.RemoveWindow(this);
            OnCloseWindow(this);
        }

        public void SetBackGroundColor(Color color) {
            //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor()");
            if (backGroundImage != null) {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image is not null, setting color");
                backGroundImage.color = color;
            } else {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image IS NULL!");
            }
        }
    }

}