using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ProjectileEffect", menuName = "AnyRPG/Abilities/Effects/ProjectileEffect")]
    public class ProjectileEffect : DirectEffect {

        public float projectileSpeed = 0;

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".ProjectileEffect.Cast(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);
            if (returnObjects != null) {
                foreach (GameObject go in returnObjects.Values) {
                    //Debug.Log(MyName + ".ProjectileEffect.Cast(): found gameobject: " + go.name);
                    go.transform.parent = PlayerManager.MyInstance.EffectPrefabParent.transform;
                    ProjectileScript projectileScript = go.GetComponent<ProjectileScript>();
                    if (projectileScript != null) {
                        //Debug.Log(MyName + ".ProjectileEffect.Cast(): found gameobject: " + go.name + " and it has projectile script");
                        abilityEffectContext = ApplyInputMultiplier(abilityEffectContext);
                        projectileScript.Initialize(projectileSpeed, source, target, new Vector3(0, 1, 0), abilityEffectContext);
                        projectileScript.OnCollission += HandleCollission;
                    }
                }
            }
            return returnObjects;
        }

        public void HandleCollission(IAbilityCaster source, Interactable target, GameObject _abilityEffectObject, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".ProjectileEffect.HandleCollission()");
            PerformAbilityHit(source, target, abilityEffectInput);
            Destroy(_abilityEffectObject);
        }

    }
}