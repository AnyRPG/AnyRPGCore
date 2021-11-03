using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class HighlightButton : NavigableElement {

        [Header("Highlight Button")]

        [SerializeField]
        protected TextMeshProUGUI text;

        [SerializeField]
        protected Image highlightImage;

        [SerializeField]
        protected Button highlightButton;

        [Tooltip("The highlight image will be invisible when not selected or hovered")]
        [SerializeField]
        protected bool hideImageWhenInactive = true;

        [Tooltip("Tint the image with the highlight color")]
        [SerializeField]
        protected bool tintImage = true;

        [Tooltip("use the system image color for the tint")]
        [SerializeField]
        protected bool useSystemImageTintColor = true;

        [SerializeField]
        protected bool useHighlightColorOnButton;

        [SerializeField]
        protected Color highlightColor;

        [SerializeField]
        protected Color normalColor = new Color32(163, 163, 163, 28);

        [SerializeField]
        protected Color highlightedColor = new Color32(165, 165, 165, 103);

        [SerializeField]
        protected Color pressedColor = new Color32(120, 120, 120, 74);

        [SerializeField]
        protected Color selectedColor = new Color32(165, 165, 165, 103);

        [SerializeField]
        protected Color disabledColor = new Color32(200, 200, 200, 128);

        [SerializeField]
        protected bool CapitalizeText = false;

        // game manager references
        protected AudioManager audioManager = null;
        protected UIManager uIManager = null;

        public TextMeshProUGUI Text { get => text; }
        public Button Button { get => highlightButton; set => highlightButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (useSystemImageTintColor) {
                highlightColor = systemConfigurationManager.DefaultUIColor;
            }
            SetDefaultImageColor();
            if (highlightButton != null) {
                Image highlightButtonImage = highlightButton.GetComponent<Image>();
                if (highlightButtonImage != null) {
                    highlightButtonImage.color = systemConfigurationManager.DefaultUISolidColor;
                }
            }
            if (highlightButton != null && useHighlightColorOnButton == true) {
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

        private void SetDefaultImageColor() {
            if (highlightImage != null) {
                if (hideImageWhenInactive) {
                    highlightImage.color = new Color32(0, 0, 0, 0);
                } else {
                    highlightImage.color = normalColor;
                }
            }
        }

        private void SetNormalColors() {

        }

        public override void SetGameManagerReferences() {
            //Debug.Log(gameObject.name + ".HighlightButton.SetGameManagerReferences(): " + GetInstanceID());
            base.SetGameManagerReferences();

            audioManager = systemGameManager.AudioManager;
            uIManager = systemGameManager.UIManager;
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".HighlightButton.Select()");
            base.Select();
            if (highlightImage != null) {
                //Debug.Log(gameObject.name + ".HighlightButton.Select(): highlightimage is not null");
                if (tintImage) {
                    //Debug.Log(gameObject.name + ".HighlightButton.Select(): highlightimage is not null: setting highlightcolor on image");
                    highlightImage.color = highlightColor;
                }
            }
            if (highlightButton != null && useHighlightColorOnButton == true) {
                ColorBlock colorBlock = highlightButton.colors;
                colorBlock.normalColor = highlightColor;
                colorBlock.highlightedColor = highlightColor;
                colorBlock.selectedColor = highlightColor;
                highlightButton.colors = colorBlock;
            }
            if (CapitalizeText == true) {
                text.text = text.text.ToUpper();
            }
            if (highlightButton != null) {
                EventSystem.current.SetSelectedGameObject(highlightButton.gameObject);
            }/* else {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }*/
        }

        public override void DeSelect() {
            //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
            SetDefaultImageColor();
            if (highlightButton != null && useHighlightColorOnButton == true) {
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

        public virtual void OnHoverSound() {
            /*
            if (audioManager == null) {
                Debug.Log(gameObject.name + ".HighlightButton.OnHoverSound() : audioManager is null!: " + GetInstanceID());
            }
            */
            if (highlightButton != null && highlightButton.interactable == false) {
                // don't do hover sound for buttons we can't click
                return;
            }
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            OnHoverSound();
        }

        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            OnClickSound();
            Interact();
        }

        public override void Accept() {
            base.Accept();
            if (highlightButton != null) {
                highlightButton.onClick.Invoke();
            }
        }

        public virtual void CheckMouse() {
            if (UIManager.MouseInRect(transform as RectTransform)) {
                uIManager.HideToolTip();
            }
        }

    }

}