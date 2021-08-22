using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneComponent : InteractableOptionComponent {

        // game manager references
        private LevelManager levelManager = null;
        private UIManager uIManager = null;
        private CutSceneBarController cutSceneBarController = null;

        public CutsceneProps Props { get => interactableOptionProps as CutsceneProps; }

        public CutSceneComponent(Interactable interactable, CutsceneProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
            cutSceneBarController = uIManager.CutSceneBarController;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);
            //Debug.Log(gameObject.name + ".CutSceneInteractable.Interact()");
            // save character position and stuff here
            //uIManager.interactionWindow.CloseWindow();
            if (Props.Cutscene != null
                && cutSceneBarController.CurrentCutscene == null
                && levelManager.LoadingLevel == false) {
                if (Props.Cutscene.Viewed == false || Props.Cutscene.Repeatable == true) {
                    if (Props.Cutscene.RequirePlayerUnitSpawn == false || (Props.Cutscene.RequirePlayerUnitSpawn == true && playerManager.PlayerUnitSpawned == true)) {
                        if (Props.Cutscene.MyLoadScene != null) {
                            levelManager.LoadCutSceneWithDelay(Props.Cutscene);
                        } else {
                            cutSceneBarController.StartCutScene(Props.Cutscene);
                        }
                    }
                }
            }
            // CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
            uIManager.interactionWindow.CloseWindow();
            uIManager.questGiverWindow.CloseWindow();
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            //uIManager.dialogWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.color = Color.white;
            return true;
        }

    }

}