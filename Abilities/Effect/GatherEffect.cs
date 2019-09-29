using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New GatherEffect", menuName = "Abilities/Effects/GatherEffect")]
public class GatherEffect : DirectEffect {

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        Debug.Log("GatherAbility.Cast(" + source.name + ", " + target.name + ")");
        base.Cast(source, target, originalTarget, abilityEffectInput);

        target.GetComponent<GatheringNode>().Gather();
    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        if (Vector3.Distance(target.transform.position, source.MyCharacterUnit.transform.position) > (source.MyCharacterStats.MyHitBox * 2)) {
            Debug.Log(target.name + " is out of range!");
            return false;
        }
        return true;
    }

}
