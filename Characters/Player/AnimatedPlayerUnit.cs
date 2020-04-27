using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class AnimatedPlayerUnit : AnimatedUnit  {

        private PlayerUnitMovementController playerUnitMovementController;

        public PlayerUnitMovementController MyPlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".CharacterUnit.Awake() about to get references to all local components");
            base.Awake();
        }

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".AnimatedPlayerUnit.OrchestratorStart()");
            if (orchestratorStartupComplete) {
                return;
            }
            base.OrchestratorStart();
            if (playerUnitMovementController != null) {
                playerUnitMovementController.OrchestratorStart();
            }
        }

        public override void OrchestratorFinish() {
            //Debug.Log(gameObject.name + ".AnimatedPlayerUnit.OrchestratorFinish()");
            base.OrchestratorFinish();
        }

        public override void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("CharacterUnit.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        protected override void OnEnable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnEnable()");
            base.OnEnable();
        }

        protected override void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnDisable()");
            base.OnDisable();
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".AnimatedPlayerUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            playerUnitMovementController = GetComponent<PlayerUnitMovementController>();
            base.GetComponentReferences();
        }


    }

}