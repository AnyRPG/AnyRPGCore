using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemPanelButton : ConfiguredMonoBehaviour, IDescribable, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private Sprite icon = null;

        [SerializeField]
        private string optionName = string.Empty;

        [SerializeField]
        private string description = string.Empty;

        [SerializeField]
        private Image menuImage = null;

        protected RectTransform tooltipTransform = null;

        // game manager references
        private UIManager uIManager = null;

        public Sprite Icon {
            get => icon;
            set {
                icon = value;
                if (menuImage != null) {
                    menuImage.sprite = icon;
                }
            }
        }

        public string DisplayName { get => optionName; }
        public string Description { get => description; }

        public void SetTooltipTransform(RectTransform rectTransform) {
            tooltipTransform = rectTransform;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public string GetSummary() {
            // cyan
            return string.Format("<color=#00FFFF>{0}</color>\n{1}", optionName, GetDescription());
        }

        public string GetDescription() {
            return description;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            //uIManager.ShowToolTip(transform.position, this);
            uIManager.ShowGamepadTooltip(tooltipTransform, transform, this, "");
        }

        public void OnPointerExit(PointerEventData eventData) {
            uIManager.HideToolTip();
        }

    }

}