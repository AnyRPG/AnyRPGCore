using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class AIEquipmentManager : CharacterEquipmentManager {

    protected override void Start() {
        CreateComponentReferences();
        base.Start();
        SubscribeToCombatEvents();
    }

    public override void CreateComponentReferences() {
        Debug.Log(gameObject.name + ".AIEquipmentManager.CreateComponentReferences()");
        base.CreateComponentReferences();
        /*
        if (componentReferencesInitialized) {
            return;
        }
        */

        // NPC case
        if (playerUnitObject == null) {
            playerUnitObject = gameObject;
        }

        // NPC case
        if (dynamicCharacterAvatar == null) {
            dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
        }

        //componentReferencesInitialized = true;
    }

    public override void OnDisable() {
        base.OnDisable();
        UnSubscribeFromCombatEvents();
    }

}
