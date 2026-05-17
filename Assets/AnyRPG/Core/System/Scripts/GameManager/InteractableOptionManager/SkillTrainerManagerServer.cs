using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerManagerServer : InteractableOptionManager {

        public void LearnSkill(UnitController sourceUnitController, Interactable interactable, int componentIndex, int skillId) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is SkillTrainerComponent) {
                (currentInteractables[componentIndex] as SkillTrainerComponent).LearnSkill(sourceUnitController, skillId);
            }
        }

    }

}