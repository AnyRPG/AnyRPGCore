using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInteractable : IDescribable {

    event System.Action<IInteractable> MiniMapStatusUpdateHandler;
    string MyInteractionPanelTitle { get; set; }
    Interactable MyInteractable { get; set; }
    bool MyPrerequisitesMet { get; }

    bool CanInteract(CharacterUnit source);
    bool Interact(CharacterUnit source);
    void StopInteract();
    bool HasMiniMapText();
    bool HasMiniMapIcon();
    bool SetMiniMapText(Text text);
    void SetMiniMapIcon(Image icon);
    void HandleConfirmAction();
    int GetValidOptionCount();
    int GetCurrentOptionCount();
}
