using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGameManager : ConfiguredMonoBehaviour, ICapabilityConsumer {

        //public event System.Action<LoadGameButton> OnSetSavedGame = delegate { };
        public event System.Action OnDeleteGame = delegate { };
        public event System.Action OnCopyGame = delegate { };

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData anyRPGSaveData;

        //private LoadGameButton selectedLoadGameButton = null;

        private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        // game manager references
        private SaveManager saveManager = null;
        private ObjectPooler objectPooler = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private UIManager uIManager = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        //public LoadGameButton SelectedLoadGameButton { get => selectedLoadGameButton; set => selectedLoadGameButton = value; }
        public AnyRPGSaveData AnyRPGSaveData { get => anyRPGSaveData; set => anyRPGSaveData = value; }
        public CapabilityConsumerSnapshot CapabilityConsumerSnapshot { get => capabilityConsumerSnapshot; set => capabilityConsumerSnapshot = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            //Debug.Log("LoadGameManager.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            uIManager = systemGameManager.UIManager;
        }


        public void SetSavedGame(AnyRPGSaveData saveData) {
            //Debug.Log("LoadGameManager.SetSavedGame()");

            anyRPGSaveData = saveData;
            capabilityConsumerSnapshot = saveManager.GetCapabilityConsumerSnapshot(anyRPGSaveData);

            unitProfile = capabilityConsumerSnapshot.UnitProfile;
            UnitType = capabilityConsumerSnapshot.UnitProfile.UnitType;
            characterRace = capabilityConsumerSnapshot.CharacterRace;
            characterClass = capabilityConsumerSnapshot.CharacterClass;
            classSpecialization = capabilityConsumerSnapshot.ClassSpecialization;
            faction = capabilityConsumerSnapshot.Faction;

            saveManager.ClearSharedData();
            saveManager.LoadUMARecipe(anyRPGSaveData);

            //OnSetSavedGame(loadButton);
           
        }


        public void ResetData() {
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            anyRPGSaveData = new AnyRPGSaveData();
            capabilityConsumerSnapshot = null;
        }


        public void LoadGame(AnyRPGSaveData saveData) {
            saveManager.LoadGame(saveData);
        }

        public void DeleteGame() {
            saveManager.DeleteGame(anyRPGSaveData);
            OnDeleteGame();
        }

        public void CopyGame() {
            saveManager.CopyGame(anyRPGSaveData);
            OnCopyGame();
        }

    }

}