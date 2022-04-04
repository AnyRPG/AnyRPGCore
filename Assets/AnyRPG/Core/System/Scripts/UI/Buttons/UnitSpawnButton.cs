using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnButton : HighlightButton {

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI unitName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        //[SerializeField]
        protected UnitProfile unitProfile = null;

        protected UnitSpawnControlPanel unitSpawnControlPanel = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitSpawnControlPanel UnitSpawnControlPanel { get => unitSpawnControlPanel; set => unitSpawnControlPanel = value; }


        public void AddUnitProfile(UnitProfile unitProfile) {
            //Debug.Log("UnitSpawnButton.AddUnitProfile()");
            this.unitProfile = unitProfile;

            icon.sprite = null;
            if (unitProfile.Icon != null) {
                icon.sprite = unitProfile.Icon;
            } else {
                icon.sprite = systemConfigurationManager.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            unitName.text = unitProfile.DisplayName;

            // format the button text
            string descriptionText = string.Empty;
            descriptionText += unitProfile.Description + "\n";
            descriptionText += "Default Toughness: " + (unitProfile.DefaultToughness == null ? "Normal" : unitProfile.DefaultToughness.DisplayName) + "\n";

            // set the text on the button
            description.text = descriptionText;
        }


        public void CommonSelect() {
            //Debug.Log("UnitSpawnButton.CommonSelect() " + unitProfile.DisplayName);

            if (unitSpawnControlPanel.SelectedUnitSpawnButton != null && unitSpawnControlPanel.SelectedUnitSpawnButton != this) {
                unitSpawnControlPanel.SelectedUnitSpawnButton.DeSelect();
            }
            unitSpawnControlPanel.ShowUnit(this);
        }

        /*
        public void RawSelect() {
            CommonSelect();
        }
        */

        public override void Select() {
            CommonSelect();
            base.Select();
        }

    }

}