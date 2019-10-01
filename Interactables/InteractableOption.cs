﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractableOption : MonoBehaviour, IInteractable {

    public abstract event System.Action<IInteractable> MiniMapStatusUpdateHandler;

    [SerializeField]
    protected string interactionPanelTitle;

    [SerializeField]
    protected Sprite interactionPanelImage;

    [SerializeField]
    protected Sprite namePlateImage;

    [SerializeField]
    protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

    protected Interactable interactable;

    protected bool componentReferencesInitialized = false;
    protected bool startHasRun = false;

    protected bool eventReferencesInitialized = false;

    public string MyInteractionPanelTitle { get => interactionPanelTitle; set => interactionPanelTitle = value; }
    public Interactable MyInteractable { get => interactable; set => interactable = value; }

    public bool MyPrerequisitesMet {
        get {
            //Debug.Log(gameObject.name + ".InteractableOption.MyPrerequisitesMet");
            foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                if (!prerequisiteCondition.IsMet()) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }
    }

    public virtual Sprite MyIcon { get => interactionPanelImage;  }
    public virtual Sprite MyNamePlateImage { get => namePlateImage; }

    public string MyName { get => (interactionPanelTitle != null && interactionPanelTitle != string.Empty ? interactionPanelTitle : (interactable != null ? interactable.MyName : "interactable is null!")); }

    protected virtual void Awake () {
        //Debug.Log(gameObject.name + ".InteractableOption.Awake(). Setting interactable");
        GetComponentReferences();
    }

    protected virtual void Start() {
        startHasRun = true;
    }

    public virtual void HandleConfirmAction() {
        SystemEventManager.MyInstance.NotifyOnInteractionWithOptionCompleted(this);
    }

    public virtual void GetComponentReferences() {
        //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences()");
        if (componentReferencesInitialized) {
            //Debug.Log("InteractableOption.GetComponentReferences(): already initialized. exiting!");
            return;
        }
        interactable = GetComponent<Interactable>();
        if (interactable == null) {
            //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences(): " + interactable is null);
        }
        componentReferencesInitialized = true;
    }

    public virtual bool CanInteract(CharacterUnit source) {
        return MyPrerequisitesMet;
    }

    /*
    public virtual void InitWindow(ICloseableWindowContents vendorUI) {
        (vendorUI as VendorUI).CreatePages(items);
    }
    */

    public virtual bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".InteractableOption.Interact()");
        // TESTING, ALLOW BASE TO SEND OUT SUCCESSFUL INTERACTION NOTIFICATIONS - MAYBE JUST FORCE CHARACTER TO INTERACT INSTEAD OF DIRECTLY INTERACTING TO AVOID HAVING AN INTERACTABLE SET AND NOT NULL IN PLAYERCONTROLLER
        SystemEventManager.MyInstance.NotifyOnInteractionWithOptionStarted(this);
        return true;
    }

    public virtual void StopInteract() {
        //Debug.Log(gameObject.name + ".InanimateUnit.StopInteract()");
        PlayerManager.MyInstance.MyCharacter.MyCharacterController.StopInteract();
    }

    public virtual bool HasMiniMapText() {
        return false;
    }

    public virtual bool HasMiniMapIcon() {
        return (MyNamePlateImage != null);
    }

    public virtual bool SetMiniMapText(Text text) {
        return (GetCurrentOptionCount() > 0);
    }

    public virtual void SetMiniMapIcon(Image icon) {
        if (GetCurrentOptionCount() > 0) {
            icon.sprite = MyNamePlateImage;
            icon.color = Color.white;
        } else {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
        }
        return;
    }

    public virtual string GetDescription() {
        return string.Format("<color=#ffff00ff>{0}</color>", GetSummary());
    }

    public virtual string GetSummary() {
        return string.Format("{0}", MyName);
    }

    public virtual void OnDisable () {
        // TESTING
        CleanupEventReferences();
    }

    public virtual void CleanupEventReferences() {

    }

    public virtual int GetValidOptionCount() {
        // overwrite me if this type of interactable option has a list of options instead of just one
        return (MyPrerequisitesMet == true ? 1 : 0);
    }

    public virtual int GetCurrentOptionCount() {
        // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
        //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
        return GetValidOptionCount();
    }

    public virtual void HandlePrerequisiteUpdates() {
        // overwrite me
    }
}
