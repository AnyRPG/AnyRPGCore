﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortalInteractable : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    /// <summary>
    /// The ability to cast in order to use this portal
    /// </summary>
    [SerializeField]
    private BaseAbility ability;


    public IAbility MyAbility { get => ability; }
    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyPortalNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyPortalNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log("Portal.Awake()");
        base.Awake();
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
        base.Interact(source);
        //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): about to close interaction window");
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        //Debug.Log(gameObject.name + ".PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
        source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
        return true;
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.fontSize = 50;
        text.color = Color.cyan;
        return true;
    }

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".PortalInteractable.GetCurrentOptionCount()");
        return GetValidOptionCount();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }

}
