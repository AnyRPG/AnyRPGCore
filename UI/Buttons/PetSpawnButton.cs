using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PetSpawnButton : HighlightButton {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Text unitName = null;

        [SerializeField]
        private Text description = null;

        //[SerializeField]
        private UnitProfile unitProfile;

        public UnitProfile MyUnitProfile { get => unitProfile; set => unitProfile = value; }

        public void AddUnitProfile(UnitProfile unitProfile) {
            //Debug.Log("UnitSpawnButton.AddUnitProfile()");
            this.unitProfile = unitProfile;

            icon.sprite = null;
            if (unitProfile.MyIcon != null) {
                icon.sprite = unitProfile.MyIcon;
            } else {
                icon.sprite = SystemFactionManager.MyInstance.MyDefaultIcon;
            }
            icon.color = Color.white;
            //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
            //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
            unitName.text = unitProfile.MyName;

            // format the button text
            string descriptionText = string.Empty;
            descriptionText += "Description: " + unitProfile.MyDescription + "\n";
            descriptionText += "Default Toughness: " + unitProfile.MyDefaultToughness + "\n";

            // set the text on the button
            description.text = descriptionText;
        }


        public void CommonSelect() {
            if (PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton != null && PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton != this) {
                PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton.DeSelect();
            }
            PetSpawnControlPanel.MyInstance.ShowUnit(this);
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