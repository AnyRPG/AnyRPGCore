using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class HighlightButton : ConfiguredMonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

        [SerializeField]
        protected TextMeshProUGUI text;

        [SerializeField]
        protected Image highlightImage;

        [SerializeField]
        protected Button highlightButton;

        [SerializeField]
        protected bool useHighlightColor;

        [SerializeField]
        protected bool useHighlightColorOnButton;

        [SerializeField]
        protected Color highlightColor;

        [SerializeField]
        protected Color baseColor;

        [SerializeField]
        protected Color baseHighlightColor;

        [SerializeField]
        protected bool CapitalizeText = false;

        // game manager references
        protected SystemConfigurationManager systemConfigurationManager = null;
        protected AudioManager audioManager = null;

        public TextMeshProUGUI Text { get => text; }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            audioManager = systemGameManager.AudioManager;

            if (highlightImage != null) {
                highlightColor = systemConfigurationManager.DefaultUIColor;
            }
            if (highlightButton != null) {
                Image highlightButtonImage = highlightButton.GetComponent<Image>();
                if (highlightButtonImage != null) {
                    highlightButtonImage.color = systemConfigurationManager.DefaultUISolidColor;
                }
            }
            DeSelect();
        }

        public virtual void Select() {
            //Debug.Log(gameObject.name + ".HighlightButton.Select()");
            if (highlightImage != null) {
                //Debug.Log(gameObject.name + ".HighlightButton.Select(): highlightimage is not null");
                if (useHighlightColor) {
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
        }

        public virtual void DeSelect() {
            //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
            if (highlightImage != null) {
                if (useHighlightColor) {
                    highlightImage.color = baseColor;
                }
            } else {
                //Debug.Log(gameObject.name + ".HightlightButton.DeSelect(): highlight image is null");
            }
            if (highlightButton != null && useHighlightColorOnButton == true) {
                ColorBlock colorBlock = highlightButton.colors;
                colorBlock.normalColor = baseColor;
                colorBlock.highlightedColor = baseHighlightColor;
                colorBlock.selectedColor = baseHighlightColor;
                highlightButton.colors = colorBlock;
            }
            if (CapitalizeText == true) {
                text.text = text.text.ToLower();
            }
        }

        public virtual void OnHoverSound() {
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            OnHoverSound();
        }

        public void OnPointerClick(PointerEventData eventData) {
            OnClickSound();
        }

        public void OnPointerDown(PointerEventData eventData) {
        }

        public void OnPointerUp(PointerEventData eventData) {
        }
    }

}