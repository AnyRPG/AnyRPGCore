using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadGameButton : HighlightButton {

        [Header("Load Game Button")]

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI playerName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        protected PlayerCharacterSaveData playerCharacterData;

        LoadGamePanel loadGamePanel = null;

        protected UnitProfile unitProfile;

        protected string currentScene = string.Empty;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;

        public PlayerCharacterSaveData PlayerCharacterSaveData { get => playerCharacterData; set => playerCharacterData = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(LoadGamePanel loadGamePanel, PlayerCharacterSaveData playerCharacterData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.loadGamePanel = loadGamePanel;
            this.playerCharacterData = playerCharacterData;

            icon.sprite = null;
            if (playerCharacterData.SaveData.playerFaction != null && PlayerCharacterSaveData.SaveData.playerFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(playerCharacterData.SaveData.playerFaction);
                // needs to be checked anyway.  could have invalid faction in save data
                if (playerFaction != null) {
                    icon.sprite = playerFaction.Icon;
                } else {
                    icon.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
                }
            } else {
                icon.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            playerName.text = playerCharacterData.SaveData.playerName;

            // format the button text
            description.text = string.Empty;
            currentScene = string.Empty;
            // todo : fix; save scene DisplayName to file and load scene from resource description to avoid this loop
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile == playerCharacterData.SaveData.CurrentScene) {
                    currentScene = sceneNode.DisplayName;
                    break;
                }
            }
            if (currentScene == null || currentScene == string.Empty) {
                currentScene = playerCharacterData.SaveData.CurrentScene;
            }
            description.text += "Zone: " + currentScene + "\n";
            description.text += "Race: " + (playerCharacterData.SaveData.characterRace == null || playerCharacterData.SaveData.characterRace == string.Empty ? "None" : playerCharacterData.SaveData.characterRace) + "\n";
            description.text += "Class: " + (playerCharacterData.SaveData.characterClass == null || playerCharacterData.SaveData.characterClass == string.Empty ? "None" : playerCharacterData.SaveData.characterClass) + "\n";
            description.text += "Level: " + playerCharacterData.SaveData.PlayerLevel + "\n";
            description.text += "Experience: " + playerCharacterData.SaveData.currentExperience + "\n";
            description.text += "Faction: " + (playerCharacterData.SaveData.playerFaction == string.Empty ? "None" : playerCharacterData.SaveData.playerFaction) + "\n";
            description.text += "Created: " + playerCharacterData.SaveData.DataCreatedOn + "\n";
            description.text += "Saved: " + playerCharacterData.SaveData.DataSavedOn + "\n";
            description.text += "FileName: " + playerCharacterData.SaveData.DataFileName + "\n";

            // set the text on the button
            /*
            description.text = descriptionText;
            */

            unitProfile = systemDataFactory.GetResource<UnitProfile>(playerCharacterData.SaveData.unitProfileName);
        }

        public void CommonSelect() {
            //Debug.Log($"{gameObject.name}.LoadGameButton.CommonSelect()");
            if (loadGamePanel.SelectedLoadGameButton != null && loadGamePanel.SelectedLoadGameButton != this) {
                loadGamePanel.SelectedLoadGameButton.DeSelect();
            }
            if (loadGamePanel.SelectedLoadGameButton != this) {
                loadGamePanel.ShowSavedGame(this);
            }
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.LoadGameButton.Select()");
            CommonSelect();
            base.Select();
        }

    }

}