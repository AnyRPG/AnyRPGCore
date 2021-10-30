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

        [SerializeField]
        protected AnyRPGSaveData mySaveData;

        protected UnitProfile unitProfile;

        protected string currentScene = string.Empty;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;

        public AnyRPGSaveData SaveData { get => mySaveData; set => mySaveData = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public override bool DeselectOnLeave => false;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(LoadGamePanel loadGamePanel, AnyRPGSaveData mySaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.loadGamePanel = loadGamePanel;
            this.mySaveData = mySaveData;

            icon.sprite = null;
            if (mySaveData.playerFaction != null && SaveData.playerFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(mySaveData.playerFaction);
                // needs to be checked anyway.  could have invalid faction in save data
                if (playerFaction != null) {
                    icon.sprite = playerFaction.Icon;
                } else {
                    icon.sprite = systemConfigurationManager.DefaultFactionIcon;
                }
            } else {
                icon.sprite = systemConfigurationManager.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            playerName.text = mySaveData.playerName;

            // format the button text
            description.text = string.Empty;
            currentScene = string.Empty;
            // todo : fix; save scene DisplayName to file and load scene from resource description to avoid this loop
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile == mySaveData.CurrentScene) {
                    currentScene = sceneNode.DisplayName;
                    break;
                }
            }
            if (currentScene == null || currentScene == string.Empty) {
                currentScene = mySaveData.CurrentScene;
            }
            description.text += "Zone: " + currentScene + "\n";
            description.text += "Race: " + (mySaveData.characterRace == null || mySaveData.characterRace == string.Empty ? "None" : mySaveData.characterRace) + "\n";
            description.text += "Class: " + (mySaveData.characterClass == null || mySaveData.characterClass == string.Empty ? "None" : mySaveData.characterClass) + "\n";
            description.text += "Level: " + mySaveData.PlayerLevel + "\n";
            description.text += "Experience: " + mySaveData.currentExperience + "\n";
            description.text += "Faction: " + (mySaveData.playerFaction == string.Empty ? "None" : SaveData.playerFaction) + "\n";
            description.text += "Created: " + mySaveData.DataCreatedOn + "\n";
            description.text += "Saved: " + mySaveData.DataSavedOn + "\n";
            description.text += "FileName: " + mySaveData.DataFileName + "\n";

            // set the text on the button
            /*
            description.text = descriptionText;
            */

            unitProfile = systemDataFactory.GetResource<UnitProfile>(mySaveData.unitProfileName);
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
            if (loadGamePanel.SelectedLoadGameButton != null && loadGamePanel.SelectedLoadGameButton != this) {
                loadGamePanel.SelectedLoadGameButton.DeSelect();
            }
            loadGamePanel.ShowSavedGame(this);
        }

        public void RawSelect() {
            CommonSelect();
        }

        public override void Select() {
            CommonSelect();
            base.Select();
        }

    }

}