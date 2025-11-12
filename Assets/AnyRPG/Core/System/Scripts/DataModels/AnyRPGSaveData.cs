using System;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class AnyRPGSaveData {

        public int PlayerLevel = 1;
        public int currentExperience;

        public string playerName = string.Empty;
        public string unitProfileName = string.Empty;
        public string characterRace = string.Empty;
        public string characterClass = string.Empty;
        public string classSpecialization = string.Empty;
        public string playerFaction = string.Empty;
        public bool isDead = false;
        public bool isMounted = false;
        public bool initializeResourceAmounts = false;
        public bool OverrideLocation = false;
        public float PlayerLocationX;
        public float PlayerLocationY;
        public float PlayerLocationZ;
        public bool OverrideRotation = false;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float PlayerRotationZ;
        public string OverrideLocationTag = string.Empty;
        public string appearanceString = string.Empty;
        public string CurrentScene = string.Empty;
        public string DataCreatedOn = string.Empty;
        public string DataSavedOn = string.Empty;
        public string DataFileName = string.Empty;
        public int GamepadActionButtonSet;
        public int ClientItemIdCount = 1;

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
    public class QuestSaveData {

        public string QuestName;
        public int questStep;
        public bool turnedIn;
        public bool markedComplete;
        public bool inLog;

        public List<QuestObjectiveSaveData> questObjectives = new List<QuestObjectiveSaveData>();
    }

    [Serializable]
    public struct StatusEffectSaveData {

        public string StatusEffectName;
        public int remainingSeconds;
    }

    [Serializable]
    public class SceneNodeSaveData {

        public string SceneName;
        public bool visited;

        public List<PersistentObjectSaveData> persistentObjects = new List<PersistentObjectSaveData>();
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
    public class DialogSaveData {

        public string DialogName;
        public bool turnedIn;

        public List<bool> dialogNodeShown = new List<bool>();
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
        public bool savedIsItem;
    }

    [Serializable]
    public class InventorySlotSaveData {

        public string ItemName;
        public string DisplayName;
        public string itemQuality;
        public int dropLevel;
        public int itemInstanceId;

        public int stackCount;
        public List<int> randomSecondaryStatIndexes = new List<int>();
        public string gainCurrencyName;
        public int gainCurrencyAmount;
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
        public string DisplayName;
        public string itemQuality;
        public int dropLevel;
        public int slotCount;
        public int itemInstanceId;
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
    public class EquipmentSaveData {

        public string EquipmentName;
        public string DisplayName;
        public string itemQuality;
        public int dropLevel;
        public int itemInstanceId;

        public List<int> randomSecondaryStatIndexes = new List<int>();
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