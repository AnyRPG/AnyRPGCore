using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class HighlightButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

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

        public TextMeshProUGUI MyText { get => text; }

        public void Awake() {
            //Debug.Log(gameObject.name + ".HightlightButton.Awake()");

            if (SystemConfigurationManager.Instance != null) {
                if (highlightImage != null) {
                    highlightColor = SystemConfigurationManager.Instance.DefaultUIColor;
                }
                if (highlightButton != null) {
                    Image highlightButtonImage = highlightButton.GetComponent<Image>();
                    if (highlightButtonImage != null) {
                        highlightButtonImage.color = SystemConfigurationManager.Instance.DefaultUISolidColor;
                    }
                }
            }
            DeSelect();
        }

        public void Start() {
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
            AudioManager.Instance.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            AudioManager.Instance.PlayUIClickSound();
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