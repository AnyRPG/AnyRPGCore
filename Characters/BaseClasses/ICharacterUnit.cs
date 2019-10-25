using AnyRPG;
﻿using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
public interface ICharacterUnit {
    NavMeshAgent MyAgent { get; set; }
    BaseCharacter MyCharacter { get; set; }
    Interactable MyInteractable { get; set; }
    NamePlateController MyNamePlate { get; set; }
    Rigidbody MyRigidBody { get; set; }
    CharacterAnimator MyCharacterAnimator { get; }

    void InitializeNamePlate();
    string MyDisplayName { get; }
}
}