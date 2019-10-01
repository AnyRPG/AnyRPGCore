using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTrainer : InteractableOption
{
    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private Skill[] skills;

    [SerializeField]
    private CharacterUnit characterUnit;

    public Skill[] MySkills { get => skills; }

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage : base.MyNamePlateImage); }

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
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindowHandler += CleanupEventReferences;
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

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventReferences(windowContents)");
        CleanupEventReferences();
    }

    public override void CleanupEventReferences() {
        //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventReferences()");
        base.CleanupEventReferences();
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.skillTrainerWindow != null && PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents != null) {
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindowHandler -= CleanupEventReferences;
        }
    }

    public override void OnDisable() {
        //Debug.Log(gameObject.name + ".SkillTrainer.OnDisable()");
        base.OnDisable();
        CleanupEventReferences();
    }

    public override int GetValidOptionCount() {
        return GetCurrentOptionCount();
    }

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount()");
        int optionCount = 0;
        foreach (Skill skill in skills) {
            if (!PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(skill.MyName)) {
                optionCount++;
            }
        }
        return optionCount;
    }

    public override bool CanInteract(CharacterUnit source) {
        return ((GetCurrentOptionCount() > 0 && MyPrerequisitesMet) ? true : false);
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
