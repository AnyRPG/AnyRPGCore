using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterAppearanceManagerClient : InteractableOptionManager {

        CharacterCreatorComponent characterCreatorComponent = null;

        public CharacterCreatorComponent CharacterCreator { get => characterCreatorComponent; }

        public override void EndInteraction() {
            base.EndInteraction();

            characterCreatorComponent = null;
        }

        public void SetCharacterCreator(CharacterCreatorComponent characterCreator, int componentIndex, int choiceIndex) {
            //Debug.Log("CharacterCreatorInteractableManager.SetSkillTrainer(" + characterClass + ")");
            this.characterCreatorComponent = characterCreator;

            BeginInteraction(characterCreator, componentIndex, choiceIndex);
        }

        public void RequestUpdatePlayerAppearance(UnitController sourceUnitController, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            if (systemGameManager.GameMode == GameMode.Local) {
                //Debug.Log("PlayerManager.RequestUpdatePlayerAppearance() - local mode, updating appearance string");
                characterCreatorComponent.UpdatePlayerAppearance(sourceUnitController, 0, unitProfileName, appearanceString, swappableMeshSaveData);
            } else {
                //Debug.Log("PlayerManager.RequestUpdatePlayerAppearance() - server mode, sending request to server");
                networkManagerClient.RequestUpdatePlayerAppearance(characterCreatorComponent.Interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
            }
        }
    }

}