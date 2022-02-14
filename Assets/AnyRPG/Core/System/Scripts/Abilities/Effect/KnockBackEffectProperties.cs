using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class KnockBackEffectProperties : InstantEffectProperties {

        [Header("Knockback Type")]

        [Tooltip("If knockback, calculate direction from source to target.  If explosion, calculate from point.")]
        [SerializeField]
        private KnockbackType knockbackType = KnockbackType.Knockback;

        [Header("Knockback Effect")]

        [SerializeField]
        private float knockBackVelocity = 20f;

        [SerializeField]
        private float knockBackAngle = 45f;

        [Header("Explosion")]

        [Tooltip("The radius of the explosion.  All rigidbodies in this radius will have the force applied.")]
        [SerializeField]
        private float explosionRadius = 5f;

        [Tooltip("The force of the explosion.  All rigidbodies in this radius will have the force applied.")]
        [SerializeField]
        private float explosionForce = 10f;

        [Tooltip("Modify the explosion to throw objects updward instead of directly sideways.")]
        [SerializeField]
        private float upwardModifier = 5f;

        [Tooltip("The layers to hit when performing the explosion.")]
        [SerializeField]
        private LayerMask explosionMask = 0;

        // game manager references
        protected PlayerManager playerManager = null;

        public KnockbackType KnockbackType { get => knockbackType; set => knockbackType = value; }
        public float KnockBackVelocity { get => knockBackVelocity; set => knockBackVelocity = value; }
        public float KnockBackAngle { get => knockBackAngle; set => knockBackAngle = value; }
        public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
        public float ExplosionForce { get => explosionForce; set => explosionForce = value; }
        public float UpwardModifier { get => upwardModifier; set => upwardModifier = value; }
        public LayerMask ExplosionMask { get => explosionMask; set => explosionMask = value; }

        /*
        public void GetKnockBackEffectProperties(KnockBackEffect effect) {

            knockbackType = effect.KnockbackType;
            knockBackVelocity = effect.KnockBackVelocity;
            knockBackAngle = effect.KnockBackAngle;
            explosionRadius = effect.ExplosionRadius;
            explosionForce = effect.ExplosionForce;
            upwardModifier = effect.UpwardModifier;
            ExplosionMask = effect.ExplosionMask;

            GetInstantEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".KnockBackEffect.Cast()");
            if (target == null) {
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);

            Vector3 sourcePosition = source.AbilityManager.UnitGameObject.transform.position;
            Vector3 targetPosition = target.transform.position;

            CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter != null && targetCharacterUnit.BaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log("KnockBackEffect.Cast(): stop casting");
                targetCharacterUnit.BaseCharacter.CharacterAbilityManager.StopCasting();
            }

            if (knockbackType == KnockbackType.Knockback) {
                if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter.UnitController.UnitMotor != null) {
                    //Debug.Log("KnockBackEffect.Cast(): casting on character");
                    targetCharacterUnit.BaseCharacter.UnitController.UnitMotor.Move(GetKnockBackVelocity(sourcePosition, targetPosition), true);
                } else {
                    Rigidbody rigidbody = target.GetComponent<Rigidbody>();
                    if (rigidbody != null) {
                        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                        rigidbody.AddForce(GetKnockBackVelocity(sourcePosition, targetPosition), ForceMode.VelocityChange);
                    }
                }
            } else {
                Collider[] colliders = new Collider[0];
                //int playerMask = 1 << LayerMask.NameToLayer("Default");
                //int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
                //int validMask = (playerMask | characterMask);
                //int validMask = playerMask;
                //colliders = Physics.OverlapSphere(targetPosition, explosionRadius, validMask);
                Vector3 explosionCenter = Vector3.zero;
                if (abilityEffectContext.groundTargetLocation != Vector3.zero) {
                    explosionCenter = abilityEffectContext.groundTargetLocation;
                } else {
                    explosionCenter = targetPosition;
                }
                colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, explosionMask);
                foreach (Collider collider in colliders) {
                    //Debug.Log(DisplayName + ".KnockBackEffect.Cast() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);
                    Rigidbody rigidbody = collider.gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null) {
                        //Debug.Log(DisplayName + ".KnockBackEffect.Cast() rigidbody was not null on : " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                        //rigidbody.AddForce(GetKnockBackVelocity(targetPosition, collider.gameObject.transform.position), ForceMode.VelocityChange);

                        // we have to handle player knockback specially, as they need to be in knockback state or the idle update will freeze them in place
                        if (collider.gameObject == playerManager.ActiveUnitController.gameObject) {
                            playerManager.PlayerUnitMovementController.KnockBack();
                        }

                        // if this is a character, we want to freeze their rotation.  for inanimate objects, we want rotation
                        Interactable _interactable = collider.gameObject.GetComponent<Interactable>();
                        if (_interactable != null && CharacterUnit.GetCharacterUnit(_interactable) != null) {
                            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                        }
                        rigidbody.AddExplosionForce(explosionForce, explosionCenter, 0, upwardModifier, ForceMode.VelocityChange);
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

            //Debug.Log(DisplayName + "KnockBackEffect.GetKnockBackVelocity() return: " + finalDirection);

            return finalDirection;

        }
    }

    public enum KnockbackType { Knockback, Explosion };

}
