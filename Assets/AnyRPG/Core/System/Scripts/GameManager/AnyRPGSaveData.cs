using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    
    [Serializable]
    public class AnyRPGSaveData {

        public int PlayerLevel;
        public int currentExperience;

        public string playerName = string.Empty;
        public string unitProfileName = string.Empty;
        public string characterRace = string.Empty;
        public string characterClass = string.Empty;
        public string classSpecialization = string.Empty;
        public string playerFaction = string.Empty;
        public bool OverrideLocation;
        public float PlayerLocationX;
        public float PlayerLocationY;
        public float PlayerLocationZ;
        public bool OverrideRotation;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float PlayerRotationZ;
        public string appearanceString = string.Empty;
        public string CurrentScene = string.Empty;
        public string DataCreatedOn = string.Empty;
        public string DataSavedOn = string.Empty;
        public string DataFileName = string.Empty;
        public int GamepadActionButtonSet;

        public List<ResourcePowerSaveData> resourcePowerSaveData = new List<ResourcePowerSaveData>();
        public List<SwappableMeshSaveData> swappableMeshSaveData = new List<SwappableMeshSaveData>();
        public List<ActionBarSaveData> actionBarSaveData = new List<ActionBarSaveData>();
        public List<ActionBarSaveData> gamepadActionBarSaveData = new List<ActionBarSaveData>();
        public List<InventorySlotSaveData> inventorySlotSaveData = new List<InventorySlotSaveData>();
        public List<InventorySlotSaveData> bankSlotSaveData = new List<InventorySlotSaveData>();
        public List<EquippedBagSaveData> equippedBagSaveData = new List<EquippedBagSaveData>();
        public List<EquippedBagSaveData> equippedBankBagSaveData = new List<EquippedBagSaveData>();
        public List<AbilitySaveData> abilitySaveData = new List<AbilitySaveData>();
        public List<SkillSaveData> skillSaveData = new List<SkillSaveData>();
        public List<RecipeSaveData> recipeSaveData = new List<RecipeSaveData>();
        public List<ReputationSaveData> reputationSaveData = new List<ReputationSaveData>();
        public List<EquipmentSaveData> equipmentSaveData = new List<EquipmentSaveData>();
        public List<CurrencySaveData> currencySaveData = new List<CurrencySaveData>();
        public List<StatusEffectSaveData> statusEffectSaveData = new List<StatusEffectSaveData>();
        public List<PetSaveData> petSaveData = new List<PetSaveData>();
        public List<BehaviorSaveData> behaviorSaveData = new List<BehaviorSaveData>();

        // the properties below currently overwrite properties of scriptableObjects
        // this is undesired and if any similar data is added, it needs to be intentionally cleared between game loads
        public List<QuestSaveData> questSaveData = new List<QuestSaveData>();
        public List<QuestSaveData> achievementSaveData = new List<QuestSaveData>();
        public List<DialogSaveData> dialogSaveData = new List<DialogSaveData>();
        public List<SceneNodeSaveData> sceneNodeSaveData = new List<SceneNodeSaveData>();
        public List<CutsceneSaveData> cutsceneSaveData = new List<CutsceneSaveData>();

    }

    [Serializable]
    public struct ResourcePowerSaveData {

        public string ResourceName;
        public float amount;
    }

    [Serializable]
    public struct SwappableMeshSaveData {

        public string groupName;
        public string optionName;
    }

    [Serializable]
    public struct QuestSaveData {

        public string QuestName;
        public int questStep;
        public bool turnedIn;
        public bool markedComplete;
        public bool inLog;

        public List<QuestObjectiveSaveData> questObjectives;
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