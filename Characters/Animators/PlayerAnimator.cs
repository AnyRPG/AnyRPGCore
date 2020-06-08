using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerAnimator : CharacterAnimator {

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.OrchestratorStart()");
            base.OrchestratorStart();
        }

        public override void CreateEventSubscriptions() {
            // called from base.start
            //Debug.Log(gameObject.name + ".PlayerAnimator.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized == true) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.StartListening("OnReviveComplete", HandleReviveComplete);
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.CleanupEventSubscriptions()");
            // called from base.onDisable
            base.CleanupEventSubscriptions();
            SystemEventManager.StopListening("OnReviveComplete", HandleReviveComplete);
        }

        public void HandleReviveComplete(string eventName, EventParamProperties eventParamProperties) {
            SetCorrectOverrideController();
        }


    public override void SetCorrectOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCorrectOverrideController()");
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                base.SetOverrideController(thirdPartyOverrideController, runUpdate);
                return;
            }
            base.SetCorrectOverrideController(runUpdate);
        }

        public override void SetOverrideController(AnimatorOverrideController animatorOverrideController, bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCorrectOverrideController(" + runUpdate + ")");
            base.SetOverrideController(animatorOverrideController, runUpdate);
        }


        public override void InitializeAnimator() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.InitializeAnimator()");
            if (initialized) {
                //Debug.Log(gameObject.name + ".PlayerAnimator.InitializeAnimator(): already initialized.  returning");
                return;
            }
            if (animator == null) {
                //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): Could not find animator in children");
                return;
            }
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                if (thirdPartyAnimatorController == null) {
                    thirdPartyAnimatorController = animator.runtimeAnimatorController;
                }
                if (thirdPartyAnimatorController != null) {
                    thirdPartyOverrideController = new AnimatorOverrideController(thirdPartyAnimatorController);
                    //Debug.Log(gameObject.name + ": PlayerAnimator.InitializeAnimator(): got third party override: " + thirdPartyAnimatorController.name);
                } else {
                    //Debug.Log(gameObject.name + ": PlayerAnimator.InitializeAnimator(): third party animator was null but use third party movement control was true");
                }
            }
            base.InitializeAnimator();
        }

        /*
        public override void ClearAnimationBlockers() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.ClearAnimationBlockers()");
            base.ClearAnimationBlockers();
        }
        */

        public override void SetCasting(bool varValue, bool swapAnimator = true, float castingSpeed = 1f) {
            //Debug.Log(gameObject.name + ".PlayerAnimator.SetCasting(" + varValue + ", " + swapAnimator + ", " + castingSpeed + ")");
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

            base.SetCasting(varValue, swapAnimator, castingSpeed);

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
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartLevitated", eventParam);
            base.HandleLevitated();
        }

        public override void HandleUnLevitated(bool swapAnimator = true) {
            base.HandleUnLevitated(swapAnimator);
            if (swapAnimator) {
                SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndLevitated", eventParam);
            }
        }

        public override void HandleStunned() {
            //Debug.Log(gameObject.name + ".PlayerAnimator.HandleStunned()");
            SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartStunned", eventParam);
            base.HandleStunned();
        }

        public override void HandleUnStunned(bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleUnStunned()");
            base.HandleUnStunned(swapAnimator);
            if (swapAnimator) {
                SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndStunned", eventParam);
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