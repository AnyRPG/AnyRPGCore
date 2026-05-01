using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class AggroRange : MonoBehaviour {

        [SerializeField]
        private SphereCollider aggroCollider = null;

        private UnitController unitController = null;

        [SerializeField]
        private float aggroRadius = 20f;

        public UnitController UnitController { get => unitController; set => unitController = value; }

        private void OnEnable() {
            SetLayer();
            DisableAggro();
        }

        public void SetLayer() {
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        /// <summary>
        /// Enable the collider attached to this script
        /// </summary>
        public void EnableAggro() {
            //Debug.Log($"{unitController.gameObject.name}.AggroRange.EnableAggro()");

            aggroCollider.enabled = true;
            aggroCollider.radius = aggroRadius;
        }

        public void HandleSetAggroRange(float newRange) {
            //Debug.Log($"{unitController.gameObject.name}.AggroRange.SetAgroRange({newRange})");

            aggroRadius = newRange;
            if (aggroCollider != null) {
                aggroCollider.radius = aggroRadius;
            } else {
                //Debug.Log("AggroRange.SetAgroRange(" + newRange + "): NO REFERENCE TO AGGRO COLLIDER");
            }
        }

        /// <summary>
        /// Disable the collider attached to this script
        /// </summary>
        public void DisableAggro() {
            //Debug.Log($"{(unitController == null ? "null" : unitController.gameObject.name)}.AggroRange.DisableAggro()");

            aggroCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log($"{(unitController == null ? "null" : unitController.gameObject.name)}.AggroRange.OnTriggerEnter({collider.gameObject.name})");

            if (unitController == null) {
                return;
            }
            // if a player enters our sphere, target him (which has the effect of agro because the idle state will follow any target the enemycontroller has)
            UnitController targetUnitController = collider.gameObject.GetComponent<UnitController>();
            if (targetUnitController == null || targetUnitController.IsInitialized == false) {
                // this was not a character that entered, and therefore we cannot agro it
                // or it was a character but it has not finished initializing yet, so we cannot agro it
                return;
            }

            if (targetUnitController.UnitControllerMode == UnitControllerMode.Mount) {
                // mounts should not be agroed, but we want to check if the mount has a rider and agro the rider if they do
                if (targetUnitController.RiderUnitController != null) {
                    targetUnitController = targetUnitController.RiderUnitController;
                } else {
                    return;
                }
            }

            // cannot agro characters that are stealthed
            if (targetUnitController.CharacterStats.IsStealthed == true) {
                return;
            }

            // remove requirement for other character to have faction because a neutral character would not get attacked by hostile factions
            //if (otherBaseCharacter != null && otherBaseCharacter.CharacterCombat != null && otherunitController.CharacterStats.IsAlive == true && otherBaseCharacter.Faction != null && baseCharacter != null && baseCharacter.Faction != null) {
            if (targetUnitController != null
                && targetUnitController.CharacterStats.IsAlive == true
                && unitController.BaseCharacter.Faction != null) {
                if (Faction.RelationWith(targetUnitController, UnitController) <= -1f) {
                    //baseCharacter.CharacterCombat.MyAggroTable.AddToAggroTable(_characterUnit, -1);
                    //baseCharacter.CharacterCombat.EnterCombat(targetInteractable);
                    unitController.ProximityAggro(targetUnitController);

                }
            }
        }

        public void SetUnitController(UnitController unitController) {
            //Debug.Log($"AggroRange.SetUnitController({unitController.gameObject.name})");

            this.unitController = unitController;
            unitController.UnitEventController.OnSetAggroRange += HandleSetAggroRange;
            unitController.UnitEventController.OnEnableAggro += HandleEnableAggro;
            unitController.UnitEventController.OnDisableAggro += HandleDisableAggro;
            unitController.OnInteractableResetSettings += HandleInteractableResetSettings;
        }

        private void HandleInteractableResetSettings() {
            unitController.UnitEventController.OnSetAggroRange -= HandleSetAggroRange;
            unitController.UnitEventController.OnEnableAggro -= HandleEnableAggro;
            unitController.UnitEventController.OnDisableAggro -= HandleDisableAggro;
            unitController.OnInteractableResetSettings -= HandleInteractableResetSettings;
        }

        private void HandleDisableAggro() {
            //Debug.Log($"{unitController.gameObject.name}.AggroRange.HandleDisableAggro()");

            DisableAggro();
        }

        private void HandleEnableAggro() {
            //Debug.Log($"{unitController.gameObject.name}.AggroRange.HandleEnableAggro()");

            EnableAggro();
        }

    }

}