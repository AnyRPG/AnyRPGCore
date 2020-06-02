using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface INamePlateUnit {

        event System.Action OnInitializeNamePlate;
        event System.Action<INamePlateUnit> NamePlateNeedsRemoval;
        event System.Action<int, int> ResourceBarNeedsUpdate;

        NamePlateController MyNamePlate { get; set; }
        string MyUnitFrameTarget { get; }
        Vector3 MyUnitFrameCameraPositionOffset { get; set; }
        Vector3 MyUnitFrameCameraLookOffset { get; set; }
        string MyDisplayName { get; }
        string Title { get; }
        Faction Faction { get; }
        Transform MyNamePlateTransform { get; }
        Interactable MyInteractable { get; }
        bool SuppressFaction { get; }
        int Level { get; }

        bool HasHealth();
        int CurrentHealth();
        int MaxHealth();
        //void StopInteract();

    }

}