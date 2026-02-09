using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InstantiatedActionItem : InstantiatedItem, IUseable {

        private ActionItem actionItem = null;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public override float CoolDown { get => actionItem.CoolDown; }

        public InstantiatedActionItem(SystemGameManager systemGameManager, long instanceId, ActionItem actionItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, actionItem, itemQuality) {
            this.actionItem = actionItem;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool Use(UnitController sourceUnitController) {

            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (sourceUnitController.AbilityManager.ControlLocked) {
                return false;
            }
            if (sourceUnitController.AbilityManager.IsOnCoolDown(ResourceName)) {
                sourceUnitController.WriteMessageFeedMessage("Item is on cooldown");
                return false;
            }

            sourceUnitController.UnitActionManager.BeginAction(actionItem.AnimatedAction);

            BeginAbilityCoolDown(sourceUnitController, actionItem.CoolDown);
            Remove();

            return returnValue;
        }

        public virtual void BeginAbilityCoolDown(IAbilityCaster sourceCharacter, float animationLength = -1f) {
            if (sourceCharacter != null) {
                sourceCharacter.AbilityManager.BeginActionCoolDown(this, animationLength);
            }
        }

        public override Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".CastableItem.ChooseMonitorCoroutine()");

            /*
            // testing - disable this and always monitor because of the inherent 1 second cooldown to prevent accidental spamming of action items
            if (coolDown == 0f) {
                return null;
            }
            */
            int count = playerManager.UnitController.CharacterInventoryManager.GetUseableCount(this);
            if (count == 0) {
                // don't start monitoring if there are no charges
                return null;
            }
            return systemAbilityController.StartCoroutine(actionButton.MonitorCooldown(this));
        }

        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + actionItem.GetActionItemDescription();
        }


        /*
        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetCastableInformation() + GetCooldownString();
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
        */
        public override void ProcessUpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".ActionItem.UpdateActionButtonVisual()");

            if (playerManager.UnitController.AbilityManager.ControlLocked) {
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            if (playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(ResourceName)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): Ability is on cooldown");
                if (actionButton.CoolDownIcon.isActiveAndEnabled != true) {
                    //Debug.Log("ActionButton.UpdateVisual(): coolDownIcon is not enabled: " + (useable == null ? "null" : useable.DisplayName));
                    actionButton.CoolDownIcon.enabled = true;
                }
                if (actionButton.CoolDownIcon.sprite != actionButton.Icon.sprite) {
                    actionButton.CoolDownIcon.sprite = actionButton.Icon.sprite;
                    actionButton.CoolDownIcon.color = new Color32(0, 0, 0, 230);
                    actionButton.CoolDownIcon.fillMethod = Image.FillMethod.Radial360;
                    actionButton.CoolDownIcon.fillClockwise = false;
                }
                float remainingAbilityCoolDown = 0f;
                float initialCoolDown = 0f;
                if (playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(ResourceName)) {
                    remainingAbilityCoolDown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[ResourceName].RemainingCoolDown;
                    initialCoolDown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[ResourceName].InitialCoolDown;
                } else {
                    initialCoolDown = actionItem.CoolDown;
                }
                float fillAmount = Mathf.Max(remainingAbilityCoolDown / initialCoolDown);
                //Debug.Log("Setting fill amount to: " + fillAmount);
                if (actionButton.CoolDownIcon.fillAmount != fillAmount) {
                    actionButton.CoolDownIcon.fillAmount = fillAmount;
                }
            } else {
                actionButton.DisableCoolDownIcon();
            }
        }


    }

}