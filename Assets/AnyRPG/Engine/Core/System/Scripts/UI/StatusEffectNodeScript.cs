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

        private StatusEffectPanelController statusEffectPanelController = null;
        private StatusEffectNode statusEffectNode = null;
        private CharacterUnit target = null;

        public TextMeshProUGUI Timer { get => timer; }
        public TextMeshProUGUI StackCount { get => stackCount; set => stackCount = value; }
        public Image Icon { get => icon; set => icon = value; }
        public bool UseTimerText { get => useTimerText; set => useTimerText = value; }
        public bool UseStackText { get => useStackText; set => useStackText = value; }

        public void Initialize(StatusEffectPanelController statusEffectPanelController, StatusEffectNode statusEffectNode, CharacterUnit target) {
            //Debug.Log("StatusEffectNodeScript.Initialize()");
            icon.sprite = statusEffectNode.StatusEffect.Icon;
            this.statusEffectPanelController = statusEffectPanelController;
            this.statusEffectNode = statusEffectNode;
            this.target = target;
            statusEffectNode.SetStatusNode(this);
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerClick()");

            if (eventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        public void HandleRightClick() {
            //Debug.Log("StatusEffectNodeScript.HandleRightClick()");
            if (statusEffectNode != null && statusEffectNode.StatusEffect.StatusEffectAlignment != StatusEffectAlignment.Harmful) {
                //Debug.Log("StatusEffectNodeScript.HandleRightClick(): statusEffect is not null, destroying");
                statusEffectNode.CancelStatusEffect();
            }
            SystemGameManager.Instance.UIManager.HideToolTip();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerEnter()");

            // show tooltip
            SystemGameManager.Instance.UIManager.ShowToolTip(transform.position, statusEffectNode.StatusEffect);
        }

        public void OnPointerExit(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerExit()");

            // hide tooltip
            SystemGameManager.Instance.UIManager.HideToolTip();
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
            if (coolDownIcon.sprite != Icon.sprite) {
                //Debug.Log("Setting coolDownIcon to match MyIcon");
                coolDownIcon.sprite = Icon.sprite;
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

        public void OnSendObjectToPool() {
            //Debug.Log("StatusEffectNodeScript.OnSendObjectToPool()");
            statusEffectPanelController.ClearStatusEffectNodeScript(this);
            statusEffectPanelController = null;
            statusEffectNode = null;
            target = null;
        }


        /*
        void FixedUpdate()
        {

        }
        */
    }

}