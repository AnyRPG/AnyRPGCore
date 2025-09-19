using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class ActionItem : Item {

        [Header("Action Item")]

        [Tooltip("The Use: hint that will appear in the tooltip")]
        [TextArea(5, 10)]
        [SerializeField]
        private string toolTip = string.Empty;

        [Tooltip("Cooldown before this item can be used again")]
        [SerializeField]
        protected float coolDown = 0f;

        [Header("Action")]

        [Tooltip("The name of the action to perform")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimatedAction))]
        protected string actionName = string.Empty;

        protected AnimatedAction animatedAction = null;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public float CoolDown { get => coolDown; }
        public AnimatedAction AnimatedAction { get => animatedAction; set => animatedAction = value; }
        public string ToolTip { get => toolTip; set => toolTip = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool HadSpecialIcon(ActionButton actionButton) {
            //if (ability != null) {
                //UpdateActionButtonVisual(actionButton);
                return true;
            //}
            //return base.HadSpecialIcon(actionButton);
        }

        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetActionItemDescription();
        }

        public string GetActionItemDescription() {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return GetCastableInformation() + GetCooldownString();
        }

        public virtual string GetCastableInformation() {
            string returnString = string.Empty;
            if (toolTip != string.Empty) {
                returnString += string.Format("\n\n<color=green>Use: {0}</color>", toolTip);
            }
            return returnString;
        }

        public string GetCooldownString() {
            string coolDownString = string.Empty;
            if (coolDown != 0f) {
                coolDownString = GetCooldownTimeString();
            }
            return coolDownString;
        }

        public string GetCooldownTimeString() {
            string coolDownString = string.Empty;
            if (playerManager?.UnitController?.CharacterAbilityManager != null
                && playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(ResourceName)) {
                float dictionaryCooldown = 0f;
                if (playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(ResourceName)) {
                    dictionaryCooldown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[ResourceName].RemainingCoolDown;
                }
                coolDownString = "\n\nCooldown Remaining: " + SystemAbilityController.GetTimeText(dictionaryCooldown);
            }
            return coolDownString;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);


            if (actionName != string.Empty) {
                animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
                if (animatedAction == null) {
                    Debug.LogError("ActionItem.SetupScriptableObjects(): Could not find action : " + actionName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }


        }


    }

}