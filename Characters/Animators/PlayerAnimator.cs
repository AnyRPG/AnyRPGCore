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


        public override void SetCorrectOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCorrectOverrideController()");
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                base.SetOverrideController(thirdPartyOverrideController, runUpdate);
                return;
            }
            base.SetCorrectOverrideController(runUpdate);
        }


        public override void InitializeAnimator() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.InitializeAnimator()");
            if (initialized) {
                return;
            }
            base.InitializeAnimator();
            if (animator == null) {
                //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): Could not find animator in children");
                return;
            } else {
                if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                    if (thirdPartyAnimatorController == null) {
                        thirdPartyAnimatorController = animator.runtimeAnimatorController;
                    }
                    if (thirdPartyAnimatorController != null) {
                        thirdPartyOverrideController = new AnimatorOverrideController(thirdPartyAnimatorController);
                        //Debug.Log(gameObject.name + ": PlayerAnimator.InitializeAnimator(): got third party animator: " + thirdPartyAnimatorController.name);
                    } else {
                        //Debug.Log(gameObject.name + ": PlayerAnimator.InitializeAnimator(): third party animator was null but use third party movement control was true");
                    }

                }
            }
        }

        /*
        public override void ClearAnimationBlockers() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.ClearAnimationBlockers()");
            base.ClearAnimationBlockers();
        }
        */

        public override void SetCasting(bool varValue, bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCasting(" + varValue + ")");
            if (animator == null) {
                return;
            }

            EventParamProperties eventParam = new EventParamProperties();
            if (varValue == true) {
                if (swapAnimator == true) {
                    SetDefaultOverrideController();
                }
                SystemEventManager.TriggerEvent("OnStartCasting", eventParam);
            }

            base.SetCasting(varValue);

            if (varValue == false) {
                if (swapAnimator) {
                    SetCorrectOverrideController();
                    SystemEventManager.TriggerEvent("OnEndCasting", eventParam);
                }
            }

        }

        public override void SetAttacking(bool varValue, bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".SetAttacking(" + varValue + ")");
            if (animator == null) {
                return;
            }
            EventParamProperties eventParam = new EventParamProperties();
            if (varValue == true) {
                if (swapAnimator) {
                    SetDefaultOverrideController();
                }
                SystemEventManager.TriggerEvent("OnStartAttacking", eventParam);
            }

            base.SetAttacking(varValue);

            if (varValue == false) {
                if (swapAnimator) {
                    SetCorrectOverrideController();
                    SystemEventManager.TriggerEvent("OnEndAttacking", eventParam);
                }
            }

        }

        public override void SetRiding(bool varValue) {
            //Debug.Log(gameObject.name + ".SetRiding(" + varValue + ")");
            if (animator == null) {
                return;
            }
            EventParamProperties eventParam = new EventParamProperties();
            if (varValue == true) {
                SetDefaultOverrideController();
                SystemEventManager.TriggerEvent("OnStartRiding", eventParam);
            }

            base.SetRiding(varValue);

            if (varValue == false) {
                SetCorrectOverrideController();
                SystemEventManager.TriggerEvent("OnEndRiding", eventParam);
            }
        }

        public override void HandleLevitated() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
            SetDefaultOverrideController();

            base.HandleLevitated();
        }

        public override void HandleUnLevitated(bool swapAnimator = true) {
            base.HandleUnLevitated(swapAnimator);
            if (swapAnimator) {
                SetCorrectOverrideController();
            }
        }

        public override void HandleStunned() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleStunned()");
            SetDefaultOverrideController();
            base.HandleStunned();
        }

        public override void HandleUnStunned(bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleUnStunned()");
            base.HandleUnStunned(swapAnimator);
            if (swapAnimator) {
                SetCorrectOverrideController();
            }
        }

        public override void HandleRevive() {
            SetDefaultOverrideController();
            base.HandleRevive();
        }

        public override void HandleDeath(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.HandleDeath()");
            SetDefaultOverrideController();
            SystemEventManager.TriggerEvent("OnDeath", new EventParamProperties());
            base.HandleDeath(characterStats);
        }

    }
}