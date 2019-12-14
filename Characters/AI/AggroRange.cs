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

        public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        private void Awake() {
            aggroCollider = GetComponent<SphereCollider>();
            DisableAggro();
        }

        private void Start() {
            // do this in start because our awake can run before the awake that sets this in the parent
            baseCharacter = GetComponentInParent<CharacterUnit>().MyCharacter;
            if (baseCharacter == null) {
                Debug.Log("AggroRange.Start(): baseCharacter is null!");
            }
            EnableAggro();
        }

        /// <summary>
        /// Enable the collider attached to this script
        /// </summary>
        public void EnableAggro() {
            aggroCollider.enabled = true;
            aggroCollider.radius = aggroRadius;
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

    }

}