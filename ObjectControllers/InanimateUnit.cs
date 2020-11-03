using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class InanimateUnit : Interactable {


        protected override void Awake() {
            base.Awake();
            namePlateController = new BaseNamePlateController(this);
            namePlateController.Init();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Debug.Log(gameObject.name + ".InanimateUnit.OnDestroy()");
            NamePlateController.Cleanup();
        }
    }

}