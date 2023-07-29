using AnyRPG;
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

        [SerializeField]
        private bool autoEnableAgro = true;

        public UnitController UnitController { get => unitController; set => unitController = value; }

        private void OnEnable() {
            SetLayer();
            DisableAggro();
        }

        public void SetLayer() {
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        /// <summary>
        /// detect if agro should be enabled based on monobehavior setting or unit profile
        /// </summary>
        public void StartEnableAggro() {
            //Debug.Log("AggroRange.StartEnableAggro()");
            if (autoEnableAgro
                || (unitController.UnitProfile != null && unitController.UnitProfile.IsAggressive == true)) {
                EnableAggro();
            }
        }

        /// <summary>
        /// Enable the collider attached to this script
        /// </summary>
        public void EnableAggro() {
            //Debug.Log("AggroRange.EnableAggro()");
            aggroCollider.enabled = true;
            aggroCollider.radius = aggroRadius;
        }

        public void SetAgroRange(float newRange, UnitController unitController) {
            //Debug.Log("AggroRange.SetAgroRange(" + newRange + ")");
            this.unitController = unitController;
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
            aggroCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            if (unitController == null) {
                return;
            }
            // if a player enters our sphere, target him (which has the effect of agro because the idle state will follow any target the enemycontroller has)
            Interactable targetInteractable = collider.gameObject.GetComponent<Interactable>();
            if (targetInteractable == null) {
                // whatever entered the sphere was not interactable
                return;
            }
            CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(targetInteractable);
            if (_characterUnit == null) {
                // this was not a character that entered, and therefore we cannot agro it
                return;
            }

            // cannot agro characters that are stealthed
            if (_characterUnit.UnitController.CharacterStats.IsStealthed == true) {
                return;
            }

            UnitController otherUnitController = _characterUnit.UnitController;
            // remove requirement for other character to have faction because a neutral character would not get attacked by hostile factions
            //if (otherBaseCharacter != null && otherBaseCharacter.CharacterCombat != null && otherunitController.CharacterStats.IsAlive == true && otherBaseCharacter.Faction != null && baseCharacter != null && baseCharacter.Faction != null) {
            if (otherUnitController != null
                && otherUnitController.CharacterStats.IsAlive == true
                && unitController.BaseCharacter.Faction != null) {
                if (Faction.RelationWith(otherUnitController, UnitController) <= -1) {
                    //baseCharacter.CharacterCombat.MyAggroTable.AddToAggroTable(_characterUnit, -1);
                    //baseCharacter.CharacterCombat.EnterCombat(targetInteractable);
                    unitController.ProximityAggro(_characterUnit);

                }
            }
        }

        public bool AggroEnabled() {
            if (aggroCollider != null) {
                return aggroCollider.enabled;
            }
            return false;
        }

    }

}