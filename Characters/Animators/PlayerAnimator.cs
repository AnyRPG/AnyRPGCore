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
        SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
        SystemEventManager.MyInstance.OnEquipmentRefresh += OnEquipmentChanged;
    }

    public override void CleanupEventReferences() {
        // called from base.onDisable
        base.CleanupEventReferences();
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnEquipmentChanged -= OnEquipmentChanged;
            SystemEventManager.MyInstance.OnEquipmentRefresh -= OnEquipmentChanged;
        }
    }
    public void OnEquipmentChanged(Equipment newItem) {
        OnEquipmentChanged(newItem, null);
    }

        public void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
        //Debug.Log("PlayerAnimator.OnEquipmentChanged(" + (newItem == null ? "null" : newItem.MyName) + ", " + (oldItem == null ? "null" : oldItem.MyName) + ")");
        // Animate grip for weapon when an item is added or removed from hand
        if (newItem != null && newItem.equipSlot == EquipmentSlot.MainHand && (newItem as Weapon).MyDefaultAttackAnimationProfile != null) {
            //Debug.Log("we are animating the weapon");
            //animator.SetLayerWeight(1, 1);
            //if (weaponAnimationsDict.ContainsKey(newItem)) {
            SetAnimationProfileOverride((newItem as Weapon).MyDefaultAttackAnimationProfile);
        } else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.MainHand) {
            //animator.SetLayerWeight(1, 0);
            //Debug.Log("resetting animation profile");
            ResetAnimationProfile();
        }

        // Animate grip for weapon when a shield is added or removed from hand
        if (newItem != null && newItem.equipSlot == EquipmentSlot.OffHand) {
            //Debug.Log("we are animating the shield");
            //animator.SetLayerWeight(2, 1);
        } else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.OffHand) {
            //animator.SetLayerWeight(2, 0);
        }
    }

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


}