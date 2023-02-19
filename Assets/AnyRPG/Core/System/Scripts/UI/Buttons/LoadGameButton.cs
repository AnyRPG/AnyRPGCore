using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadGameButton : HighlightButton {

        //[SerializeField]
        //private Faction faction = null;

        LoadGamePanel loadGamePanel = null;

        [SerializeField]
        protected Image icon = null;

        //[SerializeField]
        //private TextMeshProUGUI playerName = null;

        [SerializeField]
        protected TextMeshProUGUI playerName = null;

        //[SerializeField]
        //private TextMeshProUGUI description = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        //[SerializeField]
        protected AnyRPGSaveData saveData;

        protected UnitProfile unitProfile;

        protected string currentScene = string.Empty;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;

        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(LoadGamePanel loadGamePanel, AnyRPGSaveData saveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.loadGamePanel = loadGamePanel;
            this.saveData = saveData;

            icon.sprite = null;
            if (saveData.playerFaction != null && SaveData.playerFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(saveData.playerFaction);
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
            playerName.text = saveData.playerName;

            // format the button text
            description.text = string.Empty;
            currentScene = string.Empty;
            // todo : fix; save scene DisplayName to file and load scene from resource description to avoid this loop
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile == saveData.CurrentScene) {
                    currentScene = sceneNode.DisplayName;
                    break;
                }
            }
            if (currentScene == null || currentScene == string.Empty) {
                currentScene = saveData.CurrentScene;
            }
            description.text += "Zone: " + currentScene + "\n";
            description.text += "Race: " + (saveData.characterRace == null || saveData.characterRace == string.Empty ? "None" : saveData.characterRace) + "\n";
            description.text += "Class: " + (saveData.characterClass == null || saveData.characterClass == string.Empty ? "None" : saveData.characterClass) + "\n";
            description.text += "Level: " + saveData.PlayerLevel + "\n";
            description.text += "Experience: " + saveData.currentExperience + "\n";
            description.text += "Faction: " + (saveData.playerFaction == string.Empty ? "None" : SaveData.playerFaction) + "\n";
            description.text += "Created: " + saveData.DataCreatedOn + "\n";
            description.text += "Saved: " + saveData.DataSavedOn + "\n";
            description.text += "FileName: " + saveData.DataFileName + "\n";

            // set the text on the button
            /*
            description.text = descriptionText;
            */

            unitProfile = systemDataFactory.GetResource<UnitProfile>(saveData.unitProfileName);
        }

        /*
        public void ClearSaveData() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            factionName.text = string.Empty;
            description.text = string.Empty;
        }
        */

        public void CommonSelect() {
            //Debug.Log(gameObject.name + ".LoadGameButton.CommonSelect()");
            if (loadGamePanel.SelectedLoadGameButton != null && loadGamePanel.SelectedLoadGameButton != this) {
                loadGamePanel.SelectedLoadGameButton.DeSelect();
            }
            if (loadGamePanel.SelectedLoadGameButton != this) {
                loadGamePanel.ShowSavedGame(this);
            }
        }

        /*
        public void RawSelect() {
            CommonSelect();
        }
        */

        public override void Select() {
            //Debug.Log(gameObject.name + ".LoadGameButton.Select()");
            CommonSelect();
            base.Select();
        }

    }

}