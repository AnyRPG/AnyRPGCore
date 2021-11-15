using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [Serializable]
    public struct AnyRPGSaveData {

        public int PlayerLevel;
        public int currentExperience;

        public string playerName;
        public string unitProfileName;
        public string characterRace;
        public string characterClass;
        public string classSpecialization;
        public string playerFaction;
        public bool OverrideLocation;
        public float PlayerLocationX;
        public float PlayerLocationY;
        public float PlayerLocationZ;
        public bool OverrideRotation;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float PlayerRotationZ;
        public string PlayerUMARecipe;
        public string CurrentScene;
        public string DataCreatedOn;
        public string DataSavedOn;
        public string DataFileName;

        public List<ResourcePowerSaveData> resourcePowerSaveData;
        public List<ActionBarSaveData> actionBarSaveData;
        public List<ActionBarSaveData> gamepadActionBarSaveData;
        public List<InventorySlotSaveData> inventorySlotSaveData;
        public List<InventorySlotSaveData> bankSlotSaveData;
        public List<EquippedBagSaveData> equippedBagSaveData;
        public List<EquippedBagSaveData> equippedBankBagSaveData;
        public List<AbilitySaveData> abilitySaveData;
        public List<SkillSaveData> skillSaveData;
        public List<RecipeSaveData> recipeSaveData;
        public List<ReputationSaveData> reputationSaveData;
        public List<EquipmentSaveData> equipmentSaveData;
        public List<CurrencySaveData> currencySaveData;
        public List<StatusEffectSaveData> statusEffectSaveData;
        public List<PetSaveData> petSaveData;
        public List<BehaviorSaveData> behaviorSaveData;

        // the properties below currently overwrite properties of scriptableObjects
        // this is undesired and if any similar data is added, it needs to be intentionally cleared between game loads
        public List<QuestSaveData> questSaveData;
        public List<DialogSaveData> dialogSaveData;
        public List<SceneNodeSaveData> sceneNodeSaveData;
        public List<CutsceneSaveData> cutsceneSaveData;

    }

    [Serializable]
    public struct ResourcePowerSaveData {

        public string ResourceName;
        public float amount;
    }

    [Serializable]
    public struct QuestSaveData {

        public string QuestName;
        public int questStep;
        public bool turnedIn;
        public bool markedComplete;
        public bool inLog;

        public List<QuestObjectiveSaveData> questObjectives;

        /*
        public List<QuestObjectiveSaveData> killObjectives;
        public List<QuestObjectiveSaveData> useInteractableObjectives;
        public List<QuestObjectiveSaveData> collectObjectives;
        public List<QuestObjectiveSaveData> tradeSkillObjectives;
        public List<QuestObjectiveSaveData> abilityObjectives;
        public List<QuestObjectiveSaveData> visitZoneObjectives;
        */
    }

    [Serializable]
    public struct StatusEffectSaveData {

        public string StatusEffectName;
        public int remainingSeconds;
    }

    [Serializable]
    public struct SceneNodeSaveData {

        public string SceneName;
        public bool visited;

        public List<PersistentObjectSaveData> persistentObjects;
    }

    [Serializable]
    public struct CutsceneSaveData {

        public string CutsceneName;

        public bool isCutSceneViewed;
    }

    [Serializable]
    public struct PersistentObjectSaveData {

        public string UUID;
        public float LocationX;
        public float LocationY;
        public float LocationZ;
        public float DirectionX;
        public float DirectionY;
        public float DirectionZ;
    }

    [Serializable]
    public struct DialogSaveData {

        public string DialogName;
        public bool turnedIn;
    }

    [Serializable]
    public struct BehaviorSaveData {

        public string BehaviorName;
        public bool completed;
    }

    [Serializable]
    public struct ActionBarSaveData {

        public string DisplayName;
        public bool isItem;
        public string savedName;
    }

    [Serializable]
    public struct InventorySlotSaveData {

        public string ItemName;
        public string DisplayName;
        public string itemQuality;
        public int dropLevel;

        public int stackCount;
        public List<int> randomSecondaryStatIndexes;
    }

    [Serializable]
    public struct QuestObjectiveSaveData {

        public string ObjectiveType;
        public string ObjectiveName;
        public int Amount;
    }

    [Serializable]
    public struct EquippedBagSaveData {

        public string BagName;
        public int slotCount;
    }

    [Serializable]
    public struct AbilitySaveData {

        public string AbilityName;

    }

    [Serializable]
    public struct PetSaveData {

        public string PetName;

    }

    [Serializable]
    public struct EquipmentSaveData {

        public string EquipmentName;
        public string DisplayName;
        public string itemQuality;
        public int dropLevel;

        public List<int> randomSecondaryStatIndexes;
    }

    [Serializable]
    public struct SkillSaveData {

        public string SkillName;
    }

    [Serializable]
    public struct RecipeSaveData {

        public string RecipeName;
    }

    [Serializable]
    public struct ReputationSaveData {

        public string ReputationName;
        public float Amount;

    }

    [Serializable]
    public struct CurrencySaveData {

        public string CurrencyName;
        public int Amount;

    }

}