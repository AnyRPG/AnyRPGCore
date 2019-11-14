using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ChanneledEffect",menuName = "AnyRPG/Abilities/Effects/ChanneledEffect")]
    public class ChanneledEffect : DirectEffect {

        // the amount of time to delay damage after spawning the prefab
        public float effectDelay = 0f;

        public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("ChanelledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ")");
            if (target == null) {
                // maybe target died or despawned in the middle of cast?
                return;
            }
            if (abilityEffectInput == null) {
                abilityEffectInput = new AbilityEffectOutput();
            }
            base.Cast(source, target, originalTarget, abilityEffectInput);
            if (abilityEffectObject != null) {
                abilityEffectObject.transform.parent = PlayerManager.MyInstance.MyEffectPrefabParent.transform;
                IChanneledObject channeledObjectScript = abilityEffectObject.GetComponent<IChanneledObject>();
                if (channeledObjectScript != null) {
                    GameObject prefabParent = source.MyCharacterUnit.gameObject;
                    Transform usedPrefabSourceBone = null;
                    if (prefabSourceBone != null && prefabSourceBone != string.Empty) {
                        usedPrefabSourceBone = prefabParent.transform.FindChildByRecursive(prefabSourceBone);
                    }
                    if (usedPrefabSourceBone != null) {
                        prefabParent = usedPrefabSourceBone.gameObject;
                    }
                    channeledObjectScript.MyStartObject = prefabParent;
                    //channeledObjectScript.MyStartPosition = source.MyCharacterUnit.GetComponent<Collider>().bounds.center - source.MyCharacterUnit.transform.position;
                    channeledObjectScript.MyStartPosition = prefabOffset;
                    //channeledObjectScript.MyStartPosition = prefabParent.transform.TransformPoint(prefabOffset);
                    channeledObjectScript.MyEndObject = target.gameObject;
                    channeledObjectScript.MyEndPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
                } else {
                    Debug.Log(MyName + ".ChanelledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + "): CHECK INSPECTOR, CHANNELEDOBJECT NOT FOUND");
                }
                // delayed damage
                //source.StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput));
                source.MyCharacterAbilityManager.BeginPerformAbilityHitDelay(source, target, abilityEffectInput, this);
            }
        }

        public override bool CanUseOn(GameObject target, BaseCharacter sourceCharacter) {
            if (target == null) {
                // channeled effect always requires target because the prefab object must have a start and end point
                return false;
            }
            return base.CanUseOn(target, sourceCharacter);
        }

    }
}
