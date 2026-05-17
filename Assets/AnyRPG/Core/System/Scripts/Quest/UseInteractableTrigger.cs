using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UseInteractableTrigger : ConfiguredClass, IEventTrigger {

        public event System.Action OnEventTriggered = delegate { };

        [SerializeField]
        protected string interactableName = null;

        protected string ownerName = string.Empty;

        // if just interacting is not enough, but actually finishing using an interactable is required.
        public bool requireCompletion = false;

        // game manager references
        private SystemEventManager systemEventManager = null;


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void HandleCompleteInteractWithOption(UnitController sourceUnitController, InteractableOptionComponent interactableOption) {
            //Debug.Log($"{interactableName}.UseInteractableTrigger.CheckInteractionComplete({sourceUnitController.gameObject.name}, {interactableOption.Interactable.DisplayName})");

            CheckInteractableName(sourceUnitController, interactableOption.DisplayName, true);
            CheckInteractableName(sourceUnitController, interactableOption.Interactable.DisplayName, true);
        }

        public void HandleStartInteractWithOption(UnitController sourceUnitController, InteractableOptionComponent interactableOption, int optionIndex, int choiceIndex) {
            //Debug.Log($"{interactableName}.UseInteractableTrigger.CheckInteractionWithOptionStart({sourceUnitController.gameObject.name}, {optionIndex}, {choiceIndex})");

            CheckInteractableName(sourceUnitController, interactableOption.GetOptionChoiceName(sourceUnitController, choiceIndex), false);
            CheckInteractableName(sourceUnitController, interactableOption.DisplayName, false);
            CheckInteractableName(sourceUnitController, interactableOption.Interactable.DisplayName, false);
        }

        public void CheckInteractableName(UnitController sourceUnitController, string interactableName, bool interactionComplete) {
            //Debug.Log($"{interactableName}.UseInteractableTrigger.CheckInteractableName({sourceUnitController.gameObject.name}, {interactableName}, {interactionComplete})");

            if (SystemDataUtility.MatchResource(interactableName, this.interactableName)) {
                if (!interactionComplete && requireCompletion == true) {
                    return;
                }
                if (requireCompletion == false && interactionComplete) {
                    return;
                }

                OnEventTriggered();
            }
        }

        public void SetupTrigger() {
            //Debug.Log($"{ownerName}.UseInteractableTrigger.SetupTrigger()");

            systemEventManager.OnCompleteInteractWithOption += HandleCompleteInteractWithOption;
            systemEventManager.OnStartInteractWithOption += HandleStartInteractWithOption;
        }

        public void CleanupTrigger() {
            //Debug.Log($"{interactableName}.UseInteractableTrigger.CleanupTrigger()");

            systemEventManager.OnCompleteInteractWithOption -= HandleCompleteInteractWithOption;
            systemEventManager.OnStartInteractWithOption -= HandleStartInteractWithOption;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            this.ownerName = ownerName;
            Configure(systemGameManager);
            SetupTrigger();
        }

        public void CleanupScriptableObjects() {
            CleanupTrigger();
        }
    }
}