using AnyRPG;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNodeScript : NavigableElement {

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

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;

        public TextMeshProUGUI Timer { get => timer; }
        public TextMeshProUGUI StackCount { get => stackCount; set => stackCount = value; }
        public Image Icon { get => icon; set => icon = value; }
        public bool UseTimerText { get => useTimerText; set => useTimerText = value; }
        public bool UseStackText { get => useStackText; set => useStackText = value; }

        public void Initialize(StatusEffectNode statusEffectNode, CharacterUnit target, SystemGameManager systemGameManager) {
            //Debug.Log("StatusEffectNodeScript.Initialize()");
            icon.sprite = statusEffectNode.StatusEffect.Icon;
            this.statusEffectNode = statusEffectNode;
            this.target = target;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerClick()");
            base.OnPointerClick(eventData);
            if (eventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        public void HandleRightClick() {
            //Debug.Log("StatusEffectNodeScript.HandleRightClick()");
            if (target == playerManager.UnitController?.CharacterUnit) {
                if (statusEffectNode != null && statusEffectNode.StatusEffect.StatusEffectAlignment != StatusEffectAlignment.Harmful) {
                    //Debug.Log("StatusEffectNodeScript.HandleRightClick(): statusEffect is not null, destroying");
                    statusEffectNode.CancelStatusEffect();
                }
                uIManager.HideToolTip();
            }
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerEnter()");
            base.OnPointerEnter(eventData);
            // show tooltip
            uIManager.ShowToolTip(transform.position, statusEffectNode.StatusEffect);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            //Debug.Log("StatusEffectNodeScript.OnPointerExit()");
            base.OnPointerExit(eventData);
            // hide tooltip
            uIManager.HideToolTip();
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
                //Debug.Log("Setting coolDownIcon to match Icon");
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

        public void OnSendObjectToPool() {
            //Debug.Log("StatusEffectNodeScript.OnSendObjectToPool()");
            statusEffectNode = null;
            target = null;
        }

        public override void Select() {
            Debug.Log("StatusEffectNodeScript.Select()");
            base.Select();
            if (owner != null && statusEffectNode != null && statusEffectNode.StatusEffect.StatusEffectAlignment != StatusEffectAlignment.Harmful) {
                //Debug.Log("StatusEffectNodeScript.HandleRightClick(): statusEffect is not null, destroying");
                owner.SetControllerHints("", "Cancel Effect", "", "");
            }
        }

        public override void DeSelect() {
            Debug.Log("StatusEffectNodeScript.DeSelect()");
            base.DeSelect();
            if (owner != null) {
                owner.HideControllerHints();
            }
        }

        public override void JoystickButton2() {
            Debug.Log("StatusEffectNodeScript.JoystickButton2()");
            base.JoystickButton2();
            HandleRightClick();
        }


        /*
        void FixedUpdate()
        {

        }
        */
    }

}