using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public CutsceneProps Props { get => interactableOptionProps as CutsceneProps; }

        public CutSceneComponent(Interactable interactable, CutsceneProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);
            //Debug.Log(gameObject.name + ".CutSceneInteractable.Interact()");
            // save character position and stuff here
            //PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (Props.Cutscene != null
                && UIManager.MyInstance.MyCutSceneBarController.CurrentCutscene == null
                && LevelManager.MyInstance.LoadingLevel == false) {
                if (Props.Cutscene.Viewed == false || Props.Cutscene.Repeatable == true) {
                    if (Props.Cutscene.RequirePlayerUnitSpawn == false || (Props.Cutscene.RequirePlayerUnitSpawn == true && PlayerManager.MyInstance.PlayerUnitSpawned == true)) {
                        if (Props.Cutscene.MyLoadScene != null) {
                            LevelManager.MyInstance.LoadCutSceneWithDelay(Props.Cutscene);
                        } else {
                            UIManager.MyInstance.MyCutSceneBarController.StartCutScene(Props.Cutscene);
                        }
                    }
                }
            }
            // CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            //PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
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
            text.fontSize = 50;
            text.color = Color.white;
            return true;
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}