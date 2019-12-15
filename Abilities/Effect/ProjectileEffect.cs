using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ProjectileEffect", menuName = "AnyRPG/Abilities/Effects/ProjectileEffect")]
    public class ProjectileEffect : DirectEffect {

        public float projectileSpeed = 0;

        public override Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".ProjectileAttackEffect.Cast(" + source.name + ", " + target.name + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            if (returnObjects != null) {
                foreach (GameObject go in returnObjects.Values) {
                    go.transform.parent = PlayerManager.MyInstance.MyEffectPrefabParent.transform;
                    ProjectileScript projectileScript = go.GetComponent<ProjectileScript>();
                    if (projectileScript != null) {
                        abilityEffectInput = ApplyInputMultiplier(abilityEffectInput);
                        projectileScript.Initialize(projectileSpeed, source, target, new Vector3(0, 1, 0), abilityEffectInput);
                        projectileScript.OnCollission += HandleCollission;
                    }
                }
            }
            return returnObjects;
        }

        public void HandleCollission(BaseCharacter source, GameObject target, GameObject _abilityEffectObject, AbilityEffectOutput abilityEffectInput) {
            PerformAbilityHit(source, target, abilityEffectInput);
            Destroy(_abilityEffectObject);
        }

    }
}