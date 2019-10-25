using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class StatusEffectPanelController : DraggableWindow
{
    [SerializeField]
    private GameObject statusNodePrefab;

    // how many effects can be shown before not showing anymore.  needed for small controllers like the ones on nameplates
    [SerializeField]
    private int effectLimit = 0;

    private CharacterUnit targetCharacterUnit;

    private List<StatusEffectNodeScript> statusEffectNodeScripts = new List<StatusEffectNodeScript>();

    public int MyEffectLimit { get => effectLimit; set => effectLimit = value; }

    public void SetTarget(CharacterUnit characterUnit) {
        //Debug.Log("StatusEffectPanelController.SetTarget(" + characterUnit.MyDisplayName + ")");
        this.targetCharacterUnit = characterUnit;
        if (targetCharacterUnit.MyCharacter != null && targetCharacterUnit.MyCharacter.MyCharacterStats != null) {
            //Debug.Log("StatusEffectPanelController.SetTarget(" + characterUnit.MyDisplayName + "): checking status effects");
            foreach (StatusEffectNode statusEffectNode in targetCharacterUnit.MyCharacter.MyCharacterStats.MyStatusEffects.Values) {
                SpawnStatusNode(statusEffectNode, characterUnit);
            }
            CreateEventReferences(targetCharacterUnit.MyCharacter.MyCharacterStats as CharacterStats);
        }
    }

    public void ClearTarget() {
        //Debug.Log("StatusEffectPanelController.ClearTarget()");

        // do this first or there will be no character to unsubscribe from
        CleanupEventReferences();

        targetCharacterUnit = null;
        foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodeScripts) {
            if (_statusEffectNodeScript != null) {
                Destroy(_statusEffectNodeScript.gameObject);
            }
        }
        statusEffectNodeScripts.Clear();
    }

    public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
        //Debug.Log("StatusEffectPanelController.HandleStatusEffectUpdate()");
        SpawnStatusNode(statusEffectNode, targetCharacterUnit);
    }

    public void CreateEventReferences(CharacterStats characterStats) {
        //Debug.Log("StatusEffectPanelController.CreateEventReferences()");
        if (characterStats != null) {
            //Debug.Log("StatusEffectPanelController.CreateEventReferences(): characterStats is not null.");
            characterStats.OnStatusEffectAdd += HandleStatusEffectAdd;
        }
    }

    public void CleanupEventReferences() {
        //Debug.Log("StatusEffectPanelController.CleanupEventReferences()");
        if (targetCharacterUnit != null && targetCharacterUnit.MyCharacter != null && targetCharacterUnit.MyCharacter.MyCharacterStats != null) {
            targetCharacterUnit.MyCharacter.MyCharacterStats.OnStatusEffectAdd -= HandleStatusEffectAdd;
        }
    }

    public StatusEffectNodeScript SpawnStatusNode(StatusEffectNode statusEffectNode, CharacterUnit target) {
        //Debug.Log("StatusEffectPanelController.SpawnStatusNode()");

        // clear invalid entries before trying to check for total entries
        List<StatusEffectNodeScript> removeList = new List<StatusEffectNodeScript>();
        foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodeScripts) {
            if (_statusEffectNodeScript == null) {
                removeList.Add(_statusEffectNodeScript);
            }
        }
        foreach (StatusEffectNodeScript _statusEffectNodeScript in removeList) {
            statusEffectNodeScripts.Remove(_statusEffectNodeScript);
        }


        // prevent status effect bars on unit frames from printing too many effects
        if (effectLimit > 0 && statusEffectNodeScripts.Count >= effectLimit) {
            //Debug.Log("StatusEffectPanelController.SpawnStatusNode() Too many nodes(" + statusEffectNodeScripts.Count + "), can't spawn");
            return null;
        }

        // determine if a node with that status effect already exists
        GameObject statusNode = Instantiate(statusNodePrefab, transform);
        StatusEffectNodeScript statusEffectNodeScript = statusNode.GetComponent<StatusEffectNodeScript>();
        if (statusEffectNodeScript != null) {
            statusEffectNodeScript.Initialize(statusEffectNode, target);
        } else {
            //Debug.Log("StatusEffectPanelController.SpawnStatusNode(): statusEffectNodeScript is null!");
        }
        statusEffectNodeScripts.Add(statusEffectNodeScript);

        return statusEffectNodeScript;
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
    }
}

}