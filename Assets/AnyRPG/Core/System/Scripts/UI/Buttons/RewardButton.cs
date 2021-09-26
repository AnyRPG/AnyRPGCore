using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class RewardButton : DescribableIcon, IClickable, IPointerClickHandler {

        public event System.Action<RewardButton> OnAttempSelect = delegate { };

        [SerializeField]
        protected Image highlightIcon;

        [SerializeField]
        protected bool limitReached = false;

        // is this reward button currently highlighted
        protected bool selected = false;

        public bool Selected { get => selected; set => selected = value; }
        public Image HighlightIcon { get => highlightIcon; set => highlightIcon = value; }
        public bool LimitReached { get => limitReached; set => limitReached = value; }

        /// <summary>
        /// UPdates the visual representation of the describablebutton
        /// </summary>
        public override void UpdateVisual() {
            //Debug.Log("RewardButton.UpdateVisual()");
            base.UpdateVisual();


            if (selected == true) {
                highlightIcon.sprite = null;
                highlightIcon.color = new Color32(255, 255, 255, 180);
            } else {
                highlightIcon.sprite = null;
                highlightIcon.color = new Color32(0, 0, 0, 0);
            }
        }

        public void Unselect() {
            //Debug.Log("RewardButton: Unselect()");
            selected = false;
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("RewardButton: OnPointerClick()");

            if (selected) {
                selected = false;
                //Debug.Log("RewardButton: OnPointerClick() set selected to false");
            } else {
                selected = true;
                //Debug.Log("RewardButton: OnPointerClick() set selected to true");
            }
            OnAttempSelect(this);
            UpdateVisual();
        }

    }

}