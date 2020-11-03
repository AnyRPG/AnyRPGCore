using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class InanimateUnit : NamePlateUnit {

        protected override void Awake() {
            base.Awake();
            namePlateController = new BaseNamePlateController(this);
        }
    }

}