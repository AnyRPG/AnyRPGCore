using System;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class CharacterSaveData {

        public int CharacterId = 0;
        public string DataCreatedOn = string.Empty;
        public string DataSavedOn = string.Empty;

        public int CharacterLevel = 1;
        public int CurrentExperience;

        public string CharacterName = string.Empty;
        public string UnitProfileName = string.Empty;
        public string CharacterRace = string.Empty;
        public string CharacterClass = string.Empty;
        public string ClassSpecialization = string.Empty;
        public string CharacterFaction = string.Empty;
        public bool IsDead = false;
        public bool IsMounted = false;
        public bool InitializeResourceAmounts = false;
        public bool OverrideLocation = false;
        public float PlayerLocationX;
        public float PlayerLocationY;
        public float PlayerLocationZ;
        public bool OverrideRotation = false;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float PlayerRotationZ;
        public string OverrideLocationTag = string.Empty;
        public string AppearanceString = string.Empty;
        public string CurrentScene = string.Empty;

        public int GamepadActionButtonSet;

        public List<ResourcePowerSaveData> ResourcePowerSaveData = new List<ResourcePowerSaveData>();
        public List<SwappableMeshSaveData> SwappableMeshSaveData = new List<SwappableMeshSaveData>();
        public List<ActionBarSaveData> ActionBarSaveData = new List<ActionBarSaveData>();
        public List<ActionBarSaveData> GamepadActionBarSaveData = new List<ActionBarSaveData>();
        public List<InventorySlotSaveData> InventorySlotSaveData = new List<InventorySlotSaveData>();
        public List<InventorySlotSaveData> BankSlotSaveData = new List<InventorySlotSaveData>();
        public List<EquippedBagSaveData> EquippedBagSaveData = new List<EquippedBagSaveData>();
        public List<EquippedBagSaveData> EquippedBankBagSaveData = new List<EquippedBagSaveData>();
        public List<AbilitySaveData> AbilitySaveData = new List<AbilitySaveData>();
        public List<SkillSaveData> SkillSaveData = new List<SkillSaveData>();
        public List<RecipeSaveData> RecipeSaveData = new List<RecipeSaveData>();
        public List<ReputationSaveData> ReputationSaveData = new List<ReputationSaveData>();
        public List<EquipmentInventorySlotSaveData> EquipmentSaveData = new List<EquipmentInventorySlotSaveData>();
        public List<CurrencySaveData> CurrencySaveData = new List<CurrencySaveData>();
        public List<StatusEffectSaveData> StatusEffectSaveData = new List<StatusEffectSaveData>();
        public List<PetSaveData> PetSaveData = new List<PetSaveData>();
        public List<BehaviorSaveData> BehaviorSaveData = new List<BehaviorSaveData>();
        public List<QuestSaveData> QuestSaveData = new List<QuestSaveData>();
        public List<QuestSaveData> AchievementSaveData = new List<QuestSaveData>();
        public List<DialogSaveData> DialogSaveData = new List<DialogSaveData>();
        public List<SceneNodeSaveData> SceneNodeSaveData = new List<SceneNodeSaveData>();
        public List<CutsceneSaveData> CutsceneSaveData = new List<CutsceneSaveData>();
    }


    [Serializable]
    public struct ResourcePowerSaveData {

        public string ResourceName;
        public float Amount;
    }

    [Serializable]
    public struct SwappableMeshSaveData {

        public string GroupName;
        public string OptionName;
    }

    [Serializable]
    public class QuestSaveData {

        public string QuestName;
        public int QuestStep;
        public bool TurnedIn;
        public bool MarkedComplete;
        public bool InLog;

        public List<QuestObjectiveSaveData> QuestObjectives = new List<QuestObjectiveSaveData>();
    }

    [Serializable]
    public struct StatusEffectSaveData {

        public string StatusEffectName;
        public int RemainingSeconds;
    }

    [Serializable]
    public class SceneNodeSaveData {

        public string SceneName;
        public bool Visited;

        public List<PersistentObjectSaveData> PersistentObjects = new List<PersistentObjectSaveData>();
    }

    [Serializable]
    public struct CutsceneSaveData {

        public string CutsceneName;

        public bool IsCutSceneViewed;
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
        public bool TurnedIn;

        public List<bool> DialogNodeShown = new List<bool>();
    }

    [Serializable]
    public struct BehaviorSaveData {

        public string BehaviorName;
        public bool Completed;
    }

    [Serializable]
    public struct ActionBarSaveData {

        public string DisplayName;
        public bool IsItem;
        public string SavedName;
        public bool SavedIsItem;
    }

    [Serializable]
    public class InventorySlotSaveData {

        public List<long> ItemInstanceIds = new List<long>();
    }

    [Serializable]
    public struct QuestObjectiveSaveData {

        public string ObjectiveType;
        public string ObjectiveName;
        public int Amount;
    }

    [Serializable]
    public struct EquippedBagSaveData {

        public bool HasItem;
        public long ItemInstanceId;
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
    public class EquipmentInventorySlotSaveData {
        public bool HasItem;
        public long ItemInstanceId;
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