using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CloseableWindowContents : ConfiguredMonoBehaviour, ICloseableWindowContents {

        public virtual event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Closeable Window")]

        [SerializeField]
        protected Image backGroundImage;

        [Tooltip("If true, the camera controls will not apply to the active character.  This is useful if the window contains a camera / unit preview.")]
        [SerializeField]
        protected bool captureCamera = false;

        [SerializeField]
        protected List<ColoredUIElement> coloredUIElements = new List<ColoredUIElement>();


        [Header("Navigation")]

        [Tooltip("Set this field to false for sub panels")]
        [SerializeField]
        protected bool addToWindowStack = true;

        [Tooltip("Set this field to false for a base window that should not be closed by the player.  This does not prevent the window from being closed by the system when necessary")]
        [SerializeField]
        protected bool userCloseable = true;

        [SerializeField]
        protected bool focusFirstButtonOnOpen = true;

        [SerializeField]
        protected bool focusCurrentButtonOnOpen = false;

        [Tooltip("When leaving a navigation controller for a panel (parent window) choose the open sub panel in that window")]
        [SerializeField]
        protected bool focusActiveSubPanel = false;

        /*
        [Tooltip("Set this to false if another panel will configure the navigation controllers")]
        [SerializeField]
        protected bool navigationControllerOwner = true;
        */

        [SerializeField]
        protected List<UINavigationController> uINavigationControllers = new List<UINavigationController>();

        [SerializeField]
        protected List<CloseableWindowContents> subPanels = new List<CloseableWindowContents>();

        [Header("Controller Hints")]

        [SerializeField]
        protected HintBarController hintBarController = null;


        protected UINavigationController currentNavigationController = null;

        protected RectTransform rectTransform;

        protected CloseableWindow closeableWindow = null;

        protected CloseableWindowContents parentPanel = null;

        protected CloseableWindowContents openSubPanel = null;
        protected CloseableWindowContents activeSubPanel = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected AudioManager audioManager = null;
        protected WindowManager windowManager = null;
        protected ControlsManager controlsManager = null;
        protected InputManager inputManager = null;

        public Image BackGroundImage { get => backGroundImage; set => backGroundImage = value; }
        public UINavigationController CurrentNavigationController { get => currentNavigationController; }
        public CloseableWindowContents ParentPanel { get => parentPanel; }
        public bool UserCloseable { get => userCloseable; }
        public RectTransform RectTransform { get => rectTransform; }
        public bool CaptureCamera { get => captureCamera; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.Configure()");
            base.Configure(systemGameManager);
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
            if (hintBarController != null) {
                hintBarController.Configure(systemGameManager);
            }
            if (subPanels.Count > 0) {
                foreach (CloseableWindowContents closeableWindowContents in subPanels) {
                    closeableWindowContents.Configure(systemGameManager);
                    closeableWindowContents.SetParentPanel(this);
                }
            }
            foreach (ColoredUIElement coloredUIElement in coloredUIElements) {
                coloredUIElement.Configure(systemGameManager);
            }
            if (uINavigationControllers.Count != 0) {
                foreach (UINavigationController uINavigationController in uINavigationControllers) {
                    uINavigationController.Configure(systemGameManager);
                    uINavigationController.SetOwner(this);
                }
                //Debug.Log(gameObject.name + ".CloseableWindowContents.Configure() setting current navigation controller");
                currentNavigationController = uINavigationControllers[0];
            }
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
            windowManager = systemGameManager.WindowManager;
            controlsManager = systemGameManager.ControlsManager;
            inputManager = systemGameManager.InputManager;
        }

        public bool HasOpenSubPanel() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.HasOpenSubPanel()");

            return openSubPanel != null;
        }

        public virtual void Init() {
            foreach (CloseableWindowContents closeableWindowContents in subPanels) {
                closeableWindowContents.Init();
            }
        }

        public virtual void SetWindow(CloseableWindow closeableWindow) {
            this.closeableWindow = closeableWindow;
        }

        public virtual void SetParentPanel(CloseableWindowContents closeableWindowContents) {
            parentPanel = closeableWindowContents;
        }

        public virtual void SetActiveSubPanel(CloseableWindowContents closeableWindowContents, bool focus = true) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetActiveSubPanel(" + (closeableWindowContents == null ? "null" : closeableWindowContents.name) + ")");
            if (closeableWindowContents != null) {
                foreach (UINavigationController uINavigationController in uINavigationControllers) {
                    uINavigationController.UnFocus();
                }
            }

            activeSubPanel = closeableWindowContents;

            if (activeSubPanel != null) {
                currentNavigationController = null;
                if (focus == true) {
                    activeSubPanel.FocusCurrentButton();
                }
            }
        }

        public void DeactivateGamepadInput() {
            foreach (CloseableWindowContents closeableWindowContents in subPanels) {
                closeableWindowContents.DeactivateGamepadInput();
            }
            foreach (UINavigationController uINavigationController in uINavigationControllers) {
                uINavigationController.DeactivateGamepadInput();
            }
        }

        public virtual void UnFocus() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.UnFocus()");
            if (currentNavigationController != null) {
                currentNavigationController.UnFocus();
            }
        }

        public virtual void SetOpenSubPanel(CloseableWindowContents closeableWindowContents, bool focus = false) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetOpenSubPanel(" + closeableWindowContents.name + ")");
            if (openSubPanel != null) {
                openSubPanel.UnFocus();
            }
            openSubPanel = closeableWindowContents;
            if (focus == true) {
                SetActiveSubPanel(closeableWindowContents);
            }
        }

        public virtual void SetNavigationController(UINavigationController uINavigationController) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetNavigationController(" + uINavigationController.gameObject.name + ")");
            if (uINavigationControllers.Contains(uINavigationController)) {
                if (currentNavigationController != null &&  currentNavigationController != uINavigationController) {
                    currentNavigationController.UnFocus();
                }
                currentNavigationController = uINavigationController;
                currentNavigationController.Focus();
            }
        }

        public virtual void SetNavigationControllerByIndex(int controllerIndex) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetNavigationController(" + uINavigationController.gameObject.name + ")");
            if (uINavigationControllers.Contains(uINavigationControllers[controllerIndex])) {
                if (currentNavigationController != null && currentNavigationController != uINavigationControllers[controllerIndex]) {
                    currentNavigationController.UnFocus();
                }
                currentNavigationController = uINavigationControllers[controllerIndex];
                //currentNavigationController.Focus();
            }
        }

        public virtual int GetNavigationControllerIndex() {
            for (int i = 0; i < uINavigationControllers.Count; i++) {
                if (currentNavigationController == uINavigationControllers[i]) {
                    return i;
                }
            }
            return 0;
        }


        public virtual void ActivateNavigationController(UINavigationController uINavigationController) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.ActivateNavigationController(" + uINavigationController.gameObject.name + ")");
            SetActiveSubPanel(null);
            SetNavigationController(uINavigationController);
            if (parentPanel != null) {
                parentPanel.SetActiveSubPanel(this, false);
            }
        }

        /// <summary>
        /// re-focus a window after closing another window
        /// </summary>
        public void FocusCurrentButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.FocusCurrentButton()");
            if (activeSubPanel != null) {
                activeSubPanel.FocusCurrentButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.Focus();
                return;
            }
            if (currentNavigationController == null && uINavigationControllers != null && uINavigationControllers.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            }
        }

        public void FocusFirstButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.FocusFirstButton()");
            if (currentNavigationController == null && uINavigationControllers != null) {
                SetNavigationController(uINavigationControllers[0]);
            }
        }

        public virtual void ChooseFocus() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.ChooseFocus()");
            if (controlsManager.GamePadInputActive && focusActiveSubPanel == true) {
                if (openSubPanel != null) {
                    SetActiveSubPanel(openSubPanel);
                    //currentNavigationController = openSubPanel.FocusCurrentButton();
                    //openSubPanel.FocusCurrentButton();
                }
                return;
            }

        }

        public virtual void Accept() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.Accept()");
            if (activeSubPanel != null) {
                activeSubPanel.Accept();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.Accept();
            }
        }

        public virtual bool Cancel() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.Cancel()");
            if (activeSubPanel != null) {
                if (activeSubPanel.Cancel()) {
                    activeSubPanel = null;
                    FocusCurrentButton();
                }
                return false;
            }
            if (currentNavigationController != null && currentNavigationController.CurrentNavigableElement?.CaptureCancelButton == true) {
                currentNavigationController.CurrentNavigableElement.Cancel();
                return false;
            }
            if (currentNavigationController != null) {
                currentNavigationController.Cancel();
            }
            if (userCloseable == true && parentPanel == null) {
                Close();
            }
            return true;
        }

        public virtual void JoystickButton2() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.JoystickButton2()");
            if (activeSubPanel != null) {
                activeSubPanel.JoystickButton2();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.JoystickButton2();
            }
        }

        public virtual void JoystickButton3() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.JoystickButton3()");
            if (activeSubPanel != null) {
                activeSubPanel.JoystickButton3();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.JoystickButton3();
            }
        }

        public virtual void JoystickButton9() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.JoystickButton3()");
            if (activeSubPanel != null) {
                activeSubPanel.JoystickButton9();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.JoystickButton9();
            }
        }

        public virtual void JoystickButton4() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.JoystickButton3()");
            LBButton();
        }

        public virtual void JoystickButton5() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.JoystickButton3()");
            RBButton();
        }

        public virtual void UpButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.UpButton()");
            if (activeSubPanel != null) {
                activeSubPanel.UpButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.UpButton();
            }
        }

        public virtual void DownButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.DownButton()");
            if (activeSubPanel != null) {
                activeSubPanel.DownButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.DownButton();
            }
        }

        public virtual void LeftButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.LeftButton()");
            if (activeSubPanel != null) {
                activeSubPanel.LeftButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.LeftButton();
            }
        }

        public virtual void RightButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.RightButton()");
            if (activeSubPanel != null) {
                activeSubPanel.RightButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.RightButton();
            }
        }

        /// <summary>
        /// return true if there was an active sub panel to pass the command to
        /// </summary>
        /// <returns></returns>
        public virtual bool LeftTrigger() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.RightButton()");
            if (activeSubPanel != null) {
                activeSubPanel.LeftTrigger();
                return true;
            }
            return false;
        }

        /// <summary>
        /// return true if there was an active sub panel to pass the command to
        /// </summary>
        /// <returns></returns>
        public virtual bool RightTrigger() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.RightButton()");
            if (activeSubPanel != null) {
                activeSubPanel.RightTrigger();
                return true;
            }
            return false;
        }

        public virtual void LBButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.LeftButton()");
            if (activeSubPanel != null) {
                activeSubPanel.LBButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.LBButton();
            }
        }

        public virtual void RBButton() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.RightButton()");
            if (activeSubPanel != null) {
                activeSubPanel.RBButton();
                return;
            }
            if (currentNavigationController != null) {
                currentNavigationController.RBButton();
            }
        }



        public virtual void Close() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.Close()");
            if (closeableWindow != null) {
                closeableWindow.CloseWindow();
                return;
            }
            /*
            if (parentPanel != null) {
                parentPanel.Close();
            }
            */
        }

        protected void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        protected virtual void ProcessCreateEventSubscriptions() {
        }

        protected void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        protected virtual void ProcessCleanupEventSubscriptions() {
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
            //Debug.Log(gameObject.name + ".CloseableWindowContents.AddToWindowStack()");
            windowManager.AddWindow(this);
        }

        public virtual void RemoveFromWindowStack() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.RemoveFromWindowStack()");
            if (parentPanel == null && addToWindowStack) {
                windowManager.RemoveWindow(this);
            }
            if (currentNavigationController != null) {
                currentNavigationController.LeaveController();
            }
        }

        public virtual void ProcessOpenWindowNotification() {
            if (hintBarController != null) {
                hintBarController.Hide();
            }
            if (parentPanel == null && addToWindowStack == true) {
                AddToWindowStack();
            }
            foreach (UINavigationController uINavigationController in uINavigationControllers) {
                uINavigationController.ReceiveOpenWindowNotification();
            }
            if (currentNavigationController != null) {
                if (parentPanel == null) {
                    currentNavigationController.Focus(false);
                }
                if (controlsManager.GamePadInputActive) {
                    if (focusFirstButtonOnOpen) {
                        currentNavigationController.FocusFirstButton();
                    } else if (focusCurrentButtonOnOpen) {
                        currentNavigationController.FocusCurrentButton();
                    }
                }
            } else {
                //Debug.Log("No navigation controller for " + gameObject.name);
            }
        }

        public void ReceiveOpenWindowNotification() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.ReceiveOpenWindowNotification()");
            
            ProcessOpenWindowNotification();
        }

        public virtual void ReceiveClosedWindowNotification() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.ReceiveClosedWindowNotification()");
            RemoveFromWindowStack();
            OnCloseWindow(this);
        }

        public void SetBackGroundColor(Color color) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetBackGroundColor()");
            if (backGroundImage != null) {
                //Debug.Log(gameObject.name + ".CloseableWindowContents.SetBackGroundColor(): background image is not null, setting color");
                backGroundImage.color = color;
            } else {
                //Debug.Log(gameObject.name + ".CloseableWindowContents.SetBackGroundColor(): background image IS NULL!");
            }
        }

        public void SetControllerHints(string aOption, string xOption, string yOption, string bOption, string dPadOption, string rDownOption) {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.SetControllerHints()");

            // first, check for a local hint bar
            if (hintBarController != null) {
                hintBarController.SetOptions(aOption, xOption, yOption, bOption, dPadOption, rDownOption);
                return;
            }

            // if no local hint bar found, check for a parent panel
            if (parentPanel != null) {
                parentPanel.SetControllerHints(aOption, xOption, yOption, bOption, dPadOption, rDownOption);
                return;
            }

            // if no parent panel found, check for a parent window frame
            if (closeableWindow != null) {
                closeableWindow.SetControllerHints(aOption, xOption, yOption, bOption, dPadOption, rDownOption);
            }
        }

        public void HideControllerHints() {
            //Debug.Log(gameObject.name + ".CloseableWindowContents.HideControllerHints()");

            // first, check for a local hint bar
            if (hintBarController != null) {
                hintBarController.Hide();
                return;
            }

            // if no local hint bar found, check for a parent panel
            if (parentPanel != null) {
                parentPanel.HideControllerHints();
                return;
            }

            // if no parent panel found, check for a parent window frame
            if (closeableWindow != null) {
                closeableWindow.HideControllerHints();
            }

        }

        public void LeftAnalog(float inputHorizontal, float inputVertical) {
            //Debug.Log(gameObject.name + ".NavigableElement.LeftAnalog()");

            // if the left analog stick was held down, then this is a movement of the window
            // send the event to the window so it can pass it on to the drag handle
            if (closeableWindow != null && inputManager.KeyBindWasPressedOrHeld("JOYSTICKBUTTON8")) {
                closeableWindow.LeftAnalog(inputHorizontal, inputVertical);
                return;
            }

            // if the left analog stick was not clicked, this is to scroll a scrollrect

            if (activeSubPanel != null) {
                activeSubPanel.LeftAnalog(inputHorizontal, inputVertical);
                return;
            }

            // there are no sub panels
            // pass it onto the navigation controller
            if (currentNavigationController != null) {
                currentNavigationController.LeftAnalog(inputHorizontal, inputVertical);
            }
        }
    }

}