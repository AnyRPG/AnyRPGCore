using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
public class InanimateUnit : InteractableOption, INamePlateUnit  {

    public event System.Action OnInitializeNamePlate = delegate { };
    public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
    public event Action<int, int> HealthBarNeedsUpdate = delegate { };

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private string displayName = string.Empty;

    private NamePlateController namePlate;

    [SerializeField]
    private string unitFrameTarget;

    [SerializeField]
    private Vector3 unitFrameCameraLookOffset;

    [SerializeField]
    private Vector3 unitFrameCameraPositionOffset;

    public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
    public string MyDisplayName { get => displayName; }
    public string MyFactionName { get => string.Empty; }
    public string MyUnitFrameTarget { get => unitFrameTarget; }
    public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
    public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }

    public bool HasHealth() {
        return false;
    }

    private void OnEnable() {
        //Debug.Log(gameObject.name + ": running OnEnable()");
        InitializeNamePlate();
    }

    public override void OnDisable() {
        if (NamePlateManager.MyInstance != null) {
            NamePlateManager.MyInstance.RemoveNamePlate(this as INamePlateUnit);
        }
    }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ": Awake() about to get references to all local components");
        base.Awake();
    }

    protected void Start() {
        //Debug.Log(gameObject.name + ".InanimateUnit.Start()");
        InitializeNamePlate();
    }

    public override void CleanupEventSubscriptions() {
        //Debug.Log(gameObject.name + ".InanimateUnit.CleanupEventSubscriptions()");
        base.CleanupEventSubscriptions();
    }

    public void InitializeNamePlate() {
        //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate()");

        if (interactable.CanInteract()) {
            //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): isStarted && interactable.CanInteract() == true");
            NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this);
            if (_namePlate != null) {
                namePlate = _namePlate;
            }
            OnInitializeNamePlate();
        } else {
            //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): isStarted && interactable.CanInteract() == false");
            return;
        }
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        //Debug.Log(gameObject.name + ".InanimateUnit.SetMiniMapText()");
        text.text = "";
        text.color = new Color32(0, 0, 0, 0);
        //text.fontSize = 50;
        //text.color = Faction.GetFactionColor(baseCharacter.MyFaction);
        return true;
    }

    public override bool CanInteract(CharacterUnit source) {
        return false;
    }

    public override bool Interact(CharacterUnit source) {
        return false;
    }

    public override void StopInteract() {
        base.StopInteract();
    }

    public void OnDestroy() {
        CleanupEventSubscriptions();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
        InitializeNamePlate();
    }
}

}