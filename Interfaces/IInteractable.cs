using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IInteractable : IDescribable {

        event System.Action<IInteractable> MiniMapStatusUpdateHandler;
        string InteractionPanelTitle { get; set; }
        Interactable Interactable { get; set; }
        bool MyPrerequisitesMet { get; }
        Sprite NamePlateImage { get; }

        bool CanInteract();
        bool Interact(CharacterUnit source);
        void StopInteract();
        bool HasMiniMapText();
        bool HasMiniMapIcon();
        bool SetMiniMapText(TextMeshProUGUI text);
        void SetMiniMapIcon(Image icon);
        void HandleConfirmAction();
        int GetValidOptionCount();
        int GetCurrentOptionCount();
        void HandlePrerequisiteUpdates();
        void SetupScriptableObjects();
        void OrchestratorStart();
        //void OrchestratorFinish();
        void HandlePlayerUnitSpawn();
    }

}