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

        protected PlayerCharacterSaveData playerCharacterSaveData;

        LoadGamePanel loadGamePanel = null;

        protected UnitProfile unitProfile;

        protected string currentScene = string.Empty;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;

        public PlayerCharacterSaveData PlayerCharacterSaveData { get => playerCharacterSaveData; set => playerCharacterSaveData = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(LoadGamePanel loadGamePanel, PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.loadGamePanel = loadGamePanel;
            this.playerCharacterSaveData = playerCharacterSaveData;

            icon.sprite = null;
            if (playerCharacterSaveData.SaveData.playerFaction != null && PlayerCharacterSaveData.SaveData.playerFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(playerCharacterSaveData.SaveData.playerFaction);
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
            playerName.text = playerCharacterSaveData.SaveData.playerName;

            // format the button text
            description.text = string.Empty;
            currentScene = string.Empty;
            // todo : fix; save scene DisplayName to file and load scene from resource description to avoid this loop
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile == playerCharacterSaveData.SaveData.CurrentScene) {
                    currentScene = sceneNode.DisplayName;
                    break;
                }
            }
            if (currentScene == null || currentScene == string.Empty) {
                currentScene = playerCharacterSaveData.SaveData.CurrentScene;
            }
            description.text += "Zone: " + currentScene + "\n";
            description.text += "Race: " + (playerCharacterSaveData.SaveData.characterRace == null || playerCharacterSaveData.SaveData.characterRace == string.Empty ? "None" : playerCharacterSaveData.SaveData.characterRace) + "\n";
            description.text += "Class: " + (playerCharacterSaveData.SaveData.characterClass == null || playerCharacterSaveData.SaveData.characterClass == string.Empty ? "None" : playerCharacterSaveData.SaveData.characterClass) + "\n";
            description.text += "Level: " + playerCharacterSaveData.SaveData.PlayerLevel + "\n";
            description.text += "Experience: " + playerCharacterSaveData.SaveData.currentExperience + "\n";
            description.text += "Faction: " + (playerCharacterSaveData.SaveData.playerFaction == string.Empty ? "None" : playerCharacterSaveData.SaveData.playerFaction) + "\n";
            description.text += "Created: " + playerCharacterSaveData.SaveData.DataCreatedOn + "\n";
            description.text += "Saved: " + playerCharacterSaveData.SaveData.DataSavedOn + "\n";
            description.text += "FileName: " + playerCharacterSaveData.SaveData.DataFileName + "\n";

            // set the text on the button
            /*
            description.text = descriptionText;
            */

            unitProfile = systemDataFactory.GetResource<UnitProfile>(playerCharacterSaveData.SaveData.unitProfileName);
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