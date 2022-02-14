using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New KnockBackEffect", menuName = "AnyRPG/Abilities/Effects/KnockBackEffect")]
    [System.Serializable]
    public class KnockBackEffect : InstantEffect {

        /*
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
        */
        [SerializeField]
        private KnockBackEffectProperties knockBackEffectProperties = new KnockBackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => knockBackEffectProperties; }

        


    }

}
