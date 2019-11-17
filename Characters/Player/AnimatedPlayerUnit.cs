using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class AnimatedPlayerUnit: AnimatedUnit  {

        private PlayerUnitMovementController playerUnitMovementController;

        public PlayerUnitMovementController MyPlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".CharacterUnit.Awake() about to get references to all local components");
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".AnimatedPlayerUnit.Start()");
            base.Start();
        }

        public override void OrchestrateStartup() {
            //Debug.Log(gameObject.name + ".AnimatedPlayerUnit.OrchestratorStartup()");
            base.OrchestrateStartup();
            playerUnitMovementController.OrchestrateStartup();
        }

        public override void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized || !startHasRun) {
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
            Debug.Log(gameObject.name + ".AnimatedPlayerUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            playerUnitMovementController = GetComponent<PlayerUnitMovementController>();
            base.GetComponentReferences();
        }


    }

}