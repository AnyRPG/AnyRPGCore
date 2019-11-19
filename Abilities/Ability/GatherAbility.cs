using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gather Ability",menuName = "AnyRPG/Abilities/Effects/GatherAbility")]
    public class GatherAbility : DirectAbility {

        public override bool Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            bool returnResult = base.Cast(source, target, groundTarget);
            if (returnResult == true) {
                target.GetComponent<GatheringNode>().Gather();
            }
            return returnResult;
        }

        public override bool CanUseOn(GameObject target, BaseCharacter source) {
            if (target != null) {
                //Debug.Log("GatherAbility.CanUseOn(" + target.name + ")");
            } else {
                //Debug.Log("GatherAbility.CanUseOn(null)");
            }
            if (!base.CanUseOn(target, source)) {
                return false;
            }
            // distance from center of character to whereever the raycast hit the object
            float distanceToTarget = Vector3.Distance((PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyMouseOverhit.point, source.MyCharacterUnit.transform.TransformPoint(source.MyCharacterUnit.GetComponent<CapsuleCollider>().center));
            //Debug.Log("PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyMouseOverhit.point: " + PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyMouseOverhit.point);

            if (distanceToTarget > (source.MyCharacterStats.MyHitBox * 2)) {
                //Debug.Log(target.name + " is out of range: " + distanceToTarget);
                return false;
            }

            GatheringNode _gatheringNode = target.GetComponent<GatheringNode>();
            if (_gatheringNode == null) {
                //Debug.Log("You cannot use " + MyName + " on: " + target.name);
                return false;
            }

            if (_gatheringNode.MyAbility == this) {
                return true;
            } else {
                //Debug.Log(target.name + " requires ability: " + _gatheringNode.MyAbility);
                return false;
            }
        }

    }

}