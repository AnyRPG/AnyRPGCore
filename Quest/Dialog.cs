using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
//[System.Serializable]
[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/Dialog")]
public class Dialog : DescribableResource {

    [SerializeField]
    private List<DialogNode> dialogNodes = new List<DialogNode>();

    [SerializeField]
    private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

    /// <summary>
    /// Track whether this dialog has been turned in
    /// </summary>
    private bool turnedIn = false;

    public bool TurnedIn {
        get {
            return turnedIn;
        }

        set {
            turnedIn = value;
            if (turnedIn == true) {
                SystemEventManager.MyInstance.NotifyOnDialogCompleted(this);
            }
        }
    }

    public bool MyPrerequisitesMet {
        get {
            foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                if (!prerequisiteCondition.IsMet()) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }
    }

    public List<DialogNode> MyDialogNodes { get => dialogNodes; set => dialogNodes = value; }
}
}