using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnButton : HighlightButton {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Text unitName = null;

        [SerializeField]
        private Text description = null;

        //[SerializeField]
        private UnitProfile unitProfile = null;

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
            if (UnitSpawnControlPanel.MyInstance.MySelectedUnitSpawnButton != null && UnitSpawnControlPanel.MyInstance.MySelectedUnitSpawnButton != this) {
                UnitSpawnControlPanel.MyInstance.MySelectedUnitSpawnButton.DeSelect();
            }
            UnitSpawnControlPanel.MyInstance.ShowUnit(this);
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