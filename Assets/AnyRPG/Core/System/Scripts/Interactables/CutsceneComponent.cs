using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutsceneComponent : InteractableOptionComponent {

        // game manager references
        private LevelManager levelManager = null;
        private CutsceneBarController cutSceneBarController = null;

        public CutsceneProps Props { get => interactableOptionProps as CutsceneProps; }

        public CutsceneComponent(Interactable interactable, CutsceneProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            cutSceneBarController = uIManager.CutSceneBarController;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.CutsceneComponent.ProcessInteract({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)}, {componentIndex}, {choiceIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            if (CanLoadCutscene() == true) {
                if (Props.Cutscene.RequirePlayerUnitSpawn == false || (Props.Cutscene.RequirePlayerUnitSpawn == true && playerManager.PlayerUnitSpawned == true)) {
                    if (Props.Cutscene.LoadScene != null) {
                        playerManagerServer.LoadCutsceneWithDelay(Props.Cutscene, sourceUnitController);
                    }
                }

            }
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"CutSceneComponent.ClientInteraction({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            // save character position and stuff here
            //uIManager.interactionWindow.CloseWindow();
            if (CanLoadCutscene()) {
                if (Props.Cutscene.RequirePlayerUnitSpawn == false || (Props.Cutscene.RequirePlayerUnitSpawn == true && playerManager.PlayerUnitSpawned == true)) {
                    if (Props.Cutscene.LoadScene != null) {
                        //networkManagerClient.RequestDespawnPlayer();
                        //levelManager.LoadCutSceneWithDelay(Props.Cutscene);
                    } else {
                        cutSceneBarController.StartCutScene(Props.Cutscene);
                    }
                }
            }
            // CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
            uIManager.interactionWindow.CloseWindow();
            uIManager.questGiverWindow.CloseWindow();

        }

        private bool CanLoadCutscene() {
            if (Props.Cutscene == null) {
                Debug.LogError("CutSceneComponent.CanLoadCutScene(): Props.Cutscene is null");
                return false;
            }
            if (cutSceneBarController.CurrentCutscene != null) {
                Debug.LogError("CutSceneComponent.CanLoadCutScene(): cutSceneBarController.CurrentCutscene is not null");
                return false;
            }
            if (levelManager.LoadingLevel) {
                Debug.LogError("CutSceneComponent.CanLoadCutScene(): levelManager.LoadingLevel is true");
                return false;
            }
            if (Props.Cutscene.Viewed && Props.Cutscene.Repeatable == false) {
                Debug.LogError("CutSceneComponent.CanLoadCutScene(): Props.Cutscene.Viewed is true and Props.Cutscene.Repeatable is false");
                return false;
            }
            return true;
        }

    }

}