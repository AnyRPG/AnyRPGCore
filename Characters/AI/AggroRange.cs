using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[RequireComponent(typeof(SphereCollider))]
public class AggroRange : MonoBehaviour {
    
    private SphereCollider aggroCollider;

    private BaseCharacter baseCharacter;

    [SerializeField]
    private float aggroRadius = 20f;

    public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

    private void Awake() {
        aggroCollider = GetComponent<SphereCollider>();
        if (aggroCollider == null) {
            Debug.Log("AggroRange.Awake(): aggroCollider is null!");
        }
        DisableAggro();
    }

    private void Start() {
        // do this in start because our awake can run before the awake that sets this in the parent
        baseCharacter = GetComponentInParent<CharacterUnit>().MyCharacter;
        if (baseCharacter == null) {
            Debug.Log("AggroRange.Start(): baseCharacter is null!");
        }
        EnableAggro();
    }

    /// <summary>
    /// Enable the collider attached to this script
    /// </summary>
    public void EnableAggro() {
        aggroCollider.enabled = true;
        aggroCollider.radius = aggroRadius;
    }

    /// <summary>
    /// Disable the collider attached to this script
    /// </summary>
    public void DisableAggro() {
        aggroCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider collider) {
        if (baseCharacter == null) {
            return;
        }
        //Debug.Log((baseCharacter == null ? "null" : baseCharacter.gameObject.name) + ".AggroRange.OnTriggerEnter()");
        // if a player enters our sphere, target him (which has the effect of agro because the idle state will follow any target the enemycontroller has)
        //if (collider.gameObject.GetComponent<PlayerStats>() != null) {
        CharacterUnit _characterUnit = collider.gameObject.GetComponent<CharacterUnit>();
        if (_characterUnit == null) {
            // this was not a character that entered, and therefore we cannot agro it
            return;
        }
        BaseCharacter otherBaseCharacter = _characterUnit.MyCharacter;
        //CharacterCombat _characterCombat = collider.gameObject.GetComponent<CharacterCombat>();
        //Debug.Log("AggroRange.OnTriggerEnter(): baseCharacter: " + baseCharacter.name);
        if (baseCharacter != null && baseCharacter.MyFactionName != string.Empty) {
            //Debug.Log("AggroRange.OnTriggerEnter(): baseCharacter: " + baseCharacter.name + " has null faction");
        }
        if (otherBaseCharacter != null && otherBaseCharacter.MyCharacterCombat != null && otherBaseCharacter.MyCharacterStats.IsAlive == true && otherBaseCharacter.MyFactionName != string.Empty && baseCharacter != null && baseCharacter.MyFactionName != string.Empty) {
            //Debug.Log("AggroRange.OnTriggerEnter(): baseCharacter: " + baseCharacter.name);
            if (Faction.RelationWith(otherBaseCharacter, MyBaseCharacter) <= -1) {
                //Debug.Log(baseCharacter.gameObject.name + ": the object that entered our sphere collider had a baseCharacter attached to it and the relationship is <= -1");
                baseCharacter.MyCharacterCombat.MyAggroTable.AddToAggroTable(_characterUnit, -1);
                //aiController.SetTarget(collider.gameObject);
                //aiCombat.AddToRangeTable(collider.gameObject);
            }
        }
    }

    // this code was really messed up.  it removes objects from aggro tables without dropping combat so you get stuck in combat.
    // also, we shouldn't really remove anyone from an agro table until the cooldown has passed so a better place for this type of thing is in charactercombat during the elapsed combat event time
    /*

private void OnTriggerExit(Collider collider) {
    Debug.Log(baseCharacter.gameObject.name + ".AggroRange.OnTriggerExit()");
    CharacterUnit _characterUnit = collider.gameObject.GetComponent<CharacterUnit>();
    if (_characterUnit == null) {
        // This was not a charcter, and therefore we do not have to remove it from our aggro table
        return;
    }
    BaseCharacter otherBaseCharacter = _characterUnit.MyCharacter;
    if (otherBaseCharacter != null) {
        //Debug.Log("AggroRange.OnTriggerExit: otherBaseCharacter: " + otherBaseCharacter);
        if (otherBaseCharacter.MyCharacterCombat == null) {
            //Debug.Log("otherBaseCharacter.MyCharacterCombat is null");
            return;
        }
        if (otherBaseCharacter.MyCharacterCombat.MyAggroTable == null) {
            Debug.Log("otherBaseCharacter.MyCharacterCombat.MyAggroTable is null");
            return;
        }

        otherBaseCharacter.MyCharacterCombat.MyAggroTable.AttemptRemoveAndBroadcast(_characterUnit);
    } else {
        Debug.Log("otherbasecharacter is null");
    }

}
    */
}

}