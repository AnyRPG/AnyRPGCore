using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class SkillTrainer : InteractableOption {

    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage : base.MyNamePlateImage); }

    [SerializeField]
    private Skill[] skills;

    [SerializeField]
    private CharacterUnit characterUnit;

    public Skill[] MySkills { get => skills; }


    protected override void Awake() {
        base.Awake();
        //Debug.Log(gameObject.name + ".SkillTrainer.Awake()");
        characterUnit = GetComponent<CharacterUnit>();
    }

    protected override void Start() {
        //Debug.Log(gameObject.name + ".SkillTrainer.Start()");
        base.Start();
        if (interactionPanelTitle == string.Empty) {
            //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
            interactionPanelTitle = "Train Me";
        }
    }

    public void InitWindow(ICloseableWindowContents skillTrainerUI) {
        //Debug.Log(gameObject.name + ".SkillTrainer.InitWindow()");
        (skillTrainerUI as SkillTrainerUI).ShowSkills(this);
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
        base.Interact(source);
        if (!PopupWindowManager.MyInstance.skillTrainerWindow.IsOpen) {
            //Debug.Log(source + " interacting with " + gameObject.name);
            //vendorWindow.MyVendorUI.CreatePages(items);
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow += CleanupEventSubscriptions;
            PopupWindowManager.MyInstance.skillTrainerWindow.OpenWindow();
            return true;
        }
        return false;
    }

    public override void StopInteract() {
        //Debug.Log(gameObject.name + ".SkillTrainer.StopInteract()");
        base.StopInteract();
        //vendorUI.ClearPages();
        PopupWindowManager.MyInstance.skillTrainerWindow.CloseWindow();
    }

    public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
        //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions(windowContents)");
        CleanupEventSubscriptions();
    }

    public override void CleanupEventSubscriptions() {
        //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions()");
        base.CleanupEventSubscriptions();
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.skillTrainerWindow != null && PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents != null) {
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow -= CleanupEventSubscriptions;
        }
    }

    public override void OnDisable() {
        //Debug.Log(gameObject.name + ".SkillTrainer.OnDisable()");
        base.OnDisable();
        CleanupEventSubscriptions();
    }

    public override int GetValidOptionCount() {
        return GetCurrentOptionCount();
    }

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount()");
        int optionCount = 0;
        foreach (Skill skill in skills) {
            if (!PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(skill)) {
                optionCount++;
            }
        }
        //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
        return optionCount;
    }

    public override bool CanInteract(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract()");
        bool returnValue = ((GetCurrentOptionCount() > 0 && MyPrerequisitesMet) ? true : false);
        //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract(): return: " + returnValue);
        return returnValue;
    }

    public override bool CanShowMiniMapIcon() {
        return CanInteract(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
    }

    public override void HandlePrerequisiteUpdates() {
        //Debug.Log(gameObject.name + ".SkillTrainer.HandlePrerequisiteUpdates()");
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}

}