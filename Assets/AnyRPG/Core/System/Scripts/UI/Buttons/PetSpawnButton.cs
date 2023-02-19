using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PetSpawnButton : HighlightButton {

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI unitName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        protected PetSpawnControlPanel petSpawnControlPanel = null;

        //[SerializeField]
        protected UnitProfile unitProfile;

        public UnitProfile MyUnitProfile { get => unitProfile; set => unitProfile = value; }
        public PetSpawnControlPanel PetSpawnControlPanel { get => petSpawnControlPanel; set => petSpawnControlPanel = value; }

        public void AddUnitProfile(UnitProfile unitProfile) {
            //Debug.Log("UnitSpawnButton.AddUnitProfile()");
            this.unitProfile = unitProfile;

            icon.sprite = null;
            if (unitProfile.Icon != null) {
                icon.sprite = unitProfile.Icon;
            } else {
                icon.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            unitName.text = unitProfile.DisplayName;

            // format the button text
            string descriptionText = string.Empty;
            descriptionText += "Description: " + unitProfile.Description + "\n";
            descriptionText += "Default Toughness: " + unitProfile.DefaultToughness + "\n";

            // set the text on the button
            description.text = descriptionText;
        }


        public void CommonSelect() {
            if (petSpawnControlPanel.SelectedPetSpawnButton != null && petSpawnControlPanel.SelectedPetSpawnButton != this) {
                petSpawnControlPanel.SelectedPetSpawnButton.DeSelect();
            }
            petSpawnControlPanel.ShowUnit(this);
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