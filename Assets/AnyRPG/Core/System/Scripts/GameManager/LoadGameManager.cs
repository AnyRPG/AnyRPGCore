using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGameManager : ConfiguredMonoBehaviour, ICapabilityConsumer, ICharacterConfigurationProvider {

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

        private PlayerCharacterSaveData playerCharacterSaveData;

        //private LoadGameButton selectedLoadGameButton = null;

        private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        // game manager references
        private SaveManager saveManager = null;
        private ObjectPooler objectPooler = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private NetworkManagerClient networkManager = null;

        // public properties
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public PlayerCharacterSaveData PlayerCharacterSaveData { get => playerCharacterSaveData; set => playerCharacterSaveData = value; }
        public CapabilityConsumerSnapshot CapabilityConsumerSnapshot { get => capabilityConsumerSnapshot; set => capabilityConsumerSnapshot = value; }
        public List<PlayerCharacterSaveData> CharacterList { get => characterList; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            //Debug.Log("LoadGameManager.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            networkManager = systemGameManager.NetworkManagerClient;
        }


        public void SetSavedGame(PlayerCharacterSaveData saveData) {
            Debug.Log("LoadGameManager.SetSavedGame()");

            playerCharacterSaveData = saveData;
            capabilityConsumerSnapshot = saveManager.GetCapabilityConsumerSnapshot(playerCharacterSaveData.SaveData);

            unitProfile = capabilityConsumerSnapshot.UnitProfile;
            UnitType = capabilityConsumerSnapshot.UnitProfile?.UnitType;
            characterRace = capabilityConsumerSnapshot.CharacterRace;
            characterClass = capabilityConsumerSnapshot.CharacterClass;
            classSpecialization = capabilityConsumerSnapshot.ClassSpecialization;
            faction = capabilityConsumerSnapshot.Faction;

            saveManager.ClearSharedData();
        }

        public void ResetData() {
            //Debug.Log("LoadGameManager.ResetData()");
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            playerCharacterSaveData = saveManager.CreateSaveData();
            capabilityConsumerSnapshot = null;
        }


        public void LoadGame(PlayerCharacterSaveData playerCharacterSaveData) {
            Debug.Log("LoadGameManager.LoadGame()");

            //if (systemGameManager.GameMode == GameMode.Local) {
            saveManager.LoadGame(playerCharacterSaveData);
            //} else {
            //    networkManager.LoadGame(playerCharacterSaveData);
            //}
        }

        public void DeleteGame() {
            if (systemGameManager.GameMode == GameMode.Local) {
                saveManager.DeleteGame(playerCharacterSaveData.SaveData);
                OnDeleteGame();
            } else {
                networkManager.DeletePlayerCharacter(playerCharacterSaveData.PlayerCharacterId);
            }
        }

        public void CopyGame() {
            saveManager.CopyGame(playerCharacterSaveData.SaveData);
            OnCopyGame();
        }

        public void LoadCharacterList() {
            if (systemGameManager.GameMode == GameMode.Local) {
                LoadCharacterListLocal();
            } else {
                networkManager.LoadCharacterList();
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
            characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(playerCharacterSaveData.SaveData);
            return characterConfigurationRequest;
        }
    }

}