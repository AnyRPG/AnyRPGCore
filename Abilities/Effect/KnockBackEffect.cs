using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New KnockBackEffect", menuName = "AnyRPG/Abilities/Effects/KnockBackEffect")]
    public class KnockBackEffect : InstantEffect {

        [Header("Knockback Effect")]

        [SerializeField]
        private float knockBackVelocity = 20f;

        [SerializeField]
        private float knockBackAngle = 45f;

        [Header("Explosion")]

        [Tooltip("If true, all rigidbodies in a set radius will be affected by the knockback")]
        [SerializeField]
        private bool isExplosion = false;

        [Tooltip("The radius of the explosion.  All rigidbodies in this radius will have the force applied.")]
        [SerializeField]
        private float explosionRadius = 5f;

        [Tooltip("The force of the explosion.  All rigidbodies in this radius will have the force applied.")]
        [SerializeField]
        private float explosionForce = 200f;

        [Tooltip("The layers to hit when performing the explosion.")]
        [SerializeField]
        private LayerMask explosionMask = 0;


        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            if (target == null) {
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            Vector3 sourcePosition = source.UnitGameObject.transform.position;
            Vector3 targetPosition = target.transform.position;

            AnimatedUnit animatedUnit = target.GetComponent<AnimatedUnit>();
            CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
            if (targetCharacterUnit != null && targetCharacterUnit.MyCharacter != null && targetCharacterUnit.MyCharacter.CharacterAbilityManager) {
                //Debug.Log("KnockBackEffect.Cast(): stop casting");
                targetCharacterUnit.MyCharacter.CharacterAbilityManager.StopCasting();
            }
            if (animatedUnit != null && animatedUnit.MyCharacterMotor != null) {
                //Debug.Log("KnockBackEffect.Cast(): casting on character");
                animatedUnit.MyCharacterMotor.Move(GetKnockBackVelocity(sourcePosition, targetPosition), true);
            } else {
                Rigidbody rigidbody = target.GetComponent<Rigidbody>();
                if (rigidbody != null) {
                    rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    rigidbody.AddForce(GetKnockBackVelocity(sourcePosition, targetPosition), ForceMode.VelocityChange);
                }
            }

            if (isExplosion) {
                Collider[] colliders = new Collider[0];
                int playerMask = 1 << LayerMask.NameToLayer("Default");
                //int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
                //int validMask = (playerMask | characterMask);
                int validMask = playerMask;
                //colliders = Physics.OverlapSphere(targetPosition, explosionRadius, validMask);
                colliders = Physics.OverlapSphere(targetPosition, explosionRadius, explosionMask);
                foreach (Collider collider in colliders) {
                    //Debug.Log(MyName + "KnockBackEffect.Cast() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);
                    Rigidbody rigidbody = collider.gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null) {
                        //Debug.Log(MyName + "KnockBackEffect.Cast() rigidbody was not null on : " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                        //rigidbody.AddForce(GetKnockBackVelocity(targetPosition, collider.gameObject.transform.position), ForceMode.VelocityChange);
                        rigidbody.AddExplosionForce(explosionForce, targetPosition, 0, 0, ForceMode.VelocityChange);
                    }
                }
            }


            return returnObjects;
        }

        public Vector3 GetKnockBackVelocity(Vector3 sourcePosition, Vector3 targetPosition) {

            // get vector from source to target for flight direction
            Vector3 originalDirection = (targetPosition - sourcePosition).normalized;

            // create a generic rotation 45 degrees upward
            Vector3 rotationDirection = Quaternion.Euler(-knockBackAngle, 0, 0) * Vector3.forward;

            // take that generic rotation and rotate it so it is now facing in the direction of flight
            Vector3 finalDirection = Quaternion.LookRotation(originalDirection) * rotationDirection;

            // add velocity to the now correct flight direction
            finalDirection *= knockBackVelocity;

            //Debug.Log(MyName + "KnockBackEffect.GetKnockBackVelocity() return: " + finalDirection);

            return finalDirection;

        }


    }

}
