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
        protected bool chosen = false;

        public bool Chosen { get => chosen; set => chosen = value; }
        public Image HighlightIcon { get => highlightIcon; set => highlightIcon = value; }
        public bool LimitReached { get => limitReached; set => limitReached = value; }

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
            //Debug.Log("RewardButton: OnPointerClick()");
            base.OnPointerClick(eventData);

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

    }

}