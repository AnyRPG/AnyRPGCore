using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class AggroRange : MonoBehaviour {

        private SphereCollider aggroCollider;

        private BaseCharacter baseCharacter;

        [SerializeField]
        private float aggroRadius = 20f;

        [SerializeField]
        private bool autoEnableAgro = true;

        public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        private void Awake() {
            SetLayer();
            aggroCollider = GetComponent<SphereCollider>();
            DisableAggro();
        }

        private void Start() {
            // do this in start because our awake can run before the awake that sets this in the parent
            baseCharacter = GetComponentInParent<CharacterUnit>().MyCharacter;
            if (baseCharacter == null) {
                //Debug.Log("AggroRange.Start(): baseCharacter is null!");
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
            CharacterUnit _characterUnit = collider.gameObject.GetComponent<CharacterUnit>();
            if (_characterUnit == null) {
                // this was not a character that entered, and therefore we cannot agro it
                return;
            }
            BaseCharacter otherBaseCharacter = _characterUnit.MyCharacter;
            if (otherBaseCharacter != null && otherBaseCharacter.MyCharacterCombat != null && otherBaseCharacter.MyCharacterStats.IsAlive == true && otherBaseCharacter.MyFaction != null && baseCharacter != null && baseCharacter.MyFaction != null) {
                if (Faction.RelationWith(otherBaseCharacter, MyBaseCharacter) <= -1) {
                    baseCharacter.MyCharacterCombat.MyAggroTable.AddToAggroTable(_characterUnit, -1);
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