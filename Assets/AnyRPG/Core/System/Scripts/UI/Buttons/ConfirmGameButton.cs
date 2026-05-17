using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ConfirmGameButton : ConfiguredMonoBehaviour {


        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI playerName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        [SerializeField]
        private CharacterSaveData mySaveData;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public CharacterSaveData MySaveData { get => mySaveData; set => mySaveData = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(CharacterSaveData mySaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.mySaveData = mySaveData;

            icon.sprite = null;
            if (mySaveData.CharacterFaction != null && MySaveData.CharacterFaction != string.Empty) {
                Faction playerFaction = systemDataFactory.GetResource<Faction>(mySaveData.CharacterFaction);
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
            playerName.text = mySaveData.CharacterName;

            // format the button text
            string descriptionText = string.Empty;
            if (systemConfigurationManager.NewGameFaction == true) {
                descriptionText += "Faction: " + (mySaveData.CharacterFaction == null || mySaveData.CharacterFaction == string.Empty ? "None" : MySaveData.CharacterFaction) + "\n";
            }
            if (systemConfigurationManager.NewGameClass == true) {
                descriptionText += "Class: " + (mySaveData.CharacterClass == null || mySaveData.CharacterClass == string.Empty ? "None" : mySaveData.CharacterClass) + "\n";
                descriptionText += "Specialization: " + (mySaveData.ClassSpecialization == null || mySaveData.ClassSpecialization == string.Empty ? "None" : MySaveData.ClassSpecialization) + "\n";
            }

            // set the text on the button
            description.text = descriptionText;
        }

        /*
        public void ClearSaveData() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            factionName.text = string.Empty;
            description.text = string.Empty;
        }
        */

    }

}