using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class LoadGameManager : ConfiguredClass, ICapabilityConsumer, ICharacterConfigurationProvider {

        //public event System.Action<LoadGameButton> OnSetSavedGame = delegate { };
        public event System.Action OnDeleteGame = delegate { };
        public event System.Action OnCopyGame = delegate { };
        public event System.Action OnLoadCharacterList = delegate { };

        private List<PlayerCharacterSaveData> characterList = new List<PlayerCharacterSaveData>();
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private CharacterSaveData characterSaveData;

        //private LoadGameButton selectedLoadGameButton = null;

        private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        // game manager references
        private SaveManager saveManager = null;

        // public properties
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public CharacterSaveData CharacterSaveData { get => characterSaveData; set => characterSaveData = value; }
        public CapabilityConsumerSnapshot CapabilityConsumerSnapshot { get => capabilityConsumerSnapshot; set => capabilityConsumerSnapshot = value; }
        public List<PlayerCharacterSaveData> CharacterList { get => characterList; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            //Debug.Log("LoadGameManager.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }


        public void SetSavedGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("LoadGameManager.SetSavedGame()");

            characterSaveData = playerCharacterSaveData.CharacterSaveData;
            capabilityConsumerSnapshot = saveManager.GetCapabilityConsumerSnapshot(characterSaveData);

            unitProfile = capabilityConsumerSnapshot.UnitProfile;
            UnitType = capabilityConsumerSnapshot.UnitProfile?.UnitType;
            characterRace = capabilityConsumerSnapshot.CharacterRace;
            characterClass = capabilityConsumerSnapshot.CharacterClass;
            classSpecialization = capabilityConsumerSnapshot.ClassSpecialization;
            faction = capabilityConsumerSnapshot.Faction;

            saveManager.ClearSharedData();
            systemItemManager.LoadPlayerCharacterSaveData(playerCharacterSaveData);
        }

        public void ResetData() {
            //Debug.Log("LoadGameManager.ResetData()");

            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            systemItemManager.ClientReset();
            characterSaveData = saveManager.CreateSaveData();
            capabilityConsumerSnapshot = null;
        }


        public void LoadGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("LoadGameManager.LoadGame()");

            if (systemGameManager.GameMode == GameMode.Local) {
                saveManager.LoadGame(playerCharacterSaveData);
            } else {
                networkManagerClient.RequestLoadPlayerCharacter(playerCharacterSaveData.CharacterSaveData.CharacterId);
            }
        }

        public void DeleteGame() {
            //Debug.Log("LoadGameManager.DeleteGame()");

            if (systemGameManager.GameMode == GameMode.Local) {
                saveManager.DeleteGame(characterSaveData);
                OnDeleteGame();
            } else {
                networkManagerClient.DeletePlayerCharacter(characterSaveData.CharacterId);
            }
        }

        public void CopyGame() {
            saveManager.CopyGame(characterSaveData);
            OnCopyGame();
        }

        public void LoadCharacterList() {
            if (systemGameManager.GameMode == GameMode.Local) {
                LoadCharacterListLocal();
            } else {
                networkManagerClient.LoadCharacterList();
            }
        }

        private void LoadCharacterListLocal() {

            characterList.Clear();
            characterList.AddRange(saveManager.GetSaveDataList());
            OnLoadCharacterList();
        }

        public void SetCharacterList(List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log("LoadGameManager.SetCharacterList()");

            characterList.Clear();
            characterList.AddRange(playerCharacterSaveDataList);
            OnLoadCharacterList();
        }

        public CharacterConfigurationRequest GetCharacterConfigurationRequest() {
            //Debug.Log("LoadGameManager.GetCharacterConfigurationRequest()");

            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(this);
            characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(characterSaveData);
            return characterConfigurationRequest;
        }
    }

}