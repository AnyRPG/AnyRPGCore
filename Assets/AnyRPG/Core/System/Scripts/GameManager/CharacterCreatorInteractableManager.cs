using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorInteractableManager : InteractableOptionManager {

        CharacterCreatorComponent characterCreator = null;

        public CharacterCreatorComponent CharacterCreator { get => characterCreator; }

        public override void EndInteraction() {
            base.EndInteraction();

            characterCreator = null;
        }

        public void SetCharacterCreator(CharacterCreatorComponent characterCreator) {
            //Debug.Log("CharacterCreatorInteractableManager.SetSkillTrainer(" + characterClass + ")");
            this.characterCreator = characterCreator;

            BeginInteraction(characterCreator);
        }
    }

}