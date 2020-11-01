using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class AggroRange : MonoBehaviour {

        [SerializeField]
        private SphereCollider aggroCollider = null;

        private BaseCharacter baseCharacter = null;

        [SerializeField]
        private float aggroRadius = 20f;

        [SerializeField]
        private bool autoEnableAgro = true;

        public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        private void Awake() {
            SetLayer();
            DisableAggro();
        }

        private void Start() {
            // do this in start because our awake can run before the awake that sets this in the parent
            Interactable interactable = GetComponentInParent<Interactable>();
            if (interactable != null) {
                baseCharacter = interactable.UnitController.BaseCharacter;
            }
            StartEnableAggro();
            //SetAgroRange(aggroRadius);
        }

        public void SetLayer() {
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        public void StartEnableAggro() {
            //Debug.Log("AggroRange.StartEnableAggro()");
            if (autoEnableAgro) {
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

        public void SetAgroRange(float newRange) {
            //Debug.Log("AggroRange.SetAgroRange(" + newRange + ")");
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
            if (baseCharacter == null) {
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
            BaseCharacter otherBaseCharacter = _characterUnit.BaseCharacter;
            if (otherBaseCharacter != null && otherBaseCharacter.CharacterCombat != null && otherBaseCharacter.CharacterStats.IsAlive == true && otherBaseCharacter.Faction != null && baseCharacter != null && baseCharacter.Faction != null) {
                if (Faction.RelationWith(otherBaseCharacter, MyBaseCharacter) <= -1) {
                    baseCharacter.CharacterCombat.MyAggroTable.AddToAggroTable(_characterUnit, -1);
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