using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnyRPG {

    public class UnitEventController : ConfiguredClass {

        public event System.Action<Interactable> OnSetTarget = delegate { };
        public event System.Action<Interactable> OnClearTarget = delegate { };
        public event System.Action OnAggroTarget = delegate { };
        public event System.Action OnAttack = delegate { };
        public event System.Action<IAbilityCaster, UnitController, int, CombatTextType, CombatMagnitude, string, AbilityEffectContext> OnTakeDamage = delegate { };
        public event System.Action OnTakeFallDamage = delegate { };
        public event System.Action OnKillTarget = delegate { };
        public event System.Action OnInteract = delegate { };
        public event System.Action OnMovement = delegate { };
        public event System.Action OnManualMovement = delegate { };
        public event System.Action OnJump = delegate { };
        public event System.Action<UnitController> OnReputationChange = delegate { };
        public event System.Action<UnitController> OnReviveComplete = delegate { };
        public event System.Action<UnitController> OnBeforeDie = delegate { };
        public event System.Action<CharacterStats> OnAfterDie = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };
        public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        public event System.Action<UnitController, CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<UnitController, ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<Faction, Faction> OnFactionChange = delegate { };
        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<UnitController, StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action<string> OnAddStatusEffectStack = delegate { };
        public event System.Action<StatusEffectProperties> OnRequestCancelStatusEffect = delegate { };
        public event System.Action<StatusEffectProperties> OnCancelStatusEffect = delegate { };
        public event System.Action<IAbilityCaster, AbilityProperties, float> OnCastTimeChanged = delegate { };
        public event System.Action OnCastComplete = delegate { };
        public event System.Action OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeactivateMountedState = delegate { };
        public event System.Action OnStartInteract = delegate { };
        public event System.Action OnStopInteract = delegate { };
        public event System.Action<UnitController, InteractableOptionComponent, int, int> OnStartInteractWithOption = delegate { };
        public event System.Action<UnitController, InteractableOptionComponent> OnCompleteInteractWithOption = delegate { };
        public event System.Action<InteractableOptionComponent> OnStopInteractWithOption = delegate { };
        public event System.Action OnDropCombat = delegate { };
        public event System.Action<UnitController> OnBeginCastOnEnemy = delegate { };
        public event System.Action<float, float, float, float> OnCalculateRunSpeed = delegate { };
        public event System.Action<AbilityEffectContext> OnImmuneToEffect = delegate { };
        public event System.Action<UnitController, int, int> OnGainXP = delegate { };
        public event System.Action<PowerResource, int, CombatMagnitude, AbilityEffectContext> OnRecoverResource = delegate { };
        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action OnEnterStealth = delegate { };
        public event System.Action OnLeaveStealth = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action<float> OnReviveBegin = delegate { };
        //public event System.Action OnCombatUpdate = delegate { };
        public event System.Action<Interactable> OnEnterCombat = delegate { };
        public event System.Action<UnitController, Interactable> OnHitEvent = delegate { };
        public event System.Action<Interactable, AbilityEffectContext> OnReceiveCombatMiss = delegate { };
        public event System.Action<UnitController, UnitController, float> OnKillEvent = delegate { };
        //public event System.Action<InstantiatedEquipment, InstantiatedEquipment, int> OnEquipmentChanged = delegate { };
        public event System.Action<AbilityProperties> OnAbilityActionCheckFail = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };
        public event System.Action<string, bool> OnBeginAction = delegate { };
        public event System.Action<AbilityProperties, Interactable, bool> OnBeginAbility = delegate { };
        public event System.Action<AbilityProperties, float> OnBeginAbilityCoolDown = delegate { };
        public event System.Action<InstantiatedActionItem, float> OnBeginActionCoolDown = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<AbilityProperties> OnActivateTargetingMode = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnLearnAbility = delegate { };
        public event System.Action<AbilityProperties> OnUnlearnAbility = delegate { };
        public event System.Action<AbilityProperties> OnAttemptPerformAbility = delegate { };
        //public event System.Action<UnitController, string> OnMessageFeedMessage = delegate { };
        public event System.Action<AbilityProperties> OnLearnedCheckFail = delegate { };
        public event System.Action<AbilityProperties> OnCombatCheckFail = delegate { };
        public event System.Action<AbilityProperties> OnStealthCheckFail = delegate { };
        public event System.Action<AbilityProperties, IAbilityCaster> OnPowerResourceCheckFail = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnPerformAbility = delegate { };
        public event System.Action<UnitController> OnDespawn = delegate { };
        public event System.Action<string> OnBeginChatMessage = delegate { };
        public event System.Action OnInitializeAnimator = delegate { };
        public event System.Action<string> OnAnimatorSetTrigger = delegate { };
        public event System.Action<string> OnAnimatorResetTrigger = delegate { };
        public event System.Action<string, AnimationClip> OnSetAnimationClipOverride = delegate { };
        public event System.Action<AnimatedAction> OnPerformAnimatedActionAnimation = delegate { };
        public event System.Action<AbilityProperties, int> OnPerformAbilityCastAnimation = delegate { };
        public event System.Action<AbilityProperties, int> OnPerformAbilityActionAnimation = delegate { };
        public event System.Action OnAnimatorClearAction = delegate { };
        public event System.Action OnAnimatorClearAbilityAction = delegate { };
        public event System.Action OnAnimatorClearAbilityCast = delegate { };
        public event System.Action<AbilityProperties, int> OnSpawnAbilityObjects = delegate { };
        public event System.Action OnDespawnAbilityObjects = delegate { };
        public event System.Action<AnimatedAction> OnSpawnActionObjects = delegate { };
        public event System.Action OnDespawnActionObjects = delegate { };
        public event System.Action<Interactable, Interactable, LengthEffectProperties, AbilityEffectContext> OnSpawnAbilityEffectPrefabs = delegate { };
        public event System.Action<Interactable, Interactable, ProjectileEffectProperties, AbilityEffectContext> OnSpawnProjectileEffectPrefabs = delegate { };
        public event System.Action<Interactable, Interactable, ChanneledEffectProperties, AbilityEffectContext> OnSpawnChanneledEffectPrefabs = delegate { };
        public event System.Action<UnitController, Interactable> OnEnterInteractableTrigger = delegate { };
        public event System.Action<UnitController, Interactable> OnExitInteractableTrigger = delegate { };
        public event System.Action<UnitController, Interactable> OnEnterInteractableRange = delegate { };
        public event System.Action<UnitController, Interactable> OnExitInteractableRange = delegate { };
        public event System.Action<UnitController, Quest> OnAcceptQuest = delegate { };
        public event System.Action<UnitController, Achievement> OnAcceptAchievement = delegate { };
        public event System.Action<UnitController, QuestBase> OnAbandonQuest = delegate { };
        public event System.Action<UnitController, Quest> OnTurnInQuest = delegate { };
        public event System.Action<UnitController, Quest> OnMarkQuestComplete = delegate { };
        public event System.Action<UnitController, Achievement> OnMarkAchievementComplete = delegate { };
        public event System.Action<UnitController, Quest> OnQuestObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Achievement> OnAchievementObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Skill> OnLearnSkill = delegate { };
        public event System.Action<UnitController, Skill> OnUnLearnSkill = delegate { };
        public event System.Action<string, string, string, int> OnSetQuestObjectiveCurrentAmount = delegate { };
        public event System.Action<string, string, string, int> OnSetAchievementObjectiveCurrentAmount = delegate { };
        public event System.Action<long, bool, int> OnPlaceInStack = delegate { };
        public event System.Action<long, bool, int> OnPlaceInEmpty = delegate { };
        public event System.Action<InstantiatedItem> OnGetNewInstantiatedItem = delegate { };
        public event System.Action<InstantiatedItem> OnRequestDeleteItem = delegate { };
        public event System.Action<InstantiatedItem> OnDeleteItem = delegate { };
        public event System.Action<InstantiatedEquipment, EquipmentSlotProfile> OnRequestEquipToSlot = delegate { };
        //public event System.Action<EquipmentSlotProfile> OnRequestUnequipFromList = delegate { };
        public event System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnAddEquipment = delegate { };
        public event System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnRemoveEquipment = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnAddItemToInventorySlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnAddItemToBankSlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnRemoveItemFromInventorySlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnRemoveItemFromBankSlot = delegate { };
        public event System.Action<InventorySlot, InventorySlot, bool, bool> OnRequestDropItemFromInventorySlot = delegate { };
        public event System.Action<CraftAbilityProperties> OnSetCraftAbility = delegate { };
        public event System.Action OnCraftItem = delegate { };
        public event System.Action OnRemoveFirstCraftingQueueItem = delegate { };
        public event System.Action OnClearCraftingQueue = delegate { };
        public event System.Action<Recipe> OnAddToCraftingQueue = delegate { };
        public event System.Action<int> OnRequestMoveFromBankToInventory = delegate { };
        public event System.Action<int> OnRequestMoveFromInventoryToBank = delegate { };
        public event System.Action<int> OnRequestUseItem = delegate { };
        public event System.Action OnRebuildModelAppearance = delegate { };
        public event System.Action<InstantiatedEquipment, InstantiatedEquipment> OnRequestSwapInventoryEquipment = delegate { };
        public event System.Action<InstantiatedEquipment, int> OnRequestUnequipToSlot = delegate { };
        public event System.Action<InstantiatedBag, InstantiatedBag> OnRequestSwapBags = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestUnequipBagToSlot = delegate { };
        public event System.Action<InstantiatedBag, bool> OnRequestUnequipBag = delegate { };
        public event System.Action<InstantiatedBag> OnRemoveBag = delegate { };
        public event System.Action<InstantiatedBag, BagNode> OnAddBag = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestMoveBag = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestAddBag = delegate { };
        public event System.Action<Vector3> OnSetGroundTarget = delegate { };
        public event System.Action<UnitController, int, CombatTextType, CombatMagnitude, AbilityEffectContext> OnReceiveCombatTextEvent = delegate {};
        public event System.Action<Recipe> OnLearnRecipe = delegate { };
        public event System.Action<Recipe> OnUnlearnRecipe = delegate { };
        public event System.Action<string, int> OnCurrencyChange = delegate { };
        public event System.Action<UnitProfile> OnAddPet = delegate { };
        public event System.Action<UnitProfile, UnitController> OnAddActivePet = delegate { };
        public event System.Action<UnitProfile> OnRemoveActivePet = delegate { };
        public event System.Action<Faction, float> OnSetReputationAmount = delegate { };
        public event System.Action<IUseable, int> OnSetGamepadActionButton = delegate { };
        public event System.Action<int> OnUnsetGamepadActionButton = delegate { };
        public event System.Action<IUseable, int> OnSetMouseActionButton = delegate { };
        public event System.Action<int> OnUnsetMouseActionButton = delegate { };
        public event System.Action<int, int> OnRequestMoveGamepadUseable = delegate { };
        public event System.Action<IUseable, int> OnRequestAssignGamepadUseable = delegate { };
        public event System.Action<int> OnRequestClearGamepadUseable = delegate { };
        public event System.Action<int, int> OnRequestMoveMouseUseable = delegate { };
        public event System.Action<IUseable, int> OnRequestAssignMouseUseable = delegate { };
        public event System.Action<int> OnRequestClearMouseUseable = delegate { };
        public event System.Action OnCharacterConfigured = delegate { };
        public event System.Action<float> OnInitiateGlobalCooldown = delegate { };
        public event System.Action OnActivateAutoAttack = delegate { };
        public event System.Action OnDeactivateAutoAttack = delegate { };
        public event System.Action OnStartFlying = delegate { };
        public event System.Action OnStopFlying = delegate { };
        public event System.Action<UnitController, UnitProfile> OnSetMountedState = delegate { };
        //public event System.Action<Transform, Vector3, Vector3> OnHandleMountUnitSpawn = delegate { };
        public event System.Action<Transform> OnSetParent = delegate { };
        public event System.Action OnUnsetParent = delegate { };
        public event System.Action OnMountUnitSpawn = delegate { };
        public event System.Action OnDespawnMountUnit = delegate { };
        public event System.Action<string> OnWriteMessageFeedMessage = delegate { };
        public event System.Action<UnitController, Item> OnItemCountChanged = delegate { };
        public event System.Action<UnitController, Dialog> OnDialogCompleted = delegate { };
        public event System.Action<Quest, int, long> OnInteractWithQuestStartItem = delegate { };
        public event System.Action<int, long, Quest> OnRequestAcceptQuestItemQuest = delegate { };
        public event System.Action<int, long, Quest, QuestRewardChoices> OnRequestCompleteQuestItemQuest = delegate { };
        public event System.Action OnSaveDataUpdated = delegate { };
        public event System.Action OnNameChangeFail = delegate { };
        public event System.Action<int> OnSetGroupId = delegate { };
        public event System.Action<int, string> OnSetGuildId = delegate { };

        //public event System.Action<BaseAbilityProperties, Interactable> OnTargetInAbilityRangeFail = delegate { };


        // unit controller of controlling unit
        private UnitController unitController;

        public void Configure(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        #region EventNotifications

        public void NotifyOnDespawn(UnitController despawnController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawn()");

            OnDespawn(despawnController);
        }

        public void NotifyOnPerformAbility(AbilityProperties abilityProperties) {
            OnPerformAbility(unitController, abilityProperties);
        }

        public void NotifyOnPowerResourceCheckFail(AbilityProperties abilityProperties, IAbilityCaster abilityCaster) {
            OnPowerResourceCheckFail(abilityProperties, abilityCaster);
        }

        public void NotifyOnStealthCheckFail(AbilityProperties abilityProperties) {
            OnStealthCheckFail(abilityProperties);
        }

        public void NotifyOnCombatCheckFail(AbilityProperties abilityProperties) {
            OnCombatCheckFail(abilityProperties);
        }

        public void NotifyOnLearnedCheckFail(AbilityProperties abilityProperties) {
            OnLearnedCheckFail(abilityProperties);
        }

        /*
        public void NotifyOnMessageFeedMessage(string message) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnMessageFeedMessage({message})");

            OnMessageFeedMessage(unitController, message);
        }
        */

        public void NotifyOnAttemptPerformAbility(AbilityProperties abilityProperties) {
            OnAttemptPerformAbility(abilityProperties);
        }

        public void NotifyOnUnlearnAbility(AbilityProperties abilityProperties) {
            OnUnlearnAbility(abilityProperties);
        }

        public void NotifyOnLearnAbility(AbilityProperties abilityProperties) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnLearnAbility({abilityProperties.ResourceName})");

            OnLearnAbility(unitController, abilityProperties);
        }

        public void NotifyOnActivateTargetingMode(AbilityProperties abilityProperties) {
            OnActivateTargetingMode(abilityProperties);
        }

        public void NotifyOnUnlearnAbilities() {
            OnUnlearnAbilities();
        }

        public void NotifyOnBeginAction(string actionName, bool playerInitiated) {
            OnBeginAction(actionName, playerInitiated);
        }

        public void NotifyOnBeginAbility(AbilityProperties baseAbility, Interactable target, bool playerInitiated) {
            OnBeginAbility(baseAbility, target, playerInitiated);
        }

        public void NotifyOnBeginAbilityCoolDown(AbilityProperties baseAbility, float coolDownLength) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnBeginAbilityCoolDown({baseAbility.ResourceName}, {coolDownLength})");

            OnBeginAbilityCoolDown(baseAbility, coolDownLength);
        }

        public void NotifyOnCombatMessage(string message) {
            OnCombatMessage(message);
        }

        public void NotifyOnAbilityActionCheckFail(AbilityProperties baseAbilityProperties) {
            OnAbilityActionCheckFail(baseAbilityProperties);
        }

        /*
        public void NotifyOnEquipmentChanged(InstantiatedEquipment newEquipment, InstantiatedEquipment oldEquipment, int slotIndex) {
            OnEquipmentChanged(newEquipment, oldEquipment, slotIndex);
        }
        */

        public void NotifyOnKillEvent(UnitController killedCharacter, float creditPercent) {
            OnKillEvent(unitController, killedCharacter, creditPercent);
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

        /*
        public void NotifyOnCombatUpdate() {
            OnCombatUpdate();
        }
        */

        public void NotifyOnReviveBegin(float reviveTime) {
            OnReviveBegin(reviveTime);
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

        public void NotifyOnRecoverResource(PowerResource powerResource, int amount, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            OnRecoverResource(powerResource, amount, combatMagnitude, abilityEffectContext);
        }

        public void NotifyOnGainXP(int gainedXP, int currentXP) {
            OnGainXP(unitController, gainedXP, currentXP);
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
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDropCombat()");

            OnDropCombat();
        }

        public void NotifyOnStartInteract() {
            OnStartInteract();
        }

        public void NotifyOnStopInteract() {
            OnStopInteract();
        }

        public void NotifyOnStartInteractWithOption(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnStartInteractWithOption({interactableOptionComponent.Interactable.gameObject.name}, {componentIndex}, {choiceIndex})");

            OnStartInteractWithOption(unitController, interactableOptionComponent, componentIndex, choiceIndex);
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

        public void NotifyOnTakeDamage(IAbilityCaster source, UnitController target, int damage, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, AbilityEffectContext abilityEffectContext) {
            unitController.UnitAnimator.HandleTakeDamage();
            OnTakeDamage(source, target, damage, combatTextType, combatMagnitude, abilityName, abilityEffectContext);
        }

        public void NotifyOnTakeFallDamage() {
            OnTakeFallDamage();
        }

        public void NotifyOnKillTarget() {
            OnKillTarget();
        }

        public void NotifyOnMovement() {
            OnMovement();
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
            OnReputationChange(unitController);
            unitController.UnitComponentController.HighlightController.UpdateColors();
        }

        public void NotifyOnBeforeDie(UnitController targetUnitController) {
            unitController.UnitComponentController.StopMovementSound();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnBeforeDie(targetUnitController);

        }

        public void NotifyOnAfterDie(CharacterStats characterStats) {
            OnAfterDie(characterStats);
        }

        public void NotifyOnReviveComplete() {
            OnReviveComplete(unitController);
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
            OnClassChange(unitController, newCharacterClass, oldCharacterClass);
        }
        public void NotifyOnSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(unitController, newClassSpecialization, oldClassSpecialization);
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
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnStatusEffectAdd({statusEffectNode.StatusEffect.DisplayName})");

            OnStatusEffectAdd(unitController, statusEffectNode);
        }

        public void NotifyOnAddStatusEffectStack(string resourceName) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAddStatusEffectStack({resourceName})");

            OnAddStatusEffectStack(resourceName);
        }

        public void NotifyOnCastTimeChanged(IAbilityCaster source, AbilityProperties baseAbility, float castPercent) {
            OnCastTimeChanged(source, baseAbility, castPercent);
        }

        public void NotifyOnCastComplete() {
            OnCastComplete();
        }

        public void NotifyOnCastCancel() {
            OnCastCancel();
        }

        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnActivateMountedState({mountUnitController.gameObject.name})");

            OnActivateMountedState(mountUnitController);
        }

        public void NotifyOnDeactivateMountedState() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDeactivateMountedState()");

            OnDeactivateMountedState();
        }

        public void NotifyOnBeginChatMessage(string message) {
            //Debug.Log($"{gameObject.name}.NotifyOnMessageFeed(" + message + ")");
            OnBeginChatMessage(message);
        }

        public void NotifyOnInitializeAnimator() {
            OnInitializeAnimator();
        }

        public void NotifyOnAnimatorSetTrigger(string triggerName) {
            OnAnimatorSetTrigger(triggerName);
        }

        public void NotifyOnAnimatorResetTrigger(string triggerName) {
            OnAnimatorResetTrigger(triggerName);
        }

        public void NotifyOnSetAnimationClipOverride(string originalClipName, AnimationClip newAnimationClip) {
            OnSetAnimationClipOverride(originalClipName, newAnimationClip);
        }

        public void NotifyOnPerformAnimatedActionAnimation(AnimatedAction animatedAction) {
            OnPerformAnimatedActionAnimation(animatedAction);
        }

        public void NotifyOnPerformAbilityCastAnimation(AbilityProperties abilityProperties, int clipIndex) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnPerformAbilityCastAnimation()");

            OnPerformAbilityCastAnimation(abilityProperties, clipIndex);
        }

        public void NotifyOnPerformAbilityActionAnimation(AbilityProperties abilityProperties, int clipIndex) {
            OnPerformAbilityActionAnimation(abilityProperties, clipIndex);
        }

        public void NotifyOnAnimatorClearAction() {
            OnAnimatorClearAction();
        }

        public void NotifyOnAnimatorClearAbilityAction() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnClearAbilityAction()");

            OnAnimatorClearAbilityAction();
        }

        public void NotifyOnAnimatorClearAbilityCast() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnClearAbilityCast()");

            OnAnimatorClearAbilityCast();
        }

        public void NotifyOnSpawnAbilityObjects(AbilityProperties abilityProperties, int index) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSpawnAbilityObjects({abilityProperties.ResourceName}, {index})");

            OnSpawnAbilityObjects(abilityProperties, index);
        }

        public void NotifyOnDespawnAbilityObjects() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawnAbilityObjects()");

            OnDespawnAbilityObjects();
        }

        public void NotifyOnSpawnActionObjects(AnimatedAction animatedAction) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSpawnActionObjects({animatedAction.ResourceName})");

            OnSpawnActionObjects(animatedAction);
        }

        public void NotifyOnDespawnActionObjects() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawnActionObjects()");

            OnDespawnActionObjects();
        }

        public void NotifyOnSpawnAbilityEffectPrefabs(Interactable target, Interactable originalTarget, LengthEffectProperties lengthEffectProperties, AbilityEffectContext abilityEffectInput) {
            OnSpawnAbilityEffectPrefabs(target, originalTarget, lengthEffectProperties, abilityEffectInput);
        }

        public void NotifyOnEnterInteractableTrigger(Interactable interactable) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnEnterInteractableTrigger({interactable.gameObject.name})");

            OnEnterInteractableTrigger(unitController, interactable);
        }

        public void NotifyOnExitInteractableTrigger(Interactable interactable) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnExitInteractableTrigger({interactable.gameObject.name})");

            OnExitInteractableTrigger(unitController, interactable);
        }

        public void NotifyOnEnterInteractableRange(Interactable interactable) {
            OnEnterInteractableRange(unitController, interactable);
        }

        public void NotifyOnExitInteractableRange(Interactable interactable) {
            OnExitInteractableRange(unitController, interactable);
        }

        public void NotifyOnAcceptQuest(Quest quest) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnAcceptQuest({questBase.ResourceName})");

            OnAcceptQuest(unitController, quest);
        }

        public void NotifyOnAcceptAchievement(Achievement achievement) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnAcceptQuest({questBase.ResourceName})");

            OnAcceptAchievement(unitController, achievement);
        }


        public void NotifyOnMarkQuestComplete(Quest quest) {
            OnMarkQuestComplete(unitController, quest);
        }

        public void NotifyOnMarkAchievementComplete(Achievement achievement) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnMarkAchievementComplete({achievement.ResourceName})");

            OnMarkAchievementComplete(unitController, achievement);
        }

        public void NotifyOnQuestObjectiveStatusUpdated(Quest quest) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnQuestObjectiveStatusUpdated({quest.ResourceName})");

            OnQuestObjectiveStatusUpdated(unitController, quest);
        }

        public void NotifyOnAchievementObjectiveStatusUpdated(Achievement achievement) {
            OnAchievementObjectiveStatusUpdated(unitController, achievement);
        }

        public void NotifyOnLearnSkill(Skill newSkill) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnLearnSkill({newSkill.ResourceName})");

            OnLearnSkill(unitController, newSkill);
        }

        public void NotifyOnUnLearnSkill(Skill oldSkill) {
            OnUnLearnSkill(unitController, oldSkill);
        }

        public void NotifyOnSetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            OnSetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, amount);
        }

        public void NotifyOnSetAchievementObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            OnSetAchievementObjectiveCurrentAmount(questName, objectiveType, objectiveName, amount);
        }

        public void NotifyOnAbandonQuest(QuestBase oldQuest) {
            OnAbandonQuest(unitController, oldQuest);
        }

        public void NotifyOnTurnInQuest(Quest quest) {
            OnTurnInQuest(unitController, quest);
        }

        public void NotifyOnPlaceInStack(InstantiatedItem instantiatedItem, bool addToBank, int slotIndex) {
            OnPlaceInStack(instantiatedItem.InstanceId, addToBank, slotIndex);
        }

        public void NotifyOnPlaceInEmpty(InstantiatedItem instantiatedItem, bool addToBank, int slotIndex) {
            OnPlaceInEmpty(instantiatedItem.InstanceId, addToBank, slotIndex);
        }

        public void NotifyOnGetNewInstantiatedItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnGetNewInstantiatedItem({instantiatedItem.Item.ResourceName})");

            OnGetNewInstantiatedItem(instantiatedItem);
        }

        public void NotifyOnRequestDeleteItem(InstantiatedItem instantiatedItem) {
            OnRequestDeleteItem(instantiatedItem);
        }

        public void NotifyOnDeleteItem(InstantiatedItem instantiatedItem) {
            OnDeleteItem(instantiatedItem);
        }

        public void NotifyOnRequestEquipToSlot(InstantiatedEquipment newEquipment, EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestEquipToSlot({equipmentSlotProfile.ResourceName}, {newEquipment.Item.ResourceName})");

            OnRequestEquipToSlot(newEquipment, equipmentSlotProfile);
        }

        /*
        public void NotifyOnRequestUnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            OnRequestUnequipFromList(equipmentSlotProfile);
        }
        */

        public void NotifyOnRemoveEquipment(EquipmentSlotProfile equipmentSlotProfile, InstantiatedEquipment instantiatedEquipment) {
            OnRemoveEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public void NotifyOnAddEquipment(EquipmentSlotProfile equipmentSlotProfile, InstantiatedEquipment instantiatedEquipment) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAddEquipment({equipmentSlotProfile.ResourceName}, {instantiatedEquipment.Item.ResourceName})");

            OnAddEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public void NotifyOnAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"UnitEventController.NotifyOnAddItemToInventorySlot({item.Item.ResourceName})");

            OnAddItemToInventorySlot(slot, item);
        }

        public void NotifyOnRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRemoveItemFromInventorySlot({item.Item.ResourceName})");

            OnRemoveItemFromInventorySlot(slot, item);
        }

        public void NotifyOnAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            OnAddItemToBankSlot(slot, item);
        }

        public void NotifyOnRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            OnRemoveItemFromBankSlot(slot, item);
        }

        public void NotifyOnRequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot, bool fromSlotIsInventory, bool toSlotIsInventory) {
            OnRequestDropItemFromInventorySlot(fromSlot, toSlot, fromSlotIsInventory, toSlotIsInventory);
        }

        public void NotifyOnSetCraftAbility(CraftAbilityProperties craftAbility) {
            OnSetCraftAbility(craftAbility);
        }

        public void NotifyOnCraftItem() {
            OnCraftItem();
        }

        public void NotifyOnRemoveFirstCraftingQueueItem() {
            OnRemoveFirstCraftingQueueItem();
        }

        public void NotifyOnClearCraftingQueue() {
            OnClearCraftingQueue();
        }

        public void NotifyOnAddToCraftingQueue(Recipe recipe) {
            OnAddToCraftingQueue(recipe);
        }

        public void NotifyOnRequestMoveFromBankToInventory(int slotIndex) {
            OnRequestMoveFromBankToInventory(slotIndex);
        }

        public void NotifyOnRequestMoveFromInventoryToBank(int slotIndex) {
            OnRequestMoveFromInventoryToBank(slotIndex);
        }

        public void NotifyOnRequestUseItem(int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestUseItem({slotIndex})");

            OnRequestUseItem(slotIndex);
        }

        public void NotifyOnRebuildModelAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRebuildModelAppearance()");

            OnRebuildModelAppearance();
        }

        public void NotifyOnRequestSwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            OnRequestSwapInventoryEquipment(oldEquipment, newEquipment);
        }

        public void NotifyOnRequestUnequipToSlot(InstantiatedEquipment instantiatedEquipment, int inventorySlotId) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestUnequipToSlot({instantiatedEquipment.Item.ResourceName}, {inventorySlotId})");

            OnRequestUnequipToSlot(instantiatedEquipment, inventorySlotId);
        }

        public void NotifyOnRequestSwapBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            OnRequestSwapBags(oldInstantiatedBag, newInstantiatedBag);
        }

        public void NotifyOnRequestUnequipBagToSlot(InstantiatedBag instantiatedBag, int slotIndex, bool isBankSlot) {
            OnRequestUnequipBagToSlot(instantiatedBag, slotIndex, isBankSlot);
        }

        public void NotifyOnRemoveBag(InstantiatedBag instantiatedBag) {
            OnRemoveBag(instantiatedBag);
        }

        public void NotifyOnAddBag(InstantiatedBag instantiatedBag, BagNode bagNode) {
            OnAddBag(instantiatedBag, bagNode);
        }

        public void NotifyOnRequestMoveBag(InstantiatedBag bag, int nodeIndex, bool isBankNode) {
            OnRequestMoveBag(bag, nodeIndex, isBankNode);
        }

        public void NotifyOnRequestAddBagFromInventory(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestAddBagFromInventory({instantiatedBag.Item.ResourceName}, {nodeIndex}, {isBankNode})");
            
            OnRequestAddBag(instantiatedBag, nodeIndex, isBankNode);
        }

        public void NotifyOnRequestUnequipBag(InstantiatedBag instantiatedBag, bool isBank) {
            OnRequestUnequipBag(instantiatedBag, isBank);
        }

        public void NotifyOnCancelStatusEffect(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnCancelStatusEffect({statusEffect.ResourceName})");

            OnCancelStatusEffect(statusEffect);
        }

        public void NotifyOnSpawnProjectileEffectPrefabs(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSpawnProjectileEffectPrefabs({projectileEffectProperties.ResourceName})");

            OnSpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, abilityEffectContext);
        }

        public void NotifyOnSpawnChanneledEffectPrefabs(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            OnSpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, abilityEffectContext);
        }

        public void NotifyOnSetGroundTarget(Vector3 newGroundTarget) {
            OnSetGroundTarget(newGroundTarget);
        }

        public void NotifyOnReceiveCombatTextEvent(UnitController targetUnitController, int damage, CombatTextType combatTextType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            OnReceiveCombatTextEvent(targetUnitController, damage, combatTextType, combatMagnitude, abilityEffectContext);
        }

        public void NotifyOnRequestCancelStatusEffect(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestCancelStatusEffect({statusEffect.ResourceName})");
            OnRequestCancelStatusEffect(statusEffect);
        }

        public void NotifyOnUnlearnRecipe(Recipe oldRecipe) {
            OnUnlearnRecipe(oldRecipe);
        }

        public void NotifyOnLearnRecipe(Recipe newRecipe) {
            OnLearnRecipe(newRecipe);
        }

        public void NotifyOnCurrencyChange(string currencyResourceName, int amount) {
            OnCurrencyChange(currencyResourceName, amount);
        }

        public void NotifyOnAddPet(UnitProfile unitProfile) {
            OnAddPet(unitProfile);
        }

        public void NotifyOnAddActivePet(UnitProfile unitProfile, UnitController petUnitController) {
            OnAddActivePet(unitProfile, petUnitController);
        }

        public void NotifyOnSetReputationAmount(Faction faction, float amount) {
            OnSetReputationAmount(faction, amount);
        }

        public void NotifyOnUnsetGamepadActionButton(int buttonIndex) {
            OnUnsetGamepadActionButton(buttonIndex);
        }

        public void NotifyOnUnsetMouseActionButton(int buttonIndex) {
            OnUnsetMouseActionButton(buttonIndex);
        }

        public void NotifyOnSetMouseActionButton(IUseable useable, int buttonIndex) {
            OnSetMouseActionButton(useable, buttonIndex);
        }

        public void NotifyOnSetGamepadActionButton(IUseable useable, int buttonIndex) {
            OnSetGamepadActionButton(useable, buttonIndex);
        }

        public void NotifyOnRequestMoveGamepadUseable(int oldIndex, int newIndex) {
            OnRequestMoveGamepadUseable(oldIndex, newIndex);
        }

        public void NotifyOnRequestAssignGamepadUseable(IUseable useable, int buttonIndex) {
            OnRequestAssignGamepadUseable(useable, buttonIndex);
        }

        public void NotifyOnRequestMoveMouseUseable(int oldIndex, int newIndex) {
            OnRequestMoveMouseUseable(oldIndex, newIndex);
        }

        public void NotifyOnRequestAssignMouseUseable(IUseable useable, int buttonIndex) {
            OnRequestAssignMouseUseable(useable, buttonIndex);
        }

        public void NotifyOnRequestClearMouseUseable(int buttonIndex) {
            OnRequestClearMouseUseable(buttonIndex);
        }

        public void NotifyOnRequestClearGamepadUseable(int buttonIndex) {
            OnRequestClearGamepadUseable(buttonIndex);
        }

        public void NotifyOnCharacterConfigured() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnCharacterConfigured()");

            OnCharacterConfigured();
        }

        public void NotifyOnRemoveActivePet(UnitProfile unitProfile) {
            OnRemoveActivePet(unitProfile);
        }

        public void NotifyOnInitiateGlobalCooldown(float coolDownToUse) {
            OnInitiateGlobalCooldown(coolDownToUse);
        }

        public void NotifyOnBeginActionCoolDown(InstantiatedActionItem actionItem, float coolDownLength) {
            OnBeginActionCoolDown(actionItem, coolDownLength);
        }

        public void NotifyOnActivateAutoAttack() {
            OnActivateAutoAttack();
        }

        public void NotifyOnDeactivateAutoAttack() {
            OnDeactivateAutoAttack();
        }

        public void NotifyOnStartFlying() {
            OnStartFlying();
        }

        public void NotifyOnStopFlying() {
            OnStopFlying();
        }

        public void NotifyOnSetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSetMountedState({mountUnitController.gameObject.name}, {mountUnitProfile.ResourceName})");

            OnSetMountedState(mountUnitController, mountUnitProfile);
        }

        public void NotifyOnSetParent(Transform mountPoint) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSetParent({(mountPoint == null ? "null" : mountPoint.gameObject.name)})");

            //public void NotifyOnSetParent(Transform mountPoint, Vector3 position, Vector3 localEulerAngles) {
            OnSetParent(mountPoint);
        }

        public void NotifyOnUnsetParent() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnUnSetParent()");

            OnUnsetParent();
        }

        public void NotifyOnMountUnitSpawn() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnMountUnitSpawn()");

            OnMountUnitSpawn();
        }

        public void NotifyOnDespawnMountUnit() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawnMountUnit()");

            OnDespawnMountUnit();
        }

        public void NotifyOnWriteMessageFeedMessage(string messageText) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnWriteMessageFeedMessage({messageText})");

            OnWriteMessageFeedMessage(messageText);
        }

        public void NotifyOnItemCountChanged(Item item) {
            OnItemCountChanged(unitController, item);
        }

        public void NotifyOnDialogCompleted(Dialog dialog) {
            OnDialogCompleted(unitController, dialog);
        }

        public void NotifyOnCompleteInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            OnCompleteInteractWithOption(unitController, interactableOptionComponent);
        }

        public void NotifyOnInteractWithQuestStartItem(Quest quest, int slotIndex, long itemInstanceId) {
            OnInteractWithQuestStartItem(quest, slotIndex, itemInstanceId);
        }

        public void NotifyOnRequestAcceptQuestItemQuest(int slotIndex, long instanceId, Quest currentQuest) {
            OnRequestAcceptQuestItemQuest(slotIndex, instanceId, currentQuest);
        }

        public void NotifyOnRequestCompleteQuestItemQuest(int slotIndex, long instanceId, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            OnRequestCompleteQuestItemQuest(slotIndex, instanceId, currentQuest, questRewardChoices);
        }

        public void NotifyOnSaveDataUpdated() {
            OnSaveDataUpdated();
        }

        public void NotifyOnNameChangeFail() {
            OnNameChangeFail();
        }

        public void NotifyOnSetGroupId(int groupId) {
            OnSetGroupId(groupId);
        }

        public void NotifyOnSetGuildId(int guildId, string guildName) {
            OnSetGuildId(guildId, guildName);
        }

        #endregion


    }

}