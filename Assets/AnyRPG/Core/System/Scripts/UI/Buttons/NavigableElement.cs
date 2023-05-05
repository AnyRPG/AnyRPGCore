using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableElement : ConfiguredMonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

        public event System.Action OnInteract = delegate { };
        /*
        [Header("Navigable Element")]

        [Tooltip("Use the system image color for tinting images")]
        [SerializeField]
        private bool useSystemImageTintColor = true;
        */

        [Header("Outline Image")]

        [SerializeField]
        private Image outlineImage;

        [SerializeField]
        protected Color highlightOutlineColor = new Color32(165, 165, 165, 166);

        [SerializeField]
        protected bool useSystemOutlineColor = true;

        [Header("Highlight Image")]

        [SerializeField]
        private Image highlightImage;

        [Tooltip("The highlight image will be visible when selected")]
        [SerializeField]
        private bool highlightBackgroundOnSelect = false;

        [Tooltip("The highlight image will be visible when on interaction")]
        [SerializeField]
        private bool highlightBackgroundOnInteract = true;

        [Tooltip("The highlight image will be hidden when unfocusing the controller")]
        [SerializeField]
        private bool unHighlightBackgroundOnUnFocus = true;

        [Tooltip("The highlight image will be hidden when leaving the element")]
        [SerializeField]
        private bool unHighlightBackgroundOnLeave = true;

        [Tooltip("The color that will be used on the highlight image")]
        [SerializeField]
        private Color highlightImageColor = new Color32(165, 165, 165, 166);

        // the color to change the highlight image when not hidden
        protected Color hiddenColor = new Color32(0, 0, 0, 0);

        [Tooltip("If gamepad mode is active, and the navigation controller is unfocused, and the button background is highlighted, change the highlight to the unfocused color")]
        [SerializeField]
        private bool useUnfocusedColor = true;

        [Tooltip("the color to change the highlight image when navigation controller is not focused")]
        [SerializeField]
        private Color unFocusedColor = new Color32(255, 255, 255, 39);

        [Header("Scrolling")]

        [Tooltip("used to determine if scroll rect should scroll to keep element in frame and for tooltips")]
        [SerializeField]
        protected RectTransform rectTransform = null;

        protected UINavigationController owner = null;

        protected bool selected = false;

        protected bool navigationControllerFocused = false;

        public virtual bool DeselectOnLeave { get => true; }
        public virtual bool CaptureCancelButton { get => false; }
        public virtual bool CaptureDPad { get => false; }
        public RectTransform RectTransform { get => rectTransform; }

        // game manager references
        protected AudioManager audioManager = null;
        protected ControlsManager controlsManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (configureCount > 1) {
                // disabled because these objects can be pooled
                //Debug.LogWarning(gameObject.name + ".NavigableElement.Configure() This element has been configured multiple times");
                return;
            }

            if (rectTransform == null) {
                rectTransform = transform as RectTransform;
            }

            //if (useSystemImageTintColor) {
            if (useSystemOutlineColor == true) {
                highlightOutlineColor = systemConfigurationManager.UIConfiguration.HighlightOutlineColor;
            }
            highlightImageColor = systemConfigurationManager.UIConfiguration.HighlightImageColor;
            //}

            UnHighlightBackground();
            UnHighlightOutline();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            audioManager = systemGameManager.AudioManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public virtual void SetController(UINavigationController uINavigationController) {
            owner = uINavigationController;
        }

        public virtual bool Available() {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// respond to accept button pressed on gamepad
        /// </summary>
        public virtual void Accept() {
            //Debug.Log($"{gameObject.name}NavigableElement.Accept()");
            Interact();
        }

        public virtual void Cancel() {

        }

        public virtual void JoystickButton2() {
            //Debug.Log($"{gameObject.name}.NavigableElement.JoystickButton2()");
        }

        public virtual void JoystickButton3() {
            //Debug.Log($"{gameObject.name}.NavigableElement.JoystickButton3()");
        }

        public virtual void JoystickButton9() {
            //Debug.Log($"{gameObject.name}.NavigableElement.JoystickButton9()");
        }

        public virtual void LeftAnalog(float inputHorizontal, float inputVertical) {
            //Debug.Log($"{gameObject.name}.NavigableElement.LeftAnalog()");
        }

        public virtual void Interact() {
            OnInteract();

            if (highlightBackgroundOnInteract) {
                HighlightBackground();
            }
        }

        public virtual void UpButton() {
        }

        public virtual void DownButton() {
            //Debug.Log($"{gameObject.name}.NavigableElement.DownButton()");
        }

        public virtual void LeftButton() {
        }

        public virtual void RightButton() {
        }

        /// <summary>
        /// leave the element for another element on the same navigation controller
        /// </summary>
        public virtual void LeaveElement() {
            //Debug.Log($"{gameObject.name}.NavigableElement.LeaveElement()");
            if (DeselectOnLeave) {
                DeSelect();
            }
            if (unHighlightBackgroundOnLeave) {
                UnHighlightBackground();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
        }

        public virtual void OnPointerDown(PointerEventData eventData) {
        }

        public virtual void OnPointerUp(PointerEventData eventData) {
        }

        public virtual void Select() {
            //Debug.Log($"{gameObject.name}.NavigableElement.Select()");

            if (highlightBackgroundOnSelect == true) {
                HighlightBackground();
            }
            if (controlsManager.GamePadInputActive == true) {
                HighlightOutline();
            }
            selected = true;
        }

        public virtual void DeSelect() {
            //Debug.Log($"{gameObject.name}NavigableElement.DeSelect()");
            /*
            if (highlightBackgroundOnSelect == true) {
                UnHighlightBackground();
            }
            */
            UnHighlightOutline();
            selected = false;
        }


        public void HighlightOutline() {
            //Debug.Log($"{gameObject.name}.NavigableElement.HighlightOutline()");

            if (outlineImage != null) {
                outlineImage.color = highlightOutlineColor;
            }
        }

        public void UnHighlightOutline() {
            if (outlineImage != null) {
                outlineImage.color = hiddenColor;
            }
        }

        public void HighlightBackground() {
            //Debug.Log($"{gameObject.name}.NavigableElement.HighlightBackground()");
            
            if (highlightImage == null) {
                return;
            }

            if (navigationControllerFocused == true
                || useUnfocusedColor == false
                || controlsManager.GamePadInputActive == false) {
                //highlightImage.color = highlightImageColor * selectedColor;
                highlightImage.color = highlightImageColor;
            } else {
                highlightImage.color = unFocusedColor;
            }

        }

        public void UnHighlightBackground() {
            //Debug.Log($"{gameObject.name}.NavigableElement.UnHighlightBackground()");

            if (highlightImage != null) {
                highlightImage.color = hiddenColor;
            }
        }

        public virtual void FocusNavigationController() {
            //Debug.Log($"{gameObject.name}.NavigableElement.FocusNavigationController()");

            navigationControllerFocused = true;

            // buttons that are currently disabled could still receive notification that their controller is active
            if (Available() == false) {
                return;
            }

            if (highlightImage == null) {
                return;
            }
            if (highlightImage.color != hiddenColor) {
                HighlightBackground();
            }
        }

        /// <summary>
        /// leave the current navigation controller
        /// </summary>
        public virtual void UnFocus() {
            //Debug.Log($"{gameObject.name}.NavigableElement.UnFocus()");

            navigationControllerFocused = false;

            DeSelect();

            if (highlightImage == null) {
                return;
            }
            if (unHighlightBackgroundOnUnFocus) {
                UnHighlightBackground();
            } else {
                if (highlightImage.color != hiddenColor
                    && useUnfocusedColor == true
                    && controlsManager.GamePadInputActive == true) {
                    highlightImage.color = unFocusedColor;
                }
            }

        }

        public virtual void OnHoverSound() {
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

        public virtual void OnSendObjectToPool() {
            //Debug.Log($"{gameObject.name}.navigableElement.OnSendObjectToPool()");

            UnHighlightOutline();
            UnHighlightBackground();
        }


    }

}