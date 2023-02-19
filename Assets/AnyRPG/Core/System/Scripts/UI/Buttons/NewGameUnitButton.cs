using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameUnitButton : HighlightButton {

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI unitName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        //[SerializeField]
        protected UnitProfile unitProfile = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

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
            descriptionText += unitProfile.Description + "\n";

            // set the text on the button
            description.text = descriptionText;
        }

        /*
        public override void Interact() {
            Debug.Log(gameObject.name + ".NewGameUnitButton.Interact()");

            base.Interact();
            newGameManager.SetUnitProfile(unitProfile);
        }
        */

        public override void ButtonClickAction() {
            //Debug.Log(gameObject.name + ".NewGameUnitButton.ButtonClickAction()");
            base.ButtonClickAction();

            newGameManager.SetUnitProfile(unitProfile);
        }

        
        public void CommonSelect() {
            //Debug.Log(gameObject.name + ".NewGameUnitButton.CommonSelect()");
            newGameManager.SetUnitProfile(unitProfile);
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