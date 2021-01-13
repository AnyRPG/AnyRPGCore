using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ConfirmGameButton : MonoBehaviour {


        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI playerName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        [SerializeField]
        private AnyRPGSaveData mySaveData;

        public AnyRPGSaveData MySaveData { get => mySaveData; set => mySaveData = value; }

        public void AddSaveData(AnyRPGSaveData mySaveData) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.mySaveData = mySaveData;

            icon.sprite = null;
            if (mySaveData.playerFaction != null && MySaveData.playerFaction != string.Empty) {
                Faction playerFaction = SystemFactionManager.MyInstance.GetResource(mySaveData.playerFaction);
                // needs to be checked anyway.  could have invalid faction in save data
                if (playerFaction != null) {
                    icon.sprite = playerFaction.Icon;
                } else {
                    icon.sprite = SystemConfigurationManager.MyInstance.DefaultFactionIcon;
                }
            } else {
                icon.sprite = SystemConfigurationManager.MyInstance.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            playerName.text = mySaveData.playerName;

            // format the button text
            string descriptionText = string.Empty;
            if (SystemConfigurationManager.MyInstance.NewGameFaction == true) {
                descriptionText += "Faction: " + (mySaveData.playerFaction == null || mySaveData.playerFaction == string.Empty ? "None" : MySaveData.playerFaction) + "\n";
            }
            if (SystemConfigurationManager.MyInstance.NewGameClass == true) {
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