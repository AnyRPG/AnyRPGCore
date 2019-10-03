using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICombat : CharacterCombat
{
    public override void Start() {
        base.Start();
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

    public override void TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {
        //Debug.Log("AICombat.TakeDamage(" + damage + ", " + sourcePosition + ", " + source + ")");
        if (!((baseCharacter.MyCharacterController as AIController).MyCurrentState is EvadeState) && !((baseCharacter.MyCharacterController as AIController).MyCurrentState is DeathState)) {
            // order is important here.  we want to set target before taking damage because taking damage could kill us, and we don't want to re-trigger and agro on someone after we are dead

            // this should happen automatically inside the update loop of idle state
            //baseCharacter.MyCharacterController.SetTarget(source);
            base.TakeDamage(damage, sourcePosition, source, combatType, combatMagnitude, abilityName);
        }
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
}
