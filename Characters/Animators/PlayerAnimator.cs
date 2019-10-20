using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharacterAnimator {

    protected override void Awake() {
        //Debug.Log("PlayerAnimator.Awake()");
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("PlayerAnimator.Start()");
        base.Start();
    }

    public override void CreateEventReferences() {
        // called from base.start
        base.CreateEventReferences();
        SystemEventManager.MyInstance.OnEquipmentChanged += PerformEquipmentChange;
        SystemEventManager.MyInstance.OnEquipmentRefresh += PerformEquipmentChange;
    }

    public override void CleanupEventReferences() {
        // called from base.onDisable
        base.CleanupEventReferences();
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnEquipmentChanged -= PerformEquipmentChange;
            SystemEventManager.MyInstance.OnEquipmentRefresh -= PerformEquipmentChange;
        }
    }

    // for debugging
    /*
    public override void InitializeAnimator() {
        //Debug.Log(gameObject.name + ".PlayerAnimator.InitializeAnimator()");
        base.InitializeAnimator();
    }

    public override void SetCasting(bool varValue) {
        //Debug.Log(gameObject.name + ".PlayerAnimator.SetCasting(" + varValue + ")");
        base.SetCasting(varValue);
    }

    public override void ClearAnimationBlockers() {
        //Debug.Log(gameObject.name + ".PlayerAnimator.ClearAnimationBlockers()");
        base.ClearAnimationBlockers();
    }
    */

}