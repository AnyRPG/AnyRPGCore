using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class ActionItem : Item, IUseable {

        [Tooltip("The name of the action to perform")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimatedAction))]
        protected string actionName = string.Empty;

        [Tooltip("Cooldown before this item can be used again")]
        [SerializeField]
        protected float coolDown = 0f;

        //[SerializeField]
        protected AnimatedActionProperties actionProperties = null;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public override float CoolDown { get => coolDown; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool Use() {
            //Debug.Log("CastableItem.Use()");
            /*
            if (ability == null) {
                Debug.LogError(DisplayName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return false;
            }
            */
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (playerManager.MyCharacter.AbilityManager.ControlLocked) {
                return false;
            }

            // perform action
            playerManager.UnitController.UnitActionManager.BeginAction(actionProperties);

            BeginAbilityCoolDown(playerManager.MyCharacter, coolDown);
            Remove();

            return returnValue;
        }

        public virtual void BeginAbilityCoolDown(IAbilityCaster sourceCharacter, float animationLength = -1f) {
            if (sourceCharacter != null) {
                sourceCharacter.AbilityManager.BeginActionCoolDown(this, animationLength);
            }
        }

        public override bool HadSpecialIcon(ActionButton actionButton) {
            //if (ability != null) {
                //UpdateActionButtonVisual(actionButton);
                return true;
            //}
            //return base.HadSpecialIcon(actionButton);
        }

        public override Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".CastableItem.ChooseMonitorCoroutine()");
            if (coolDown == 0f) {
                return null;
            }
            return systemAbilityController.StartCoroutine(actionButton.MonitorCooldown(this));
        }

        public override string GetSummary(ItemQuality usedItemQuality) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetSummary(usedItemQuality) + GetCastableInformation() + GetCooldownString();
        }

        public virtual string GetCastableInformation() {
            return string.Empty;
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
            if (playerManager?.MyCharacter?.CharacterAbilityManager != null
                && playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                float dictionaryCooldown = 0f;
                if (playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    dictionaryCooldown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                }
                coolDownString = "\n\nCooldown Remaining: " + SystemAbilityController.GetTimeText(dictionaryCooldown);
            }
            return coolDownString;
        }

        public override void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual()");
            // set cooldown icon on abilities that don't have enough resources to cast
            base.UpdateActionButtonVisual(actionButton);

            if (playerManager.MyCharacter.AbilityManager.ControlLocked) {
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            if (playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
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
                if (playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    remainingAbilityCoolDown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                    initialCoolDown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].InitialCoolDown;
                } else {
                    initialCoolDown = coolDown;
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

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);


            if (actionName != null) {
                AnimatedAction tmpAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
                if (tmpAction != null) {
                    actionProperties = tmpAction.ActionProperties;
                } else {
                    Debug.LogError("ActionItem.SetupScriptableObjects(): Could not find action : " + actionName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


        }


    }

}