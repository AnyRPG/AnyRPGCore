using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterModel : SpawnedNetworkObject {

        private SystemGameManager systemGameManager = null;
        private NetworkAnimator networkAnimator = null;
        private UnitController unitController = null;
        private Animator animator = null;

        private void FindGameManager() {
            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
            networkAnimator = GetComponent<NetworkAnimator>();
            unitController = GetComponentInParent<UnitController>();
            animator = GetComponent<Animator>();
            unitController.UnitAnimator.OnInitializeAnimator += HandleInitializeAnimator;

            if (base.IsOwner) {
                unitController.UnitAnimator.OnSetTrigger += HandleSetTrigger;
            }
        }

        private void HandleSetTrigger(string triggerName) {
            networkAnimator.SetTrigger(triggerName);
        }

        private void HandleInitializeAnimator() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleInitializeAnimator()");
            networkAnimator.SetAnimator(animator);
        }

        private void CompleteModelRequest(bool isOwner) {
            /*
            CharacterRequestData characterRequestData;
            characterRequestData = new CharacterRequestData(null, 
                GameMode.Network,
                null, // does not matter since it's unused to the CompleteModelRequest() process
                UnitControllerMode.Preview, // does not matter since it's unused to the CompleteModelRequest() process
                new CharacterConfigurationRequest());
            characterRequestData.spawnRequestId = clientSpawnRequestId;
            systemGameManager.CharacterManager.CompleteModelRequest(characterRequestData, unitController, isOwner);
            */
            systemGameManager.CharacterManager.CompleteModelRequest(clientSpawnRequestId, unitController, isOwner);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartClient()");

            FindGameManager();
            if (systemGameManager == null) {
                return;
            }
            CompleteModelRequest(base.IsOwner);
        }


        public override void OnStartServer() {
            base.OnStartClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartServer()");

            FindGameManager();
            if (systemGameManager == null) {
                return;
            }
            CompleteModelRequest(false);
            //systemGameManager.CharacterManager.CompleteModelRequest(serverRequestId, false);
        }

    }
}

