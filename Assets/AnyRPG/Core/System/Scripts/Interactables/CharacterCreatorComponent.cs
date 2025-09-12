using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorComponent : InteractableOptionComponent {

        private CharacterCreatorManager characterCreatorManager = null;
        private CharacterAppearanceManagerClient characterCreatorInteractableManager = null;

        public CharacterCreatorProps Props { get => interactableOptionProps as CharacterCreatorProps; }

        public CharacterCreatorComponent(Interactable interactable, CharacterCreatorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            characterCreatorInteractableManager = systemGameManager.CharacterAppearanceManagerClient;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            // was there a reason why we didn't have base.Interact here before or just an oversight?
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            // moved to coroutine because UMA will crash here due to its use of DestroyImmediate in the case where an UMAData was attached to the model.
            characterCreatorInteractableManager.SetCharacterCreator(this, componentIndex, choiceIndex);
            interactable.StartCoroutine(OpenWindowWait());
        }

        public IEnumerator OpenWindowWait() {
            yield return null;
            OpenWindow();
        }

        public void OpenWindow() {
            uIManager.characterCreatorWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        public void UpdatePlayerAppearance(UnitController sourceUnitController, int accountId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.CharacterCreatorComponent.UpdatePlayerAppearance({sourceUnitController.gameObject.name}, {accountId}, {unitProfileName})");

            // notify first because unit controller might no longer exist after update
            NotifyOnConfirmAction(sourceUnitController);

            playerManagerServer.UpdatePlayerAppearance(accountId, unitProfileName, appearanceString, swappableMeshSaveData);
        }
    }

}