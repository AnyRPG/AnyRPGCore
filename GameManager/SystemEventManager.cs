using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEventManager : MonoBehaviour {

        #region Singleton
        private static SystemEventManager instance;

        public static SystemEventManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEventManager>();
                }

                return instance;
            }
        }
        #endregion

        //public event System.Action OnPrerequisiteUpdated = delegate { };
        public event System.Action OnQuestStatusUpdated = delegate { };
        public event System.Action OnAfterQuestStatusUpdated = delegate { };
        public event System.Action OnQuestObjectiveStatusUpdated = delegate { };
        //public event System.Action<IAbility> OnAbilityCast = delegate { };
        public event System.Action<BaseAbility> OnAbilityUsed = delegate { };
        public event System.Action<BaseAbility> OnAbilityListChanged = delegate { };
        public event System.Action<Skill> OnSkillListChanged = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action OnReputationChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };

        public event System.Action<string> OnInteractionStarted = delegate { };
        public event System.Action<InteractableOption> OnInteractionWithOptionStarted = delegate { };
        public event System.Action<Interactable> OnInteractionCompleted = delegate { };
        public event System.Action<InteractableOption> OnInteractionWithOptionCompleted = delegate { };
        public event System.Action<Item> OnItemCountChanged = delegate { };
        public event System.Action OnBeginKeybind = delegate { };
        public event System.Action OnEndKeybind = delegate { };
        public event System.Action<Dialog> OnDialogCompleted = delegate { };
        public event System.Action OnDeleteSaveData = delegate { };
        public event System.Action<BaseCharacter, CharacterUnit, int, string> OnTakeDamage = delegate { };
        public event System.Action OnXPGained = delegate { };
        public event System.Action OnPlayerDeath = delegate { };

        // Player Manager
        public event System.Action OnPlayerConnectionSpawn = delegate { };
        public event System.Action OnBeforePlayerConnectionSpawn = delegate { };
        public event System.Action OnPlayerUnitSpawn = delegate { };
        public event System.Action OnPlayerConnectionDespawn = delegate { };
        public event System.Action OnPlayerUnitDespawn = delegate { };
        public event System.Action OnPlayerUMACreated = delegate { };
        public event System.Action OnPlayerNameChanged = delegate { };

        // Level manager
        public event System.Action OnLevelUnload = delegate { };
        public event System.Action OnLevelLoad = delegate { };
        public event System.Action OnExitGame = delegate { };

        // loot UI
        public event System.Action OnTakeLoot = delegate { };

        // equipment manager
        public System.Action<Equipment, Equipment> OnEquipmentChanged = delegate { };
        //public System.Action<Equipment> OnEquipmentRefresh = delegate { };

        // UI
        public System.Action OnPagedButtonsTransparencyUpdate = delegate { };
        public System.Action OnInventoryTransparencyUpdate = delegate { };

        // currency manager
        public System.Action OnCurrencyChange = delegate { };

        private void Awake() {
            //Debug.Log("SystemGameManager.Awake()");
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");
        }

        public void NotifyOnCurrencyChange() {
            OnCurrencyChange();
        }

        public void NotifyOnPlayerDeath() {
            OnPlayerDeath();
        }

        public void NotifyOnInventoryTransparencyUpdate() {
            //Debug.Log("SystemEventManager.OnInventoryTransparencyUpdate()");
            OnInventoryTransparencyUpdate();
        }

        public void NotifyOnPagedButtonsTransparencyUpdate() {
            //Debug.Log("SystemEventManager.NotifyOnPagedButtonsTransparencyUpdate()");
            OnPagedButtonsTransparencyUpdate();
        }

        public void NotifyOnQuestObjectiveStatusUpdated() {
            OnQuestObjectiveStatusUpdated();
        }

        public void NotifyOnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            OnEquipmentChanged(newEquipment, oldEquipment);
        }

        public void NotifyOnClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(newCharacterClass, oldCharacterClass);
        }

        /*
        public void NotifyOnEquipmentRefresh(Equipment newEquipment) {
            OnEquipmentRefresh(newEquipment);
        }
        */

        public void NotifyOnTakeLoot() {
            OnTakeLoot();
        }

        public void NotifyOnLevelUnload() {
            OnLevelUnload();
        }

        public void NotifyOnLevelLoad() {
            OnLevelLoad();
        }

        public void NotifyOnExitGame() {
            OnExitGame();
        }

        public void NotifyOnPlayerConnectionSpawn() {
            OnPlayerConnectionSpawn();
        }

        public void NotifyBeforePlayerConnectionSpawn() {
            OnBeforePlayerConnectionSpawn();
        }

        public void NotifyOnPlayerUnitSpawn() {
            //Debug.Log("SystemEventManager.NotifyOnPlayerUnitSpawn()");
            OnPlayerUnitSpawn();
        }

        public void NotifyOnPlayerConnectionDespawn() {
            OnPlayerConnectionDespawn();
        }

        public void NotifyOnPlayerUnitDespawn() {
            OnPlayerUnitDespawn();
        }

        public void NotifyOnPlayerUMACreated() {
            OnPlayerUMACreated();
        }

        public void NotifyOnPlayerNameChanged() {
            OnPlayerNameChanged();
        }

        public void NotifyOnXPGained() {
            OnXPGained();
        }

        public void NotifyOnTakeDamage(BaseCharacter source, CharacterUnit target, int damage, string abilityName) {
            OnTakeDamage(source, target, damage, abilityName);
        }

        public void NotifyOnDeleteSaveData() {
            OnDeleteSaveData();
        }

        public void NotifyOnOnBeginKeybind() {
            OnBeginKeybind();
        }

        public void NotifyOnEndKeybind() {
            OnEndKeybind();
        }

        public void NotifyOnDialogCompleted(Dialog dialog) {
            OnDialogCompleted(dialog);
            //OnPrerequisiteUpdated();

        }

        public void NotifyOnInteractionStarted(string interactableName) {
            //Debug.Log("SystemEventManager.NotifyOnInteractionStarted(" + interactableName + ")");
            OnInteractionStarted(interactableName);
        }

        public void NotifyOnInteractionWithOptionStarted(InteractableOption interactableOption) {
            OnInteractionWithOptionStarted(interactableOption);
        }

        public void NotifyOnInteractionCompleted(Interactable interactable) {
            OnInteractionCompleted(interactable);
        }

        public void NotifyOnInteractionWithOptionCompleted(InteractableOption interactableOption) {
            OnInteractionWithOptionCompleted(interactableOption);
        }

        public void NotifyOnReputationChange() {
            //Debug.Log("SystemEventManager.NotifyOnReputationChange()");
            OnReputationChange();
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnLevelChanged(int newLevel) {
            OnLevelChanged(newLevel);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnQuestStatusUpdated() {
            //Debug.Log("SystemEventManager.NotifyOnQuestStatusUpdated");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                return;
            }
            OnQuestStatusUpdated();
            OnAfterQuestStatusUpdated();
            // having these two separate seems to be ok for now.  the items that react to the first event do not react to the second, nor do they send prerequisiteupdates so no double calls should happen
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnAbilityListChanged(BaseAbility newAbility) {
            //Debug.Log("SystemEventManager.NotifyOnAbilityListChanged(" + abilityName + ")");
            OnAbilityListChanged(newAbility);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnAbilityUsed(BaseAbility ability) {
            //Debug.Log("SystemEventManager.NotifyAbilityused(" + ability.MyName + ")");
            OnAbilityUsed(ability);
        }

        public void NotifyOnSkillListChanged(Skill skill) {
            OnSkillListChanged(skill);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnItemCountChanged(Item item) {
            OnItemCountChanged(item);
        }
        /*
        public void NotifyAbilityCast(IAbility ability) {
            OnAbilityCast(ability);
        }
        */

    }

}