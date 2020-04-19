using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerAnimator : CharacterAnimator {

        public override void CreateEventSubscriptions() {
            // called from base.start
            base.CreateEventSubscriptions();
            //SystemEventManager.MyInstance.OnEquipmentRefresh += PerformEquipmentChange;
        }

        public override void CleanupEventSubscriptions() {
            // called from base.onDisable
            base.CleanupEventSubscriptions();
            /*
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnEquipmentRefresh -= PerformEquipmentChange;
            }
            */
        }

        public override void SetOverrideController(AnimatorOverrideController animatorOverrideController) {
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == false) {
                base.SetOverrideController(animatorOverrideController);
            }
        }

        // for debugging
        /*
        public override void InitializeAnimator() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.InitializeAnimator()");
            base.InitializeAnimator();
        }

        public override void SetCasting(bool varValue) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCasting(" + varValue + ")");
            base.SetCasting(varValue);
        }

        public override void ClearAnimationBlockers() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.ClearAnimationBlockers()");
            base.ClearAnimationBlockers();
        }
        */

    }
}