using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New TeleportEffect",menuName = "AnyRPG/Abilities/Effects/TeleportEffect")]
public class TeleportEffect : InstantEffect {

    // The name of the scene to load
    [SerializeField]
    private string levelName = string.Empty;

    [SerializeField]
    private Vector3 spawnLocation = Vector3.zero;

    public string MyLevelName { get => levelName; set => levelName = value; }

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        if (levelName != null) {
            if (spawnLocation != Vector3.zero) {
                LevelManager.MyInstance.LoadLevel(levelName, spawnLocation);
            } else {
                LevelManager.MyInstance.LoadLevel(levelName);
            }
        }
    }

}

}
