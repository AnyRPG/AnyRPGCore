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
        private AnyRPGSaveData mySaveData;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public AnyRPGSaveData MySaveData { get => mySaveData; set => mySaveData = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSaveData(AnyRPGSaveData mySaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.mySaveData = mySaveData;

            icon.sprite = null;
            if (mySaveData.playerFaction != null && MySaveData.playerFaction != string.Empty) {
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
            string descriptionText = string.Empty;
            if (systemConfigurationManager.NewGameFaction == true) {
                descriptionText += "Faction: " + (mySaveData.playerFaction == null || mySaveData.playerFaction == string.Empty ? "None" : MySaveData.playerFaction) + "\n";
            }
            if (systemConfigurationManager.NewGameClass == true) {
                descriptionText += "Class: " + (mySaveData.characterClass == null || mySaveData.characterClass == string.Empty ? "None" : mySaveData.characterClass) + "\n";
                descriptionText += "Specialization: " + (mySaveData.classSpecialization == null || mySaveData.classSpecialization == string.Empty ? "None" : MySaveData.classSpecialization) + "\n";
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