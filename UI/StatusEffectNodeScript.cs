using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNodeScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Image coolDownIcon = null;

        [SerializeField]
        private bool useTimerText = false;

        [SerializeField]
        private TextMeshProUGUI timer = null;

        [SerializeField]
        private bool useStackText = false;

        [SerializeField]
        private TextMeshProUGUI stackCount = null;

        private StatusEffectNode statusEffectNode = null;

        private CharacterUnit target = null;

        public TextMeshProUGUI MyTimer { get => timer; }
        public TextMeshProUGUI MyStackCount { get => stackCount; set => stackCount = value; }
        public Image MyIcon { get => icon; set => icon = value; }
        public bool MyUseTimerText { get => useTimerText; set => useTimerText = value; }
        public bool MyUseStackText { get => useStackText; set => useStackText = value; }

        public void Initialize(StatusEffectNode statusEffectNode, CharacterUnit target) {
            //Debug.Log("StatusEffectNodeScript.Initialize()");
            icon.sprite = statusEffectNode.StatusEffect.MyIcon;
            this.statusEffectNode = statusEffectNode;
            this.target = target;
            statusEffectNode.StatusEffect.SetStatusNode(this);
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerClick()");

            if (eventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        public void HandleRightClick() {
            //Debug.Log("StatusEffectNodeScript.HandleRightClick()");
            if (statusEffectNode != null && statusEffectNode.StatusEffect.MyStatusEffectAlignment != StatusEffectAlignment.Harmful) {
                //Debug.Log("StatusEffectNodeScript.HandleRightClick(): statusEffect is not null, destroying");
                statusEffectNode.CancelStatusEffect();
            }
            UIManager.MyInstance.HideToolTip();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerEnter()");

            // show tooltip
            UIManager.MyInstance.ShowToolTip(transform.position, statusEffectNode.StatusEffect);
        }

        public void OnPointerExit(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerExit()");

            // hide tooltip
            UIManager.MyInstance.HideToolTip();
        }

        public void UpdateFillIcon(float fillAmount) {
            //Debug.Log("StatusEffectNodeScript.UpdateFillIcon(" + fillAmount + ")");
            float usedFillAmount = fillAmount;
            if (usedFillAmount != 0) {
                usedFillAmount = (usedFillAmount * -1f) + 1f;
            }

            if (usedFillAmount == 0f && coolDownIcon.enabled == true) {
                coolDownIcon.enabled = false;
                return;
            }
            if (coolDownIcon.isActiveAndEnabled == false) {
                coolDownIcon.enabled = true;
            }
            if (coolDownIcon.sprite != MyIcon.sprite) {
                //Debug.Log("Setting coolDownIcon to match MyIcon");
                coolDownIcon.sprite = MyIcon.sprite;
                coolDownIcon.color = new Color32(0, 0, 0, 150);
                coolDownIcon.fillMethod = Image.FillMethod.Radial360;
                //coolDownIcon.fillOrigin = Image.Origin360.Top;
                coolDownIcon.fillClockwise = true;
            }
            if (!(coolDownIcon.fillAmount == usedFillAmount)) {
                coolDownIcon.fillAmount = usedFillAmount;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
        }

        public void OnPointerUp(PointerEventData eventData) {
        }


        /*
        void FixedUpdate()
        {

        }
        */
    }

}