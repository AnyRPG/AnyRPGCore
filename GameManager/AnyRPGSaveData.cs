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
        public int currentHealth;
        public int currentMana;
        public string playerName;
        public string playerFaction;
        public string characterClass;
        public string classSpecialization;
        public string unitProfileName;
        public float PlayerLocationX;
        public float PlayerLocationY;
        public float PlayerLocationZ;
        public float PlayerRotationX;
        public float PlayerRotationY;
        public float PlayerRotationZ;
        public string PlayerUMARecipe;
        public string CurrentScene;
        public string DataCreatedOn;
        public string DataSavedOn;
        public string DataFileName;

        public List<QuestSaveData> questSaveData;
        public List<ActionBarSaveData> actionBarSaveData;
        public List<InventorySlotSaveData> inventorySlotSaveData;
        public List<EquippedBagSaveData> equippedBagSaveData;
        public List<AbilitySaveData> abilitySaveData;
        public List<SkillSaveData> skillSaveData;
        public List<ReputationSaveData> reputationSaveData;
        public List<EquipmentSaveData> equipmentSaveData;
        public List<CurrencySaveData> currencySaveData;
        public List<DialogSaveData> dialogSaveData;
        public List<SceneNodeSaveData> sceneNodeSaveData;
        public List<CutsceneSaveData> cutsceneSaveData;
        public List<StatusEffectSaveData> statusEffectSaveData;
        public List<PetSaveData> petSaveData;
    }

    [Serializable]
    public struct QuestSaveData {

        public string MyName;
        public bool turnedIn;
        public bool inLog;
        public bool isAchievement;

        public List<QuestObjectiveSaveData> killObjectives;
        public List<QuestObjectiveSaveData> useInteractableObjectives;
        public List<QuestObjectiveSaveData> collectObjectives;
        public List<QuestObjectiveSaveData> tradeSkillObjectives;
        public List<QuestObjectiveSaveData> abilityObjectives;
    }

    [Serializable]
    public struct StatusEffectSaveData {

        public string MyName;
        public int remainingSeconds;
    }

    [Serializable]
    public struct SceneNodeSaveData {

        public string MyName;
        public bool isCutSceneViewed;

        public List<PersistentObjectSaveData> persistentObjects;
    }

    [Serializable]
    public struct CutsceneSaveData {

        public string MyName;
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

        public string MyName;
        public bool turnedIn;
    }


    [Serializable]
    public struct ActionBarSaveData {

        public string MyName;
        public bool isItem;
    }

    [Serializable]
    public struct InventorySlotSaveData {

        public string MyName;
        public int stackCount;
    }

    [Serializable]
    public struct QuestObjectiveSaveData {

        public string MyName;
        public int MyAmount;
        //public int MyCurrentAmount;
    }

    [Serializable]
    public struct EquippedBagSaveData {

        public string MyName;
        public string itemName;
        public int slotCount;
        public bool isBankBag;

    }

    [Serializable]
    public struct AbilitySaveData {

        public string MyName;

    }

    [Serializable]
    public struct PetSaveData {

        public string MyName;

    }

    [Serializable]
    public struct EquipmentSaveData {

        public string MyName;

    }

    [Serializable]
    public struct SkillSaveData {

        public string MyName;

    }

    [Serializable]
    public struct ReputationSaveData {

        public string MyName;
        public float MyAmount;

    }

    [Serializable]
    public struct CurrencySaveData {

        public string MyName;
        public int MyAmount;

    }

}