using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class RewardButton : DescribableIcon, IClickable {

        public event System.Action<RewardButton> OnAttempSelect = delegate { };

        [SerializeField]
        protected Image highlightIcon;

        // is this reward button currently highlighted
        protected bool chosen = false;

        protected bool chooseable = false;

        protected CloseableWindowContents closeableWindowContents = null;

        public bool Chosen { get => chosen; set => chosen = value; }
        public Image HighlightIcon { get => highlightIcon; set => highlightIcon = value; }

        public void SetOptions(CloseableWindowContents closeableWindowContents, bool chooseable) {
            this.closeableWindowContents = closeableWindowContents;
            this.chooseable = chooseable;
        }

        /// <summary>
        /// UPdates the visual representation of the describablebutton
        /// </summary>
        public override void UpdateVisual() {
            //Debug.Log("RewardButton.UpdateVisual()");
            base.UpdateVisual();


            if (chosen == true) {
                highlightIcon.sprite = null;
                highlightIcon.color = new Color32(255, 255, 255, 180);
            } else {
                highlightIcon.sprite = null;
                highlightIcon.color = new Color32(0, 0, 0, 0);
            }
        }

        public void Unselect() {
            //Debug.Log("RewardButton: Unselect()");
            chosen = false;
        }

        public override void OnPointerClick(PointerEventData eventData) {
            Debug.Log("RewardButton: OnPointerClick()");
            base.OnPointerClick(eventData);

            ToggleChosen();
        }

        private void ToggleChosen() {
            if (chosen) {
                chosen = false;
                //Debug.Log("RewardButton: OnPointerClick() set selected to false");
            } else {
                chosen = true;
                //Debug.Log("RewardButton: OnPointerClick() set selected to true");
            }
            OnAttempSelect(this);
            UpdateVisual();
        }

        
        public override void Accept() {
            base.Accept();
            if (chooseable == true) {
                ToggleChosen();
            }
        }

        public override void Select() {
            base.Select();
            ShowContextInfo();
        }

        public override void DeSelect() {
            base.DeSelect();
            uIManager.HideToolTip();
            if (owner != null) {
                owner.HideControllerHints();
            }
        }

        public void ShowContextInfo() {
            ShowGamepadTooltip();
            if (chooseable == true) {
                owner.SetControllerHints("Choose", "", "", "");
            }
        }

        public void ShowGamepadTooltip() {
            //Rect panelRect = RectTransformToScreenSpace((BagPanel.ContentArea as RectTransform));
            uIManager.ShowGamepadTooltip(closeableWindowContents.transform as RectTransform, transform, describable, "");
        }

    }

}