using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class PlayerMotor : CharacterMotor {

    protected override void Awake() {
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("PlayerMotor.Start()");
        base.Start();
        //(characterUnit.MyCharacter.MyCharacterController as PlayerController).OnManualMovement += StopFollowingTarget;
    }

    // Update is called once per frame
    protected override void FixedUpdate() {
        //Debug.Log("PlayerMotor.FixedUpdate()");
        base.FixedUpdate();
    }

    public void OnDestroy() {
        //(characterUnit.MyCharacter.MyCharacterController as PlayerController).OnManualMovement -= StopFollowingTarget;
    }
}

}