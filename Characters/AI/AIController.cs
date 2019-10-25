using AnyRPG;
﻿using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
public class AIController : BaseController {

    [SerializeField]
    private float initialAggroRange = 10f;

    public float MyAggroRange { get; set; }

    private bool isDead = false;

    [SerializeField]
    private float evadeSpeed = 5f;

    [SerializeField]
    private float leashDistance = 40f;

    [SerializeField]
    private float maxDistanceFromMasterOnMove = 10f;

    /// <summary>
    /// A reference to the agro range script 
    /// </summary>
    [SerializeField]
    private AggroRange aggroRange;

    private Vector3 startPosition;

    private float distanceToTarget;

    private IState currentState;

    private SphereCollider sphereCollider;

    public Vector3 MyStartPosition { get { return startPosition; } set { startPosition = value; MyLeashPosition = MyStartPosition; } }
    public Vector3 MyLeashPosition { get; set; }

    private AIPatrol aiPatrol;

    public float MyDistanceToTarget {get => distanceToTarget; }
    public float MyEvadeRunSpeed { get => evadeSpeed; }
    public IState MyCurrentState { get => currentState; set => currentState = value; }
    public float MyLeashDistance { get => leashDistance; }
    public AIPatrol MyAiPatrol { get => aiPatrol; }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".AIController.Awake()");
        base.Awake();

        baseCharacter = GetComponent<AICharacter>() as ICharacter;
        aiPatrol = GetComponent<AIPatrol>();

        MyAggroRange = initialAggroRange;
    }

    protected override void Start() {
        //Debug.Log(gameObject.name + ".AIController.Start()");

        // moved next 2 lines here from awake because we need some references first for them to work
        Vector3 correctedPosition = Vector3.zero;
        if (MyBaseCharacter != null && MyBaseCharacter.MyCharacterUnit != null && MyBaseCharacter.MyCharacterUnit.MyCharacterMotor != null) {
            MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.CorrectedNavmeshPosition(transform.position);
        } else {
            Debug.Log(gameObject.name + ".AIController.Start(): unable to get a corrected navmesh position for start point because there were no references to a charactermotor");
        }
        MyStartPosition = (correctedPosition != Vector3.zero ? correctedPosition : transform.position);

        // ensure base.Start is run before change to IdleState
        base.Start();

        ChangeState(new IdleState());

        // detect if unit has spherecollider (non agro units don't need one)
        SphereCollider sphereCollider = baseCharacter.MyCharacterUnit.GetComponentInChildren<SphereCollider>();
        if (sphereCollider != null) {
            sphereCollider.radius = initialAggroRange;
        }
        //baseCharacter.MyCharacterCombat.OnKillEvent += 
    }

    public void ApplyControlEffects(BaseCharacter source) {
        //Debug.Log(gameObject.name + ".AIController.ApplyControlEffects()");
        if (!underControl) {
            underControl = true;
            masterUnit = source;
            //masterUnit.MyCharacterController.OnSetTarget += SetTarget;
            masterUnit.MyCharacterController.OnClearTarget += ClearTarget;
            masterUnit.MyCharacterCombat.OnAttack += OnMasterAttack;
            masterUnit.MyCharacterCombat.OnDropCombat += OnMasterDropCombat;
            (masterUnit.MyCharacterController as PlayerController).OnManualMovement += OnMasterMovement;

            // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
            MyBaseCharacter.MyCharacterCombat.MyAggroTable.ClearTable();
            SystemEventManager.MyInstance.NotifyOnReputationChange();
            SetMasterRelativeDestination();
        } else {
            Debug.Log("Can only be under the control of one master at a time");
        }
    }

    public void RemoveControlEffects() {
        if (underControl && masterUnit != null) {
            //masterUnit.MyCharacterController.OnSetTarget -= SetTarget;
            masterUnit.MyCharacterController.OnClearTarget -= ClearTarget;
            masterUnit.MyCharacterCombat.OnAttack -= OnMasterAttack;
            masterUnit.MyCharacterCombat.OnDropCombat -= OnMasterDropCombat;
            (masterUnit.MyCharacterController as PlayerController).OnManualMovement -= OnMasterMovement;
        }
        masterUnit = null;
        underControl = false;

        // should we reset leash position to start position here ?
    }

    public void OnMasterMovement() {
        //Debug.Log(gameObject.name + ".AIController.OnMasterMovement()");
        SetMasterRelativeDestination();
    }

    public void SetMasterRelativeDestination() {
        if (MyUnderControl == false) {
            // only do this stuff if we actually have a master
            return;
        }
        //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination()");

        // stand to the right of master by one meter
        Vector3 masterRelativeDestination = masterUnit.MyCharacterUnit.gameObject.transform.position + masterUnit.MyCharacterUnit.gameObject.transform.TransformDirection(Vector3.right);

        if (Vector3.Distance(gameObject.transform.position, masterUnit.MyCharacterUnit.gameObject.transform.position) > maxDistanceFromMasterOnMove) {
            masterRelativeDestination = SetDestination(masterRelativeDestination);
        }

        MyLeashPosition = masterRelativeDestination;
    }

    public void OnMasterAttack(BaseCharacter target) {
        baseCharacter.MyCharacterCombat.Attack(target);
    }

    public void OnMasterDropCombat() {
        baseCharacter.MyCharacterCombat.TryToDropCombat();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (target != null) {
            distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        }
        if (MyControlLocked) {
            // can't allow any action if we are stunned/frozen/etc
            //Debug.Log(gameObject.name + ".AIController.FixedUpdate(): controlLocked: " + MyControlLocked);
            return;
        }
        currentState.Update();
    }

    public void UpdateTarget() {
        //Debug.Log(gameObject.name + ": UpdateTarget()");
        if (baseCharacter == null) {
            Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter is null!!!");
            return;
        }
        if (baseCharacter.MyCharacterCombat == null) {
            //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat is null. (ok for non combat units)");
            return;
        }
        if (baseCharacter.MyCharacterCombat.MyAggroTable == null) {
            Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat.MyAggroTable is null!!!");
            return;
        }
        AggroNode topNode;
        if (underControl) {
            topNode = masterUnit.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
        } else {
            topNode = baseCharacter.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
        }

        if (topNode == null) {
            //Debug.Log(gameObject.name + ": UpdateTarget() and the topnode was null");
            if (MyTarget != null) {
                ClearTarget();
            }
            if (baseCharacter.MyCharacterCombat.GetInCombat() == true) {
                baseCharacter.MyCharacterCombat.TryToDropCombat();
            }
            return;
        }
        /*
        if (MyTarget != null && MyTarget == topNode.aggroTarget.gameObject) {
            //Debug.Log(gameObject.name + ": UpdateTarget() and the target remained the same: " + topNode.aggroTarget.name);
        }
        */
        topNode.aggroValue = Mathf.Clamp(topNode.aggroValue, 0, float.MaxValue);
        if (MyTarget == null) {
            //Debug.Log(gameObject.name + ".AIController.UpdateTarget(): target was null.  setting target: " + topNode.aggroTarget.gameObject.name);
            SetTarget(topNode.aggroTarget.gameObject);
            return;
        }
        if (MyTarget != topNode.aggroTarget.gameObject) {
            //Debug.Log(gameObject.name + ".AIController.UpdateTarget(): " + topNode.aggroTarget.gameObject.name + "[" + topNode.aggroValue + "] stole agro from " + MyTarget);
            ClearTarget();
            SetTarget(topNode.aggroTarget.gameObject);
        }
    }

    public override void SetTarget(GameObject newTarget) {
        if (newTarget == null) {
            Debug.Log(gameObject.name + ".AIController.SetTarget(): newTarget is null");
        }
        //Debug.Log(gameObject.name + ": Setting target to: " + newTarget.name);
        if (!(currentState is DeathState)) {
            if (!(currentState is EvadeState)) {
                if (MyTarget == null) {
                    //Debug.Log("Setting target function and target was previously null");
                    float distance = Vector3.Distance(MyBaseCharacter.MyCharacterUnit.transform.position, newTarget.transform.position);
                    /*MyAggroRange = initialAggroRange;
                    MyAggroRange += distance;
                    */
                    base.SetTarget(newTarget);
                }
                //Debug.Log("my target is " + MyTarget.ToString());

                // moved this whole block inside the evade check because it doesn't make sense to agro anything while you are evading
                CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    Agro(targetCharacterUnit);
                }
            }
        }
    }

    public override void Agro(CharacterUnit agroTarget) {
        //Debug.Log(gameObject.name + ".AIController.Agro(): target: " + target.name);
        if (!(currentState is DeathState)) {
            //CharacterUnit characterUnit = (CharacterUnit) target.GetComponent<ICharacterUnit>();
            base.Agro(agroTarget);
        }
    }

    public Vector3 SetDestination(Vector3 destination) {
        //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position);
        if (!(currentState is DeathState)) {
            // I THINK WE MAY NEED TO SEND IN CORRECTED NAVMESH POSITION HERE
            return MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MoveToPoint(destination);
        }
        return Vector3.zero;
    }

    public void FollowTarget(GameObject target) {
        //Debug.Log(gameObject.name + ": AIController.FollowTarget()");
        if (!(currentState is DeathState)) {
            MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.FollowTarget(target);
        }
    }

    public void AttackCombatTarget() {
        //Debug.Log(gameObject.name + ".AIController.AttackCombatTarget()");
        if (!(currentState is DeathState)) {
            if (target != null) {
                baseCharacter.MyCharacterCombat.Attack(target.GetComponent<CharacterUnit>().MyCharacter);
            }
        }
    }

    public void ChangeState(IState newState) {
        //Debug.Log(gameObject.name + ": ChangeState(" + newState.ToString() + ")");
        if (currentState != null) {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter(this);
    }

    /// <summary>
    /// Meant to be called when the enemy has finished evading and returned to the spawn position
    /// </summary>
    public void Reset() {
        //Debug.Log(gameObject.name + ".AIController.Reset()");
        target = null;
        MyAggroRange = initialAggroRange;
        baseCharacter.MyCharacterStats.ResetHealth();
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyMovementSpeed = MyMovementSpeed;
        MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.ResetPath();
    }

    public void DisableAggro() {
        //Debug.Log(gameObject.name + "AIController.DisableAggro()");
        if (aggroRange != null) {
            aggroRange.DisableAggro();
            return;
        }
        //Debug.Log(gameObject.name + "AIController.DisableAggro(): AGGRORANGE IS NULL!");
    }

    public void EnableAggro() {
        if (aggroRange != null) {
            aggroRange.EnableAggro();
        }
    }

    public override void OnDisable() {
        RemoveControlEffects();
    }

}

}