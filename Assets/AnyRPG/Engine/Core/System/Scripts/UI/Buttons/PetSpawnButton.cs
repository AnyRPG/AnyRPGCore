using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PetSpawnButton : HighlightButton {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI unitName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        private PetSpawnControlPanel petSpawnControlPanel = null;
        private SystemConfigurationManager systemConfigurationManager = null;

        //[SerializeField]
        private UnitProfile unitProfile;

        public UnitProfile MyUnitProfile { get => unitProfile; set => unitProfile = value; }

        public void Init(PetSpawnControlPanel petSpawnControlPanel) {
            this.petSpawnControlPanel = petSpawnControlPanel;
            systemConfigurationManager = SystemGameManager.Instance.SystemConfigurationManager;
        }

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
            descriptionText += "Description: " + unitProfile.MyDescription + "\n";
            descriptionText += "Default Toughness: " + unitProfile.DefaultToughness + "\n";

            // set the text on the button
            description.text = descriptionText;
        }


        public void CommonSelect() {
            if (petSpawnControlPanel.MySelectedPetSpawnButton != null && petSpawnControlPanel.MySelectedPetSpawnButton != this) {
                petSpawnControlPanel.MySelectedPetSpawnButton.DeSelect();
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