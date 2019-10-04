using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour, ICharacterController {

    public virtual event System.Action<GameObject> OnSetTarget = delegate { };
    public virtual event System.Action OnClearTarget = delegate { };

    protected ICharacter baseCharacter;
    protected GameObject target;

    protected bool walking = false;

    private bool frozen = false;
    private bool stunned = false;
    private bool levitated = false;

    // disabled for now, should not have this number in multiple places, just increased hitbox size instead and multiplied capsule height by hitbox size directly.  end numbers should be the same
    //private float hitBoxSizeMultiplier = 1.5f;

    // is this unit under the control of a master unit
    protected bool underControl = false;

    protected BaseCharacter masterUnit;

    protected bool eventReferencesInitialized = false;
    protected bool startHasRun = false;

    public GameObject MyTarget { get => target; }
    public ICharacter MyBaseCharacter { get => baseCharacter; }
    public float MyMovementSpeed { get => (walking == false ? baseCharacter.MyCharacterStats.MyMovementSpeed : baseCharacter.MyCharacterStats.MyWalkSpeed); }
    public bool MyUnderControl { get => underControl; set => underControl = value; }
    public BaseCharacter MyMasterUnit { get => masterUnit; set => masterUnit = value; }
    public bool MyFrozen { get => frozen; }
    public bool MyStunned { get => stunned; set => stunned = value; }
    public bool MyLevitated { get => levitated; set => levitated = value; }
    public bool MyControlLocked {
        get {
            //Debug.Log(gameObject.name + ".BaseController.MyControlLocked: frozen: " + MyFrozen + "; stunned: "  + MyStunned + "; mylevitated: " + MyLevitated);
            return (MyFrozen || MyStunned || MyLevitated);
        }
    }

    protected virtual void Awake() {
        // overwrite me
    }

    protected virtual void Start() {
        startHasRun = true;
        CreateEventReferences();
    }

    public virtual void CreateEventReferences() {
        //Debug.Log("UnitSpawnNode.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
        eventReferencesInitialized = true;
    }

    public virtual void CleanupEventReferences() {
        //Debug.Log("UnitSpawnNode.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
        eventReferencesInitialized = false;
    }

    public virtual void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventReferences();
    }

    public virtual void OnEnable() {
        CreateEventReferences();
    }

    protected virtual void Update() {
        // overwrite me
    }

    protected virtual void FixedUpdate() {

    }

    public void HandleLevelUnload() {
        ClearTarget();
    }

    public void FreezeCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.FreezeCharacter(): ");
        frozen = true;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.MyAnimator.enabled = false;
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.FreezeCharacter();
    }

    public void UnFreezeCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.UnFreezeCharacter(): ");
        frozen = false;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.MyAnimator.enabled = true;
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.UnFreezeCharacter();
    }

    public void StunCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): ");
        stunned = true;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.HandleStunned();
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.FreezeCharacter();
    }

    public void UnStunCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.UnStunCharacter(): ");
        stunned = false;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.HandleUnStunned();
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.UnFreezeCharacter();
    }

    public void LevitateCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.LevitateCharacter(): ");
        levitated = true;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.HandleLevitated();
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.FreezeCharacter();
    }

    public void UnLevitateCharacter() {
        //Debug.Log(gameObject.name + ".BaseController.UnLevitateCharacter(): ");
        levitated = false;
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.HandleUnLevitated();
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.UnFreezeCharacter();
    }


    public virtual void SetTarget(GameObject newTarget) {
        //Debug.Log(gameObject.name + ": BaseController: setting target: " + newTarget.name);
        target = newTarget;

    }

    public virtual void ClearTarget() {
        //Debug.Log(gameObject.name + ": basecontroller.ClearTarget()");
        target = null;
        // FIX ME (reenable possibly?)
        baseCharacter.MyCharacterUnit.MyCharacterMotor.StopFollowingTarget();
    }

    private Vector3 GetHitBoxCenter() {
        Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter()");
        if (baseCharacter == null) {
            //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): baseCharacter is null!");
            return Vector3.zero;
        }
        if (baseCharacter.MyCharacterUnit == null) {
            //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): baseCharacter.MyCharacterUnit is null!");
            return Vector3.zero;
        }
        Vector3 returnValue = baseCharacter.MyCharacterUnit.transform.TransformPoint(baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().center) + (baseCharacter.MyCharacterUnit.transform.forward * (baseCharacter.MyCharacterStats.MyHitBox / 2f));
        //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter() Capsule Collider Center is:" + baseCharacter.MyCharacterUnit.transform.TransformPoint(baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().center));
        return returnValue;
    }

    private Vector3 GetHitBoxSize() {
        if (baseCharacter == null) {
            return Vector3.zero;
        }
        if (baseCharacter.MyCharacterUnit == null) {
            return Vector3.zero;
        }
        // testing disable size multiplier and just put it straight into the hitbox.  it is messing with character motor because we stop moving toward a character that is 0.5 units outside of the hitbox
        //return new Vector3(baseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier, baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().height * hitBoxSizeMultiplier, baseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier);
        return new Vector3(baseCharacter.MyCharacterStats.MyHitBox, baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().height * baseCharacter.MyCharacterStats.MyHitBox, baseCharacter.MyCharacterStats.MyHitBox);
    }

    public bool IsTargetInHitBox(GameObject newTarget) {
        //Debug.Log(gameObject.name + ".BaseController.IsTargetInHitBox(" + newTarget.name + ")");
        if (newTarget == null) {
            return false;
        }
        Collider[] hitColliders = Physics.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, Quaternion.identity);
        int i = 0;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length) {
            //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "]");
            if (hitColliders[i].gameObject == newTarget) {
                //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "] MATCH!!");
                return true;
            }
            i++;
        }
        return false;
    }

    
    // leave this function here for debugging hitboxes
    void OnDrawGizmos() {
        if (Application.isPlaying) {
            //Debug.Log(gameObject.name + ".BaseController.OnDrawGizmos(): hit box center is :" + GetHitBoxCenter());
            Gizmos.color = Color.red;
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(GetHitBoxCenter(), GetHitBoxSize());
        }
    }

    public virtual void OnDestroy() {
    }

}
