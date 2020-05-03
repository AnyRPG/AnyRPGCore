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
        event System.Action<int, int> HealthBarNeedsUpdate;
        NamePlateController MyNamePlate { get; set; }
        string MyUnitFrameTarget { get; }
        // the offset from the target position where the camera should be placed
        Vector3 MyUnitFrameCameraPositionOffset { get; set; }
        // the offset from the target position where the camera should look
        Vector3 MyUnitFrameCameraLookOffset { get; set; }
        string MyDisplayName { get; }
        string Title { get; }
        Faction MyFaction { get; }
        Transform MyNamePlateTransform { get; }
        Interactable MyInteractable { get; }

        event System.Action<IInteractable> MiniMapStatusUpdateHandler;

        bool CanInteract();
        bool Interact(CharacterUnit source);
        bool HasHealth();
        int CurrentHealth();
        int MaxHealth();
        void StopInteract();
        bool HasMiniMapText();
        bool HasMiniMapIcon();
        bool SetMiniMapText(TextMeshProUGUI text);
        void SetMiniMapIcon(Image icon);

    }

}