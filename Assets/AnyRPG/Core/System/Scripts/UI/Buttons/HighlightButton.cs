using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    public class HighlightButton : NavigableElement {

        [Header("Highlight Button")]

        [SerializeField]
        protected TextMeshProUGUI text;

        [Tooltip("If true, the image attached to the button will not be tinted with the system defined highlightButtonColor")]
        [SerializeField]
        protected bool ignoreSystemImageTint = false;

        [SerializeField]
        protected Button highlightButton;

        /*
        [Tooltip("The highlight image will be invisible when not selected or hovered")]
        [SerializeField]
        protected bool hideImageWhenInactive = true;
        */


        [Tooltip("The normalColor, highlightedColor, and selectedColor will be overwritten with this color when the button is selected")]
        [SerializeField]
        protected Color selectedButtonColor = new Color32(165, 165, 165, 166);

        [Tooltip("Use locally defined local color instead of system configuration manager normal color")]
        [SerializeField]
        protected bool overrideNormalColor = false;

        [Tooltip("Color when not clicked or hovered")]
        [SerializeField]
        protected Color normalColor = new Color32(163, 163, 163, 82);

        [Tooltip("Use locally defined local color instead of system configuration manager highlighted color")]
        [SerializeField]
        protected bool overrideHighlightedColor = false;

        [Tooltip("Color when mouse hovered")]
        [SerializeField]
        protected Color highlightedColor = new Color32(165, 165, 165, 166);

        [Tooltip("Use locally defined local color instead of system configuration manager highlighted color")]
        [SerializeField]
        protected bool overridePressedColor = false;

        [Tooltip("Color during mouse click")]
        [SerializeField]
        protected Color pressedColor = new Color32(120, 120, 120, 71);

        [Tooltip("Use locally defined local color instead of system configuration manager highlighted color")]
        [SerializeField]
        protected bool overrideSelectedColor = false;

        [Tooltip("Color after mouse click")]
        [SerializeField]
        protected Color selectedColor = new Color32(165, 165, 165, 166);

        [Tooltip("Use locally defined local color instead of system configuration manager highlighted color")]
        [SerializeField]
        protected bool overrideDisabledColor = false;

        [Tooltip("Color when not interactable")]
        [SerializeField]
        //protected Color disabledColor = new Color32(200, 200, 200, 128);
        protected Color disabledColor = new Color32(82, 82, 82, 17);

        [SerializeField]
        protected bool CapitalizeText = false;

        // game manager references
        protected UIManager uIManager = null;

        public TextMeshProUGUI Text { get => text; }
        public Button Button { get => highlightButton; set => highlightButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (!ignoreSystemImageTint) {
                //highlightedButtonColor = systemConfigurationManager.DefaultUISolidColor;
                if (highlightButton != null) {
                    //Image highlightButtonImage = highlightButton.GetComponent<Image>();
                    //if (highlightButtonImage != null) {
                    if (highlightButton.targetGraphic != null) {
                        //highlightButtonImage.color = systemConfigurationManager.HighlightButtonColor;
                        highlightButton.targetGraphic.color = systemConfigurationManager.HighlightButtonColor;
                    }
                }
            }
            if (!overrideNormalColor) {
                normalColor = systemConfigurationManager.ButtonNormalColor;
            }
            if (!overrideHighlightedColor) {
                highlightedColor = systemConfigurationManager.ButtonHighlightedColor;
            }
            if (!overridePressedColor) {
                pressedColor = systemConfigurationManager.ButtonPressedColor;
            }
            if (!overrideSelectedColor) {
                selectedColor = systemConfigurationManager.ButtonSelectedColor;
            }
            if (!overrideDisabledColor) {
                disabledColor = systemConfigurationManager.ButtonDisabledColor;
            }
            if (highlightButton != null) {
                ColorBlock colorBlock = highlightButton.colors;
                colorBlock.normalColor = normalColor;
                colorBlock.highlightedColor = highlightedColor;
                colorBlock.pressedColor = pressedColor;
                colorBlock.selectedColor = selectedColor;
                colorBlock.disabledColor = disabledColor;
                highlightButton.colors = colorBlock;
            }
            DeSelect();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log(gameObject.name + ".HighlightButton.SetGameManagerReferences(): " + GetInstanceID());
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public override bool Available() {
            if (highlightButton != null && highlightButton.interactable == false) {
                return false;
            }

            return base.Available();
        }


        public override void Select() {
            //Debug.Log(gameObject.name + ".HighlightButton.Select()");
            base.Select();
            //if (highlightButton != null && useHighlightColorOnButton == true) {
            if (highlightButton != null && navigationControllerFocused == true && controlsManager.GamePadInputActive == true) {
                ColorBlock colorBlock = highlightButton.colors;
                colorBlock.normalColor = selectedButtonColor;
                colorBlock.highlightedColor = selectedButtonColor;
                colorBlock.selectedColor = selectedButtonColor;
                highlightButton.colors = colorBlock;
                EventSystem.current.SetSelectedGameObject(highlightButton.gameObject);
            }
            if (CapitalizeText == true) {
                text.text = text.text.ToUpper();
            }
        }

        public override void DeSelect() {
            //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
            base.DeSelect();
            if (highlightButton != null) {
                ColorBlock colorBlock = highlightButton.colors;
                colorBlock.normalColor = normalColor;
                colorBlock.highlightedColor = highlightedColor;
                colorBlock.pressedColor = pressedColor;
                colorBlock.selectedColor = selectedColor;
                colorBlock.disabledColor = disabledColor;
                highlightButton.colors = colorBlock;
            }
            if (CapitalizeText == true) {
                text.text = text.text.ToLower();
            }
            EventSystem.current.SetSelectedGameObject(null);
        }

        public override void OnHoverSound() {
            if (highlightButton != null && highlightButton.interactable == false) {
                // don't do hover sound for buttons we can't click
                return;
            }
            base.OnHoverSound();
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            OnHoverSound();
        }

        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log(gameObject.name + ".HighlightButton.OnPointerClick()");
            base.OnPointerClick(eventData);
            OnClickSound();

            Interact();
        }

        /*
        public override void Interact() {
            //Debug.Log(gameObject.name + ".HighlightButton.Interact()");

            base.Interact();
            
        }
        */

        public override void Accept() {
            base.Accept();
            if (highlightButton != null) {
                highlightButton.onClick.Invoke();
            }
        }

        public virtual void ButtonClickAction() {
            // only use in child classes
        }


        public virtual void CheckMouse() {
            if (UIManager.MouseInRect(transform as RectTransform)) {
                uIManager.HideToolTip();
            }
        }

    }

}