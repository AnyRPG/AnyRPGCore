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
        protected bool focusActiveSubPanel = false;

        [Tooltip("Set this to false if another panel will configure the navigation controllers")]
        [SerializeField]
        protected bool navigationControllerOwner = true;

        [SerializeField]
        protected List<UINavigationController> uINavigationControllers = new List<UINavigationController>();

        [SerializeField]
        protected List<CloseableWindowContents> subPanels = new List<CloseableWindowContents>();

        protected UINavigationController currentNavigationController = null;

        protected RectTransform rectTransform;

        protected CloseableWindow closeableWindow = null;

        protected CloseableWindowContents parentPanel = null;

        protected CloseableWindowContents activeSubPanel = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected AudioManager audioManager = null;
        protected ControlsManager controlsManager = null;

        public Image BackGroundImage { get => backGroundImage; set => backGroundImage = value; }
        public UINavigationController CurrentNavigationController { get => currentNavigationController; }

        public override void Configure(SystemGameManager systemGameManager) {
            Debug.Log(gameObject.name + ".CloseableWindowContents.Configure()");
            base.Configure(systemGameManager);
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
            foreach (ColoredUIElement coloredUIElement in coloredUIElements) {
                coloredUIElement.Configure(systemGameManager);
            }
            if (uINavigationControllers.Count != 0) {
                if (navigationControllerOwner) {
                    foreach (UINavigationController uINavigationController in uINavigationControllers) {
                        uINavigationController.Configure(systemGameManager);
                        uINavigationController.SetOwner(this);
                    }
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

        public virtual void SetParentPanel(CloseableWindowContents closeableWindowContents) {
            parentPanel = closeableWindowContents;
        }

        public virtual void SetActiveSubPanel(CloseableWindowContents closeableWindowContents) {
            activeSubPanel = closeableWindowContents;
            SetNavigationController(activeSubPanel.currentNavigationController);
        }

        public virtual void SetNavigationController(UINavigationController uINavigationController) {
            Debug.Log(gameObject.name + ".CloseableWindowContents.SetNavigationController(" + uINavigationController.name + ")");
            if (uINavigationControllers.Contains(uINavigationController)) {
                currentNavigationController = uINavigationController;
                currentNavigationController.FocusCurrentButton();
            }
        }

        /// <summary>
        /// re-focus a window after closing another window
        /// </summary>
        public UINavigationController FocusCurrentButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.Focus()");
            if (currentNavigationController != null) {
                currentNavigationController.FocusCurrentButton();
                return currentNavigationController;
            }
            return null;
        }

        public UINavigationController FocusFirstButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.FocusFirstButton()");
            if (currentNavigationController != null) {
                currentNavigationController.FocusFirstButton();
                return currentNavigationController;
            } else {
                Debug.Log(gameObject.name + ".CloseableWindowContents.FocusFirstButton(): currentNavigationController is null");
            }
            return null;
        }

        public void ChooseFocus() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.ChooseFocus()");
            if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad && focusActiveSubPanel == true) {
                currentNavigationController = activeSubPanel.FocusCurrentButton();
                return;
            }

        }

        public void Accept() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.Accept()");
            if (currentNavigationController != null) {
                currentNavigationController.Accept();
            }
        }

        public void Cancel() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.Cancel()");
            if (currentNavigationController != null && currentNavigationController.CurrentNavigableElement.CaptureCancelButton == true) {
                currentNavigationController.CurrentNavigableElement.Cancel();
                return;
            }
            if (userCloseable == true) {
                Close();
            }
        }

        public void UpButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.UpButton()");
            if (currentNavigationController != null) {
                currentNavigationController.UpButton();
            }
        }

        public void DownButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.DownButton()");
            if (currentNavigationController != null) {
                currentNavigationController.DownButton();
            }
        }

        public void LeftButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.LeftButton()");
            if (currentNavigationController != null) {
                currentNavigationController.LeftButton();
            }
        }

        public void RightButton() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.RightButton()");
            if (currentNavigationController != null) {
                currentNavigationController.RightButton();
            }
        }

        public virtual void Close() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.Close()");
            if (closeableWindow != null) {
                closeableWindow.CloseWindow();
                return;
            }
            if (parentPanel != null) {
                parentPanel.Close();
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

        public virtual void AddToWindowStack() {
            controlsManager.AddWindow(this);
        }

        public virtual void RemoveFromWindowStack() {
            controlsManager.RemoveWindow(this);
        }

        public virtual void ReceiveOpenWindowNotification() {
            Debug.Log(gameObject.name + ".CloseableWindowContents.ReceiveOpenWindowNotification()");
            if (parentPanel == null) {
                AddToWindowStack();
            }
            if (currentNavigationController != null) {
                currentNavigationController.ReceiveOpenWindowNotification();
                if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad && focusFirstButtonOnOpen == true) {
                    currentNavigationController.FocusFirstButton();
                }
            } else {
                Debug.Log("No navigation controller for " + gameObject.name);
            }
        }

        public virtual void ReceiveClosedWindowNotification() {
            RemoveFromWindowStack();
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