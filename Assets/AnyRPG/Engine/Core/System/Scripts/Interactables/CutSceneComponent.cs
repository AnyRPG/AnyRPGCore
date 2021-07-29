using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneComponent : InteractableOptionComponent {

        public CutsceneProps Props { get => interactableOptionProps as CutsceneProps; }

        public CutSceneComponent(Interactable interactable, CutsceneProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);
            //Debug.Log(gameObject.name + ".CutSceneInteractable.Interact()");
            // save character position and stuff here
            //SystemGameManager.Instance.UIManager.PopupWindowManager.interactionWindow.CloseWindow();
            if (Props.Cutscene != null
                && SystemGameManager.Instance.UIManager.CutSceneBarController.CurrentCutscene == null
                && SystemGameManager.Instance.LevelManager.LoadingLevel == false) {
                if (Props.Cutscene.Viewed == false || Props.Cutscene.Repeatable == true) {
                    if (Props.Cutscene.RequirePlayerUnitSpawn == false || (Props.Cutscene.RequirePlayerUnitSpawn == true && SystemGameManager.Instance.PlayerManager.PlayerUnitSpawned == true)) {
                        if (Props.Cutscene.MyLoadScene != null) {
                            SystemGameManager.Instance.LevelManager.LoadCutSceneWithDelay(Props.Cutscene);
                        } else {
                            SystemGameManager.Instance.UIManager.CutSceneBarController.StartCutScene(Props.Cutscene);
                        }
                    }
                }
            }
            // CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
            SystemGameManager.Instance.UIManager.PopupWindowManager.interactionWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.PopupWindowManager.questGiverWindow.CloseWindow();
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            //SystemGameManager.Instance.UIManager.PopupWindowManager.dialogWindow.CloseWindow();
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