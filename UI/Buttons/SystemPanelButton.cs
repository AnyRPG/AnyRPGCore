using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemPanelButton : MonoBehaviour, IDescribable, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private Sprite icon = null;

        [SerializeField]
        private string optionName = string.Empty;

        [SerializeField]
        private string description = string.Empty;

        [SerializeField]
        private Image menuImage = null;

        public Sprite MyIcon {
            get => icon;
            set {
                icon = value;
                if (menuImage != null) {
                    menuImage.sprite = icon;
                }
            }
        }

        public string MyName { get => optionName; }

        private void Awake() {
        }

        public string GetDescription() {
            return string.Format("<color=cyan>{0}</color>\n{1}", optionName, GetSummary());
        }

        public string GetSummary() {
            return description;
        }

        public void OnPointerEnter(PointerEventData eventData) {

            UIManager.MyInstance.ShowToolTip(transform.position, this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            UIManager.MyInstance.HideToolTip();
        }

    }

}