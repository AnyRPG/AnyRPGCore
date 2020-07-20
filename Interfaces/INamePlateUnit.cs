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
        event System.Action OnNameChange;

        NamePlateController MyNamePlate { get; set; }
        string UnitFrameTarget { get; }
        Vector3 MyUnitFrameCameraPositionOffset { get; set; }
        Vector3 MyUnitFrameCameraLookOffset { get; set; }
        string UnitDisplayName { get; }
        string Title { get; }
        Faction Faction { get; }
        Transform NamePlateTransform { get; }
        Interactable MyInteractable { get; }
        bool SuppressFaction { get; }
        int Level { get; }

        /// <summary>
        /// true if the unit has a primary resource
        /// </summary>
        /// <returns></returns>
        bool HasPrimaryResource();

        /// <summary>
        /// true if the unit has a secondary resource
        /// </summary>
        /// <returns></returns>
        bool HasSecondaryResource();

        /// <summary>
        /// a special check to see if a resource exists that is a health resource
        /// </summary>
        /// <returns></returns>
        bool HasHealth();
        int CurrentHealth();
        int MaxHealth();

        /// <summary>
        /// Attempt to Initialize a nameplate
        /// </summary>
        void InitializeNamePlate();

    }

}