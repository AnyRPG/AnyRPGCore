using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    public class UnitEventController : ConfiguredClass {

        public event System.Action<Interactable> OnSetTarget = delegate { };
        public event System.Action<Interactable> OnClearTarget = delegate { };
        public event System.Action OnAggroTarget = delegate { };
        public event System.Action OnAttack = delegate { };
        public event System.Action OnTakeDamage = delegate { };
        public event System.Action OnTakeFallDamage = delegate { };
        public event System.Action OnKillTarget = delegate { };
        public event System.Action OnInteract = delegate { };
        public event System.Action OnManualMovement = delegate { };
        public event System.Action OnJump = delegate { };
        public event System.Action OnReputationChange = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<UnitController> OnBeforeDie = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };
        public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<Faction, Faction> OnFactionChange = delegate { };
        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action<IAbilityCaster, BaseAbilityProperties, float> OnCastTimeChanged = delegate { };
        public event System.Action OnCastComplete = delegate { };
        public event System.Action OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };
        public event System.Action<string> OnMessageFeed = delegate { };
        public event System.Action OnStartInteract = delegate { };
        public event System.Action OnStopInteract = delegate { };
        public event System.Action<InteractableOptionComponent> OnStartInteractWithOption = delegate { };
        public event System.Action<InteractableOptionComponent> OnStopInteractWithOption = delegate { };
        public event System.Action OnDropCombat = delegate { };
        public event System.Action<UnitController> OnBeginCastOnEnemy = delegate { };
        public event System.Action<float, float, float, float> OnCalculateRunSpeed = delegate { };
        public event System.Action<AbilityEffectContext> OnImmuneToEffect = delegate { };
        public event System.Action<int> OnGainXP = delegate { };
        public event System.Action<PowerResource, int> OnRecoverResource = delegate { };
        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action OnEnterStealth = delegate { };
        public event System.Action OnLeaveStealth = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action OnReviveBegin = delegate { };
        public event System.Action OnCombatUpdate = delegate { };
        public event System.Action<Interactable> OnEnterCombat = delegate { };
        public event System.Action<UnitController, Interactable> OnHitEvent = delegate { };
        public event System.Action<Interactable, AbilityEffectContext> OnReceiveCombatMiss = delegate { };
        public event System.Action<UnitController, float> OnKillEvent = delegate { };
        public System.Action<Equipment, Equipment, int> OnEquipmentChanged = delegate { };
        public event System.Action<AnimatedAbilityProperties> OnAnimatedAbilityCheckFail = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };
        public event System.Action OnBeginAbilityCoolDown = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<BaseAbilityProperties> OnActivateTargetingMode = delegate { };
        public event System.Action<BaseAbilityProperties> OnLearnAbility = delegate { };
        public event System.Action<bool> OnUnlearnAbility = delegate { };
        public event System.Action<BaseAbilityProperties> OnAttemptPerformAbility = delegate { };
        public event System.Action<string> OnMessageFeedMessage = delegate { };
        public event System.Action<BaseAbilityProperties> OnLearnedCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties> OnCombatCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties> OnStealthCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties, IAbilityCaster> OnPowerResourceCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties> OnPerformAbility = delegate { };
        public event System.Action<UnitController> OnDespawn = delegate { };
        //public event System.Action<BaseAbilityProperties, Interactable> OnTargetInAbilityRangeFail = delegate { };


        // unit controller of controlling unit
        private UnitController unitController;

        public UnitEventController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        #region EventNotifications

        public void NotifyOnDespawn(UnitController despawnController) {
            OnDespawn(despawnController);
        }

        public void NotifyOnPerformAbility(BaseAbilityProperties abilityProperties) {
            OnPerformAbility(abilityProperties);
        }

        public void NotifyOnPowerResourceCheckFail(BaseAbilityProperties abilityProperties, IAbilityCaster abilityCaster) {
            OnPowerResourceCheckFail(abilityProperties, abilityCaster);
        }

        public void NotifyOnStealthCheckFail(BaseAbilityProperties abilityProperties) {
            OnStealthCheckFail(abilityProperties);
        }

        public void NotifyOnCombatCheckFail(BaseAbilityProperties abilityProperties) {
            OnCombatCheckFail(abilityProperties);
        }

        public void NotifyOnLearnedCheckFail(BaseAbilityProperties abilityProperties) {
            OnLearnedCheckFail(abilityProperties);
        }

        public void NotifyOnMessageFeedMessage(string message) {
            OnMessageFeedMessage(message);
        }

        public void NotifyOnAttemptPerformAbility(BaseAbilityProperties abilityProperties) {
            OnAttemptPerformAbility(abilityProperties);
        }

        public void NotifyOnUnlearnAbility(bool updateActionBars) {
            OnUnlearnAbility(updateActionBars);
        }

        public void NotifyOnLearnAbility(BaseAbilityProperties abilityProperties) {
            OnLearnAbility(abilityProperties);
        }

        public void NotifyOnActivateTargetingMode(BaseAbilityProperties abilityProperties) {
            OnActivateTargetingMode(abilityProperties);
        }

        public void NotifyOnUnlearnAbilities() {
            OnUnlearnAbilities();
        }

        public void NotifyOnBeginAbilityCoolDown() {
            OnBeginAbilityCoolDown();
        }

        public void NotifyOnCombatMessage(string message) {
            OnCombatMessage(message);
        }

        public void NotifyOnAnimatedAbilityCheckFail(AnimatedAbilityProperties animatedAbilityProperties) {
            OnAnimatedAbilityCheckFail(animatedAbilityProperties);
        }

        public void NotifyOnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment, int slotIndex) {
            OnEquipmentChanged(newEquipment, oldEquipment, slotIndex);
        }

        public void NotifyOnKillEvent(UnitController sourceCharacter, float creditPercent) {
            OnKillEvent(sourceCharacter, creditPercent);
        }

        public void NotifyOnReceiveCombatMiss(Interactable target, AbilityEffectContext abilityEffectContext) {
            OnReceiveCombatMiss(target, abilityEffectContext);
        }

        public void NotifyOnHitEvent(UnitController source, Interactable target) {
            OnHitEvent(source, target);
        }

        public void NotifyOnEnterCombat(Interactable target) {
            OnEnterCombat(target);
        }

        public void NotifyOnCombatUpdate() {
            OnCombatUpdate();
        }

        public void NotifyOnReviveBegin() {
            OnReviveBegin();
        }

        public void NotifyOnStatChanged() {
            OnStatChanged();
        }

        public void NotifyOnEnterStealth() {
            OnEnterStealth();
        }

        public void NotifyOnLeaveStealth() {
            OnLeaveStealth();
        }

        public void NotifyOnPrimaryResourceAmountChanged(int maxAmount, int currentAmount) {
            OnPrimaryResourceAmountChanged(maxAmount, currentAmount);
        }

        public void NotifyOnRecoverResource(PowerResource powerResource, int amount) {
            OnRecoverResource(powerResource, amount);
        }

        public void NotifyOnGainXP(int xp) {
            OnGainXP(xp);
        }

        public void NotifyOnImmuneToEffect(AbilityEffectContext abilityEffectContext) {
            OnImmuneToEffect(abilityEffectContext);
        }

        public void NotifyOnCalculateRunSpeed(float oldRunSpeed, float currentRunSpeed, float oldSprintSpeed, float currentSprintSpeed) {
            OnCalculateRunSpeed(oldRunSpeed, currentRunSpeed, oldSprintSpeed, currentSprintSpeed);
        }

        public void NotifyOnBeginCastOnEnemy(UnitController unitController) {
            OnBeginCastOnEnemy(unitController);
        }

        public void NotifyOnDropCombat() {
            OnDropCombat();
        }

        public void NotifyOnStartInteract() {
            OnStartInteract();
        }

        public void NotifyOnStopInteract() {
            OnStopInteract();
        }

        public void NotifyOnStartInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            OnStartInteractWithOption(interactableOptionComponent);
        }

        public void NotifyOnStopInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            OnStopInteractWithOption(interactableOptionComponent);
        }

        public void NotifyOnAggroTarget() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAggroTarget()");
            OnAggroTarget();
        }

        public void NotifyOnAttack() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAttack()");
            OnAttack();
        }

        public void NotifyOnTakeDamage() {
            unitController.UnitAnimator.HandleTakeDamage();
            OnTakeDamage();
        }

        public void NotifyOnTakeFallDamage() {
            OnTakeFallDamage();
        }

        public void NotifyOnKillTarget() {
            OnKillTarget();
        }

        public void NotifyOnManualMovement() {
            OnManualMovement();
        }

        public void NotifyOnSetTarget(Interactable interactable) {
            OnSetTarget(interactable);
        }

        public void NotifyOnClearTarget(Interactable interactable) {
            OnClearTarget(interactable);
        }

        public void NotifyOnUnitDestroy(UnitProfile unitProfile) {
            OnUnitDestroy(unitProfile);
        }

        /*
        public void NotifyOnInteract() {
            OnInteract();
        }
        */

        public void NotifyOnJump() {
            OnJump();
        }

        public void NotifyOnCombatMiss() {
            unitController.UnitComponentController.PlayEffectSound(systemConfigurationManager.WeaponMissAudioClip);
        }

        public void NotifyOnReputationChange() {
            // minimap indicator can change color if reputation changed
            if (unitController.UnitControllerMode == UnitControllerMode.Preview) {
                return;
            }
            unitController.CharacterUnit.CallMiniMapStatusUpdateHandler();
            OnReputationChange();
            unitController.UnitComponentController.HighlightController.UpdateColors();
        }

        public void NotifyOnBeforeDie(UnitController targetUnitController) {
            unitController.UnitComponentController.StopMovementSound();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnBeforeDie(targetUnitController);

        }

        public void NotifyOnAfterDie(CharacterStats characterStats) {
            if (unitController.GetCurrentInteractables().Count == 0) {
                unitController.OutlineController.TurnOffOutline();
            }
        }

        public void NotifyOnReviveComplete() {
            unitController.FreezeRotation();
            unitController.InitializeNamePlate();
            unitController.CharacterUnit.HandleReviveComplete();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnReviveComplete();
        }

        public void NotifyOnLevelChanged(int newLevel) {
            OnLevelChanged(newLevel);
        }

        public void NotifyOnUnitTypeChange(UnitType newUnitType, UnitType oldUnitType) {
            OnUnitTypeChange(newUnitType, oldUnitType);
        }
        public void NotifyOnRaceChange(CharacterRace newCharacterRace, CharacterRace oldCharacterRace) {
            OnRaceChange(newCharacterRace, oldCharacterRace);
        }
        public void NotifyOnClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(newCharacterClass, oldCharacterClass);
        }
        public void NotifyOnSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(newClassSpecialization, oldClassSpecialization);
        }
        public void NotifyOnFactionChange(Faction newFaction, Faction oldFaction) {
            OnFactionChange(newFaction, oldFaction);
        }
        public void NotifyOnNameChange(string newName) {
            OnNameChange(newName);
        }
        public void NotifyOnTitleChange(string newTitle) {
            OnTitleChange(newTitle);
        }
        public void NotifyOnResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            OnResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }
        public void NotifyOnStatusEffectAdd(StatusEffectNode statusEffectNode) {
            //Debug.Log($"{gameObject.name}.NotifyOnStatusEffectAdd()");
            OnStatusEffectAdd(statusEffectNode);
        }
        public void NotifyOnCastTimeChanged(IAbilityCaster source, BaseAbilityProperties baseAbility, float castPercent) {
            OnCastTimeChanged(source, baseAbility, castPercent);
        }
        public void NotifyOnCastComplete() {
            OnCastComplete();
        }
        public void NotifyOnCastCancel() {
            OnCastCancel();
        }
        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            OnActivateMountedState(mountUnitController);
        }
        public void NotifyOnDeActivateMountedState() {
            OnDeActivateMountedState();
        }
        public void NotifyOnMessageFeed(string message) {
            //Debug.Log($"{gameObject.name}.NotifyOnMessageFeed(" + message + ")");
            OnMessageFeed(message);
        }

        #endregion


    }

}