using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameUnitButton : HighlightButton {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI unitName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        //[SerializeField]
        private UnitProfile unitProfile = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public void AddUnitProfile(UnitProfile unitProfile) {
            //Debug.Log("UnitSpawnButton.AddUnitProfile()");
            this.unitProfile = unitProfile;

            icon.sprite = null;
            if (unitProfile.MyIcon != null) {
                icon.sprite = unitProfile.MyIcon;
            } else {
                icon.sprite = SystemConfigurationManager.MyInstance.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            unitName.text = unitProfile.DisplayName;

            // format the button text
            string descriptionText = string.Empty;
            //descriptionText += "Description: " + unitProfile.MyDescription + "\n";
            descriptionText += unitProfile.MyDescription + "\n";

            // set the text on the button
            description.text = descriptionText;
        }


        public void CommonSelect() {
            NewGamePanel.MyInstance.SetUnitProfile(this);
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