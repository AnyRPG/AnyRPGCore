using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface INamePlateUnit {

        INamePlateController NamePlateController { get; }
        UnitComponentController UnitComponentController { get; }
        NamePlateProps NamePlateProps { get; }
        //INamePlateTarget NamePlateTarget { get; }
        Interactable Interactable { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
    }

}