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
        private Color hiddenColor = new Color32(0, 0, 0, 0);

        [Tooltip("If false, the highlight image will retain its color when the navigation controller is unfocused")]
        [SerializeField]
        private bool useUnfocusedColor = true;

        [Tooltip("the color to change the highlight image when navigation controller is not focused")]
        [SerializeField]
        private Color unFocusedColor = new Color32(255, 255, 255, 39);


        protected RectTransform rectTransform = null;

        protected bool navigationControllerFocused = false;

        public virtual bool DeselectOnLeave { get => true; }
        public virtual bool CaptureCancelButton { get => false; }
        public virtual bool CaptureDPad { get => false; }
        public RectTransform RectTransform { get => rectTransform; }

        protected int configureCount = 0;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            configureCount++;
            if (configureCount > 1) {
                // disabled because these objects can be pooled
                //Debug.LogWarning(gameObject.name + ".NavigableElement.Configure() This element has been configured multiple times");
                return;
            }

            rectTransform = transform as RectTransform;

            //if (useSystemImageTintColor) {
                highlightOutlineColor = systemConfigurationManager.HighlightOutlineColor;
                highlightImageColor = systemConfigurationManager.HighlightImageColor;
            //}

            UnHighlightBackground();
            UnHighlightOutline();
        }

        public virtual bool Available() {
            return gameObject.activeInHierarchy;
        }

        public virtual void Accept() {
            //Debug.Log(gameObject.name + "NavigableElement.Accept()");
            Interact();
        }

        public virtual void Cancel() {

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
        }

        public virtual void LeftButton() {
        }

        public virtual void RightButton() {
        }

        /// <summary>
        /// leave the element for another element on the same navigation controller
        /// </summary>
        public virtual void LeaveElement() {
            Debug.Log(gameObject.name + ".NavigableElement.LeaveElement()");
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
            //Debug.Log(gameObject.name + ".NavigableElement.Select()");

            if (highlightBackgroundOnSelect == true) {
                HighlightBackground();
            }
            HighlightOutline();
        }

        public virtual void DeSelect() {
            //Debug.Log(gameObject.name + "NavigableElement.DeSelect()");
            /*
            if (highlightBackgroundOnSelect == true) {
                UnHighlightBackground();
            }
            */
            UnHighlightOutline();
        }


        public void HighlightOutline() {
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
            //Debug.Log(gameObject.name + ".HightlightButton.HighlightBackground()");
            if (highlightImage != null) {
                if (navigationControllerFocused || useUnfocusedColor == false) {
                    //highlightImage.color = highlightImageColor * selectedColor;
                    highlightImage.color = highlightImageColor;
                } else {
                    highlightImage.color = unFocusedColor;
                }
            }
        }

        public void UnHighlightBackground() {
            Debug.Log(gameObject.name + ".HightlightButton.UnHighlightBackground()");
            if (highlightImage != null) {
                highlightImage.color = hiddenColor;
            }
        }

        public virtual void FocusNavigationController() {
            Debug.Log(gameObject.name + ".NavigableElement.FocusNavigationController()");

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
            //Debug.Log(gameObject.name + ".NavigableElement.UnFocus()");

            navigationControllerFocused = false;

            DeSelect();

            if (highlightImage == null) {
                return;
            }
            if (unHighlightBackgroundOnUnFocus) {
                UnHighlightBackground();
            } else {
                if (highlightImage.color != hiddenColor && useUnfocusedColor == true) {
                    highlightImage.color = unFocusedColor;
                }
            }

        }

    }

}