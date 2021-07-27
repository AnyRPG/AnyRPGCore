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

        [SerializeField]
        private Image icon = null;

        //[SerializeField]
        //private TextMeshProUGUI playerName = null;

        [SerializeField]
        private TextMeshProUGUI playerName = null;

        //[SerializeField]
        //private TextMeshProUGUI description = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        [SerializeField]
        private AnyRPGSaveData mySaveData;

        private UnitProfile unitProfile;

        public AnyRPGSaveData SaveData { get => mySaveData; set => mySaveData = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public void AddSaveData(AnyRPGSaveData mySaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.mySaveData = mySaveData;

            icon.sprite = null;
            if (mySaveData.playerFaction != null && SaveData.playerFaction != string.Empty) {
                Faction playerFaction = SystemFactionManager.Instance.GetResource(mySaveData.playerFaction);
                // needs to be checked anyway.  could have invalid faction in save data
                if (playerFaction != null) {
                    icon.sprite = playerFaction.Icon;
                } else {
                    icon.sprite = SystemConfigurationManager.Instance.DefaultFactionIcon;
                }
            } else {
                icon.sprite = SystemConfigurationManager.Instance.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            playerName.text = mySaveData.playerName;

            // format the button text
            string descriptionText = string.Empty;
            descriptionText += "Zone: " + mySaveData.CurrentScene + "\n";
            descriptionText += "Race: " + (mySaveData.characterRace == null || mySaveData.characterRace == string.Empty ? "None" : mySaveData.characterRace) + "\n";
            descriptionText += "Class: " + (mySaveData.characterClass == null || mySaveData.characterClass == string.Empty ? "None" : mySaveData.characterClass) + "\n";
            descriptionText += "Level: " + mySaveData.PlayerLevel + "\n";
            descriptionText += "Experience: " + mySaveData.currentExperience + "\n";
            descriptionText += "Faction: " + (mySaveData.playerFaction == string.Empty ? "None" : SaveData.playerFaction) + "\n";
            descriptionText += "Created: " + mySaveData.DataCreatedOn + "\n";
            descriptionText += "Saved: " + mySaveData.DataSavedOn + "\n";
            descriptionText += "FileName: " + mySaveData.DataFileName + "\n";

            // set the text on the button
            description.text = descriptionText;

            unitProfile = SystemUnitProfileManager.Instance.GetResource(mySaveData.unitProfileName);
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
            if (LoadGamePanel.Instance.SelectedLoadGameButton != null && LoadGamePanel.Instance.SelectedLoadGameButton != this) {
                LoadGamePanel.Instance.SelectedLoadGameButton.DeSelect();
            }
            LoadGamePanel.Instance.ShowSavedGame(this);
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