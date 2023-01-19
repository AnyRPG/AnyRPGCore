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
        public event System.Action<CharacterStats> OnBeforeDie = delegate { };
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
        public event System.Action<BaseCharacter> OnCastComplete = delegate { };
        public event System.Action<BaseCharacter> OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };
        public event System.Action<string> OnMessageFeed = delegate { };
        public event System.Action OnStartInteract = delegate { };
        public event System.Action OnStopInteract = delegate { };
        public event System.Action<InteractableOptionComponent> OnStartInteractWithOption = delegate { };
        public event System.Action<InteractableOptionComponent> OnStopInteractWithOption = delegate { };

        // unit controller of controlling unit
        private UnitController unitController;

        public UnitEventController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        #region EventNotifications

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
            //Debug.Log(unitController.gameObject.name + ".UnitEventController.NotifyOnAggroTarget()");
            OnAggroTarget();
        }

        public void NotifyOnAttack() {
            Debug.Log(unitController.gameObject.name + ".UnitEventController.NotifyOnAttack()");
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

        public void NotifyOnBeforeDie(CharacterStats characterStats) {
            unitController.UnitComponentController.StopMovementSound();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnBeforeDie(characterStats);

        }

        public void NotifyOnAfterDie(CharacterStats characterStats) {
            if (unitController.GetCurrentInteractables().Count == 0) {
                unitController.RevertMaterialChange();
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
            //Debug.Log(gameObject.name + ".NotifyOnStatusEffectAdd()");
            OnStatusEffectAdd(statusEffectNode);
        }
        public void NotifyOnCastTimeChanged(IAbilityCaster source, BaseAbilityProperties baseAbility, float castPercent) {
            OnCastTimeChanged(source, baseAbility, castPercent);
        }
        public void NotifyOnCastComplete(BaseCharacter baseCharacter) {
            OnCastComplete(baseCharacter);
        }
        public void NotifyOnCastCancel(BaseCharacter baseCharacter) {
            OnCastCancel(baseCharacter);
        }
        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            OnActivateMountedState(mountUnitController);
        }
        public void NotifyOnDeActivateMountedState() {
            OnDeActivateMountedState();
        }
        public void NotifyOnMessageFeed(string message) {
            //Debug.Log(gameObject.name + ".NotifyOnMessageFeed(" + message + ")");
            OnMessageFeed(message);
        }

        #endregion


    }

}