using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class PortalComponent : InteractableOptionComponent {

        // game manager references
        protected LevelManagerClient levelManagerClient = null;

        public PortalProps Props { get => interactableOptionProps as PortalProps; }

        public PortalComponent(Interactable interactable, PortalProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.PortalComponent.Interact({sourceUnitController.gameObject.name})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            //Debug.Log($"{gameObject.name}.PortalInteractable.Interact(): about to close interaction window");
            //Debug.Log($"{gameObject.name}.PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest();
            if (Props.OverrideSpawnDirection == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = Props.SpawnForwardDirection;
            }
            if (Props.OverrideSpawnLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = Props.SpawnLocation;
            } else {
                if (Props.LocationTag != null && Props.LocationTag != string.Empty) {
                    loadSceneRequest.locationTag = Props.LocationTag;
                }
            }
            playerManagerServer.AddSpawnRequest(sourceUnitController, loadSceneRequest);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.PortalInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

    }

}