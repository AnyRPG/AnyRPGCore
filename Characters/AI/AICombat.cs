using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AICombat : CharacterCombat {

        protected override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".AICombat.CreateEventSubscriptions()");
            base.CreateEventSubscriptions();
            baseCharacter.CharacterStats.OnDie += HandleDie;
        }

        protected override void Update() {
            //Debug.Log(gameObject.name + ".AICombat.Update()");
            base.Update();
            if (!baseCharacter.CharacterStats.IsAlive) {
                return;
            }
        }
        /*
            public override void ClearAggro(GameObject target) {
                base.ClearAggro(target);
                aiController.ClearTarget();
            }
            */

        public override bool TakeDamage(int damage, Vector3 sourcePosition, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect, bool reflectDamage = false) {
            //Debug.Log("AICombat.TakeDamage(" + damage + ", " + sourcePosition + ", " + source + ")");
            if (!((baseCharacter.CharacterController as AIController).MyCurrentState is EvadeState) && !((baseCharacter.CharacterController as AIController).MyCurrentState is DeathState)) {
                // order is important here.  we want to set target before taking damage because taking damage could kill us, and we don't want to re-trigger and agro on someone after we are dead

                // this should happen automatically inside the update loop of idle state
                //baseCharacter.MyCharacterController.SetTarget(source);
                return base.TakeDamage(damage, sourcePosition, source, combatMagnitude, abilityEffect, reflectDamage);
            }
            return false;
        }
        /*
        public override void TakeAbilityDamage(int damage, GameObject source) {
            if (!(aiController.MyCurrentState is EvadeState) && !(aiController.MyCurrentState is DeathState)) {
                // order is important here.  we want to set target before taking damage because taking damage could kill us, and we don't want to re-trigger and agro on someone after we are dead
                controller.SetTarget(source);
                base.TakeAbilityDamage(damage, source);
            }
        }
        */

        public void HandleDie(CharacterStats _characterStats) {
            //Debug.Log(gameObject.name + ".AICombat.Die()");
            if (!((baseCharacter.CharacterController as AIController).MyCurrentState is DeathState)) {

                (baseCharacter.CharacterController as AIController).ChangeState(new DeathState());
                //Destroy(gameObject);
                // drop loot
            }
        }

        public override bool EnterCombat(IAbilityCaster target) {
            //Debug.Log(gameObject.name + ".AICombat.EnterCombat()");
            bool returnResult = base.EnterCombat(target);
            return returnResult;
        }

        public BaseAbility GetValidAttackAbility() {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            List<BaseAbility> returnList = new List<BaseAbility>();

            if (MyBaseCharacter != null && MyBaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in MyBaseCharacter.CharacterAbilityManager.MyAbilityList.Values) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                    if (baseAbility.CanCastOnEnemy && MyBaseCharacter.CharacterAbilityManager.CanCastAbility(baseAbility) && baseAbility.CanUseOn(MyBaseCharacter.CharacterController.MyTarget, MyBaseCharacter.CharacterAbilityManager as IAbilityCaster)) {
                        //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                        //if (baseAbility.MyCanCastOnEnemy) {
                        returnList.Add(baseAbility);
                    }
                    //}
                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;
        }

        public BaseAbility GetMeleeAbility() {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            if (MyBaseCharacter != null && MyBaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in MyBaseCharacter.CharacterAbilityManager.MyAbilityList.Values) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                    if (baseAbility.CanCastOnEnemy && baseAbility.UseMeleeRange == true) {
                        //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                        //if (baseAbility.MyCanCastOnEnemy) {
                        return baseAbility;
                    }
                    //}
                }
            }
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;
        }

        public int GetMinAttackRange() {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            int returnValue = 0;

            if (MyBaseCharacter != null && MyBaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in MyBaseCharacter.CharacterAbilityManager.MyAbilityList.Values) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                    if (baseAbility.CanCastOnEnemy && baseAbility.UseMeleeRange == false && baseAbility.MaxRange > 0 && (returnValue == 0 || baseAbility.MaxRange < returnValue)) {
                        //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.MyName);
                        returnValue = baseAbility.MaxRange;
                    }
                    //}
                }
            }
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return returnValue;
        }


    }

}