using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New KnockBackEffect", menuName = "AnyRPG/Abilities/Effects/KnockBackEffect")]
    public class KnockBackEffect : InstantEffect {

        // The prefab to summon
        [SerializeField]
        private float knockBackVelocity = 20f;

        [SerializeField]
        private float knockBackAngle = 45f;

        /*
        [SerializeField]
        private Vector3 spawnLocation = Vector3.zero;
        */

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            if (target == null) {
                return null;
            }

            AnimatedUnit animatedUnit = target.GetComponent<AnimatedUnit>();
            if (animatedUnit == null || animatedUnit.MyCharacterMotor == null) {
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            Vector3 sourcePosition = source.UnitGameObject.transform.position;
            Vector3 targetPosition = target.transform.position;
            
            // get vector from source to target for flight direction
            Vector3 originalDirection = (targetPosition - sourcePosition).normalized;

            // create a generic rotation 45 degrees upward
            Vector3 rotationDirection = Quaternion.Euler(-knockBackAngle, 0, 0) * Vector3.forward;

            // take that generic rotation and rotate it so it is now facing in the direction of flight
            Vector3 finalDirection = Quaternion.LookRotation(originalDirection) * rotationDirection;

            // add velocity to the now correct flight direction
            finalDirection *= knockBackVelocity;

            //Debug.Log("KnockBackEffect.Cast(): originalDirection: " + originalDirection + "; rotationDirection: " + rotationDirection + "; finalDirection: " + finalDirection + "; knockbackAngle: " + knockBackAngle);
            animatedUnit.MyCharacterMotor.Move(finalDirection, true);

            return returnObjects;
        }


    }

}
