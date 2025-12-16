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
            if (playerCharacterSaveData.CharacterSaveData.CharacterFaction != null && PlayerCharacterSaveData.CharacterSaveData.CharacterFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(playerCharacterSaveData.CharacterSaveData.CharacterFaction);
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
            playerName.text = playerCharacterSaveData.CharacterSaveData.CharacterName;

            // format the button text
            description.text = string.Empty;
            currentScene = string.Empty;
            // todo : fix; save scene DisplayName to file and load scene from resource description to avoid this loop
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile == playerCharacterSaveData.CharacterSaveData.CurrentScene) {
                    currentScene = sceneNode.DisplayName;
                    break;
                }
            }
            if (currentScene == null || currentScene == string.Empty) {
                currentScene = playerCharacterSaveData.CharacterSaveData.CurrentScene;
            }
            description.text += "Zone: " + currentScene + "\n";
            description.text += "Race: " + (playerCharacterSaveData.CharacterSaveData.CharacterRace == null || playerCharacterSaveData.CharacterSaveData.CharacterRace == string.Empty ? "None" : playerCharacterSaveData.CharacterSaveData.CharacterRace) + "\n";
            description.text += "Class: " + (playerCharacterSaveData.CharacterSaveData.CharacterClass == null || playerCharacterSaveData.CharacterSaveData.CharacterClass == string.Empty ? "None" : playerCharacterSaveData.CharacterSaveData.CharacterClass) + "\n";
            description.text += "Level: " + playerCharacterSaveData.CharacterSaveData.CharacterLevel + "\n";
            description.text += "Experience: " + playerCharacterSaveData.CharacterSaveData.CurrentExperience + "\n";
            description.text += "Faction: " + (playerCharacterSaveData.CharacterSaveData.CharacterFaction == string.Empty ? "None" : playerCharacterSaveData.CharacterSaveData.CharacterFaction) + "\n";
            description.text += "Created: " + playerCharacterSaveData.CharacterSaveData.DataCreatedOn + "\n";
            description.text += "Saved: " + playerCharacterSaveData.CharacterSaveData.DataSavedOn + "\n";
            //description.text += "FileName: " + playerCharacterSaveData.CharacterSaveData.CharacterId + "\n";

            // set the text on the button
            /*
            description.text = descriptionText;
            */

            unitProfile = systemDataFactory.GetResource<UnitProfile>(playerCharacterSaveData.CharacterSaveData.UnitProfileName);
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