using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerUnit : CharacterUnit {

        protected override void Start() {
            base.Start();
            if (MyBaseCharacter != null && MyBaseCharacter.MyCharacterStats != null) {
                EventParam eventParam = new EventParam();
                eventParam.FloatParam = MyBaseCharacter.MyCharacterStats.MyMovementSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);
            }
        }

        /*
        PlayerUnitMovementController playerUnitMovementController;

        public PlayerUnitMovementController MyPlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            base.GetComponentReferences();
            playerUnitMovementController = GetComponent<PlayerUnitMovementController>();
        }
        */
        protected override void SetDefaultLayer() {
            // intentionally overwrite base class to avoid settting layer incorrectly on player
        }
    }


}