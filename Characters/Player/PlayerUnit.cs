using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerUnit : CharacterUnit {

        protected override void Start() {
            base.Start();

            // this code is a quick way to set speed on third party controllers when the player spawns
            if (BaseCharacter != null && BaseCharacter.CharacterStats != null) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = BaseCharacter.CharacterStats.RunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);

                eventParam.simpleParams.FloatParam = BaseCharacter.CharacterStats.SprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);

            }
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl) {
                KeyBindManager.MyInstance.SendKeyBindEvents();
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

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".PlayerUnit.OrchestratorStart()");
            base.OrchestratorStart();
        }

        public override void OrchestratorFinish() {
            //Debug.Log(gameObject.name + ".PlayerUnit.OrchestratorFinish()");
            base.OrchestratorFinish();
        }
    }


}