using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AICombat : CharacterCombat {

        protected override void CreateEventSubscriptions() {
            base.CreateEventSubscriptions();
            baseCharacter.MyCharacterStats.OnDie += HandleDie;
        }

        protected override void Update() {
            //Debug.Log(gameObject.name + ": Update()");
            base.Update();
            if (!baseCharacter.MyCharacterStats.IsAlive) {
                return;
            }
        }
        /*
            public override void ClearAggro(GameObject target) {
                base.ClearAggro(target);
                aiController.ClearTarget();
            }
            */

        public override bool TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {
            //Debug.Log("AICombat.TakeDamage(" + damage + ", " + sourcePosition + ", " + source + ")");
            if (!((baseCharacter.MyCharacterController as AIController).MyCurrentState is EvadeState) && !((baseCharacter.MyCharacterController as AIController).MyCurrentState is DeathState)) {
                // order is important here.  we want to set target before taking damage because taking damage could kill us, and we don't want to re-trigger and agro on someone after we are dead

                // this should happen automatically inside the update loop of idle state
                //baseCharacter.MyCharacterController.SetTarget(source);
                return base.TakeDamage(damage, sourcePosition, source, combatType, combatMagnitude, abilityName);
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
            if (!((baseCharacter.MyCharacterController as AIController).MyCurrentState is DeathState)) {

                (baseCharacter.MyCharacterController as AIController).ChangeState(new DeathState());
                //Destroy(gameObject);
                // drop loot
            }
        }

        public override bool EnterCombat(BaseCharacter target) {
            //Debug.Log(gameObject.name + ".AICombat.EnterCombat()");
            bool returnResult = base.EnterCombat(target);
            return returnResult;
        }

        public BaseAbility GetValidAttackAbility() {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            List<BaseAbility> returnList = new List<BaseAbility>();

            if (MyBaseCharacter != null && MyBaseCharacter.MyCharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in MyBaseCharacter.MyCharacterAbilityManager.MyAbilityList.Values) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                    if (baseAbility.MyCanCastOnEnemy && MyBaseCharacter.MyCharacterAbilityManager.CanCastAbility(baseAbility) && baseAbility.CanUseOn(MyBaseCharacter.MyCharacterController.MyTarget, MyBaseCharacter as BaseCharacter)) {
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

    }

}