using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {
    public class UnitController : MonoBehaviour, INamePlateUnit {

        public event System.Action<GameObject> OnSetTarget = delegate { };
        public event System.Action OnClearTarget = delegate { };
        public event System.Action OnManualMovement = delegate { };

        private INamePlateTarget namePlateTarget;

        [SerializeField]
        protected UnitNamePlateController namePlateController = new UnitNamePlateController();

        [SerializeField]
        private UnitComponentController unitComponentController = null;

        // by default, a unit will enter AI mode if no mode is set before Start()
        [SerializeField]
        private UnitControllerMode unitControllerMode = UnitControllerMode.AI;

        [Header("Patrol")]

        [Tooltip("In AI mode, use these patrol profiles")]
        [SerializeField]
        private List<string> patrolNames = new List<string>();

        [Header("Behavior")]

        [SerializeField]
        private List<string> behaviorNames = new List<string>();

        [Tooltip("instantiate a new behavior profile or not when loading behavior profiles")]
        [SerializeField]
        private bool useBehaviorCopy = false;

        // components
        private NavMeshAgent agent;
        private Rigidbody rigidBody;
        private UnitMotor unitMotor;
        private UnitAnimator unitAnimator;
        private CharacterUnit characterUnit;
        private BaseCharacter baseCharacter;
        private LootableCharacter lootableCharacter = null;
        private PatrolController patrolController;

        // track startup state
        private bool eventSubscriptionsInitialized = false;

        // control logic
        private IState currentState;
        private List<CombatStrategyNode> startedPhaseNodes = new List<CombatStrategyNode>();

        // targeting
        private GameObject target;
        private float distanceToTarget = 0f;

        // track current state
        private bool mounted = false;
        private bool walking = false;
        private bool frozen = false;
        private bool stunned = false;
        private bool levitated = false;
        private bool motorEnabled = true;

        // movement parameters
        private bool useAgent = false;
        private Vector3 startPosition = Vector3.zero;
        private float evadeSpeed = 5f;
        private float leashDistance = 40f;
        private float maxDistanceFromMasterOnMove = 3f;
        private float maxCombatDistanceFromMasterOnMove = 15f;

        // movement tracking
        private float apparentVelocity;
        private Vector3 lastPosition = Vector3.zero;

        // disabled for now, should not have this number in multiple places, just increased hitbox size instead and multiplied capsule height by hitbox size directly.  end numbers should be the same
        //private float hitBoxSizeMultiplier = 1.5f;

        // is this unit under the control of a master unit
        private bool underControl = false;
        private BaseCharacter masterUnit;

        // track the current movement sound overrides
        private MovementSoundArea movementSoundArea = null;

        public INamePlateTarget NamePlateTarget { get => namePlateTarget; set => namePlateTarget = value; }
        public UnitNamePlateController NamePlateController { get => namePlateController; set => namePlateController = value; }
        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public NavMeshAgent NavMeshAgent { get => agent; set => agent = value; }
        public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }
        public UnitMotor UnitMotor { get => unitMotor; set => unitMotor = value; }
        public UnitAnimator UnitAnimator { get => unitAnimator; set => unitAnimator = value; }
        public Vector3 MyStartPosition {
            get {
                return startPosition;
            }
            set {
                startPosition = value;
                LeashPosition = startPosition;
            }
        }
        public Vector3 LeashPosition { get; set; }
        public float DistanceToTarget { get => distanceToTarget; }
        public float EvadeRunSpeed { get => evadeSpeed; }
        public IState CurrentState { get => currentState; set => currentState = value; }
        public float LeashDistance { get => leashDistance; }
        public PatrolController PatrolController { get => patrolController; }
        public GameObject Target { get => target; }
        public BaseCharacter BaseCharacter { get => baseCharacter; }
        public float MovementSpeed {
            get {
                if (UnderControl == true && MasterUnit != null && MasterUnit.UnitController != null) {
                    return MasterUnit.UnitController.MovementSpeed;
                }
                return (walking == false ? baseCharacter.CharacterStats.RunSpeed : baseCharacter.CharacterStats.WalkSpeed);
            }
        }
        public bool UnderControl { get => underControl; set => underControl = value; }
        public BaseCharacter MasterUnit { get => masterUnit; set => masterUnit = value; }
        public bool Frozen { get => frozen; }
        public bool Stunned { get => stunned; set => stunned = value; }
        public bool Levitated { get => levitated; set => levitated = value; }
        public bool ControlLocked {
            get {
                //Debug.Log(gameObject.name + ".BaseController.MyControlLocked: frozen: " + MyFrozen + "; stunned: "  + MyStunned + "; mylevitated: " + MyLevitated);
                return (Frozen || Stunned || Levitated);
            }
        }
        public Vector3 LastPosition { get => lastPosition; set => lastPosition = value; }
        public float ApparentVelocity { get => apparentVelocity; set => apparentVelocity = value; }
        public float AggroRadius {
            get {
                if (baseCharacter != null && baseCharacter.UnitProfile != null) {
                    return baseCharacter.UnitProfile.AggroRadius;
                }
                return 20f;
            }
            set {

            }
        }
        public CombatStrategy MyCombatStrategy {
            get {
                if (baseCharacter != null && baseCharacter.UnitProfile != null) {
                    return baseCharacter.UnitProfile.CombatStrategy;
                }
                return null;
            }
        }
        public UnitControllerMode UnitControllerMode {
            get => unitControllerMode;
        }
        public LootableCharacter LootableCharacter { get => lootableCharacter; set => lootableCharacter = value; }
        public UnitComponentController UnitComponentController { get => unitComponentController; set => unitComponentController = value; }
        public bool Walking { get => walking; set => walking = value; }
        public AudioProfile MovementLoopProfile {
            get {
                if (movementSoundArea != null && movementSoundArea.MovementLoopProfile != null) {
                    return movementSoundArea.MovementLoopProfile;
                }
                if (LevelManager.MyInstance.GetActiveSceneNode().MovementLoopProfile != null) {
                    return LevelManager.MyInstance.GetActiveSceneNode().MovementLoopProfile;
                }
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.MovementAudioProfiles != null && baseCharacter.UnitProfile.MovementAudioProfiles.Count > 0) {
                    return baseCharacter.UnitProfile.MovementAudioProfiles[0];
                }
                return null;
            }
        }

        public AudioProfile MovementHitProfile {
            get {
                if (movementSoundArea != null && movementSoundArea.MovementHitProfile != null) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetMovementHitProfile: return movementSoundArea.MovementHitProfile");
                    return movementSoundArea.MovementHitProfile;
                }
                if (LevelManager.MyInstance.GetActiveSceneNode().MovementHitProfile != null) {
                    return LevelManager.MyInstance.GetActiveSceneNode().MovementHitProfile;
                }
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.MovementAudioProfiles != null && baseCharacter.UnitProfile.MovementAudioProfiles.Count > 0) {
                    return baseCharacter.UnitProfile.MovementAudioProfiles[0];
                }
                return null;
            }
        }

        public List<string> PatrolNames { get => patrolNames; set => patrolNames = value; }
        public bool Mounted { get => mounted; set => mounted = value; }
        public List<string> BehaviorNames { get => behaviorNames; set => behaviorNames = value; }
        public bool UseBehaviorCopy { get => useBehaviorCopy; set => useBehaviorCopy = value; }

        public void SetUnitControllerMode(UnitControllerMode unitControllerMode) {
            this.unitControllerMode = unitControllerMode;
            if (unitControllerMode == UnitControllerMode.AI) {
                EnableAI();
            } else if (unitControllerMode == UnitControllerMode.Player) {
                EnablePlayer();
            } else if (unitControllerMode == UnitControllerMode.Pet) {
                EnablePet();
            } else if (unitControllerMode == UnitControllerMode.Preview) {
                EnablePreview();
            } else if (unitControllerMode == UnitControllerMode.Mount) {
                EnableMount();
            }
        }

        protected void Awake() {
            GetComponentReferences();
            CreateEventSubscriptions();
        }

        private void Start() {
            SetStartPosition();
            if (unitAnimator != null) {
                unitAnimator.OrchestratorFinish();
            }

            if (unitControllerMode == UnitControllerMode.AI) {
                EnableAI();
            } else if (unitControllerMode == UnitControllerMode.Player) {
                EnablePlayer();
            }
        }

        private void SetStartPosition() {
            Vector3 correctedPosition = Vector3.zero;
            if (unitMotor != null) {
                correctedPosition = unitMotor.CorrectedNavmeshPosition(transform.position);
            }
            MyStartPosition = (correctedPosition != Vector3.zero ? correctedPosition : transform.position);
        }

        public void EnablePlayer() {
            unitComponentController.AggroRangeController.DisableAggro();
            unitComponentController.InteractableRange.gameObject.SetActive(false);
        }


        public void CreateEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this);
            }

            RemoveControlEffects();

            CleanupEventSubscriptions();
        }

        public void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
            lootableCharacter = GetComponent<LootableCharacter>();
            agent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();
            characterUnit = GetComponent<CharacterUnit>();

            unitMotor = new UnitMotor(this);
            unitAnimator = new UnitAnimator(this);
            patrolController = new PatrolController(this);
            namePlateController.Setup(this);

            if (UnitControllerMode == UnitControllerMode.AI) {
                useAgent = true;
            }
        }

        public void SetMovementSoundArea(MovementSoundArea movementSoundArea) {
            //Debug.Log(gameObject.name + ".CharacterUnit.SetMovementSoundArea()");
            if (movementSoundArea != this.movementSoundArea) {
                this.movementSoundArea = movementSoundArea;
            }
        }

        public void UnsetMovementSoundArea(MovementSoundArea movementSoundArea) {
            //Debug.Log(gameObject.name + ".CharacterUnit.UnsetMovementSoundArea()");
            if (movementSoundArea == this.movementSoundArea) {
                this.movementSoundArea = null;
            }
        }

        public void EnablePet() {
        }

        public void EnableMount() {
        }

        public void EnablePreview() {
            DisableAgent();

            // prevent preview unit from moving around
            if (rigidBody != null) {
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                rigidBody.useGravity = false;
            }
        }

        public void EnableAI() {
            // this needs to be done before changing state or idle -> patrol transition will not work because of an inactive navmeshagent
            EnableAgent();

            if (baseCharacter != null && baseCharacter.MySpawnDead == true) {
                ChangeState(new DeathState());
            } else {
                ChangeState(new IdleState());
            }

            SetAggroRange();
        }

       

        public void EnableMotor() {
            motorEnabled = true;
        }

        public void DisableMotor() {
            motorEnabled = false;
        }

        public virtual void CancelMountEffects() {
            if (mounted == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): canCast and character is mounted");

                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects");
                    if (statusEffectNode.StatusEffect is MountEffect) {
                        //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects: found a mount effect");
                        statusEffectNode.CancelStatusEffect();
                        break;
                    }
                }
            }

            // update apparent velocity so any spellcast that caused the cancel mount is not interrupted
            if (baseCharacter != null) {
                baseCharacter.UnitController.LastPosition = transform.position;
            }
        }



        public void FollowTarget(GameObject target, float minAttackRange = -1f) {
            //Debug.Log(gameObject.name + ": AIController.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + ")");
            if (!(currentState is DeathState)) {
                BaseCharacter.UnitController.UnitMotor.FollowTarget(target, minAttackRange);
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

        public void OnEnable() {
            // TESTING DISABLE - THIS IS RUN IN START
            //CreateEventSubscriptions();
        }

        public void Update() {
            if (baseCharacter.CharacterStats.IsAlive == false) {
                // can't handle movement when dead
                return;
            }

            if (motorEnabled) {
                unitMotor.Update();
            }
            UpdateApparentVelocity();
            if (ApparentVelocity > 0.1f) {
                baseCharacter.CharacterAbilityManager.HandleManualMovement();
            }
            HandleMovementAudio();
        }

        public void FixedUpdate() {
            if (target != null) {
                distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            }
            if (ControlLocked) {
                // can't allow any action if we are stunned/frozen/etc
                //Debug.Log(gameObject.name + ".AIController.FixedUpdate(): controlLocked: " + MyControlLocked);
                return;
            }
            currentState.Update();
            if (motorEnabled) {
                unitMotor.FixedUpdate();
            }
        }

        public void SetAggroRange() {
            if (unitComponentController.AggroRangeController != null) {
                //Debug.Log(gameObject.name + ".AIController.Awake(): setting aggro range");
                unitComponentController.AggroRangeController.SetAgroRange(AggroRadius);
            }
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }

        public bool StartCombatPhase(CombatStrategyNode combatStrategyNode) {
            if (!startedPhaseNodes.Contains(combatStrategyNode)) {
                startedPhaseNodes.Add(combatStrategyNode);
                return true;
            }
            return false;
        }

        public void ApplyControlEffects(BaseCharacter source) {
            //Debug.Log(gameObject.name + ".AIController.ApplyControlEffects()");
            if (!underControl) {
                underControl = true;
                masterUnit = source;
                // done so pets of player unit wouldn't attempt to attack npcs questgivers etc
                //masterUnit.MyCharacterController.OnSetTarget += SetTarget;
                if (masterUnit == null) {
                    Debug.Log(gameObject.name + ".AIController.ApplyControlEffects(): masterUnit is null, returning");
                    return;
                }
                masterUnit.UnitController.OnClearTarget += ClearTarget;
                masterUnit.CharacterAbilityManager.OnAttack += OnMasterAttack;
                masterUnit.CharacterCombat.OnDropCombat += OnMasterDropCombat;
                masterUnit.UnitController.OnManualMovement += OnMasterMovement;

                // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
                BaseCharacter.CharacterCombat.MyAggroTable.ClearTable();
                baseCharacter.CharacterFactionManager.NotifyOnReputationChange();
                SetMasterRelativeDestination();
            } else {
                //Debug.Log("Can only be under the control of one master at a time");
            }
        }

        public void RemoveControlEffects() {
            if (underControl && masterUnit != null) {
                //masterUnit.MyCharacterController.OnSetTarget -= SetTarget;
                masterUnit.UnitController.OnClearTarget -= ClearTarget;
                masterUnit.CharacterAbilityManager.OnAttack -= OnMasterAttack;
                masterUnit.CharacterCombat.OnDropCombat -= OnMasterDropCombat;
                masterUnit.UnitController.OnManualMovement -= OnMasterMovement;
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
            if (UnderControl == false) {
                // only do this stuff if we actually have a master
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not under control");
                return;
            }
            //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination()");

            // stand to the right of master by one meter
            Vector3 masterRelativeDestination = masterUnit.CharacterUnit.gameObject.transform.position + masterUnit.CharacterUnit.gameObject.transform.TransformDirection(Vector3.right);
            float usedMaxDistance = 0f;
            if (baseCharacter.CharacterCombat.GetInCombat() == true) {
                usedMaxDistance = maxCombatDistanceFromMasterOnMove;
            } else {
                usedMaxDistance = maxDistanceFromMasterOnMove;
            }

            if (Vector3.Distance(gameObject.transform.position, masterUnit.CharacterUnit.gameObject.transform.position) > usedMaxDistance && Vector3.Distance(LeashPosition, masterUnit.CharacterUnit.gameObject.transform.position) > usedMaxDistance) {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): setting master relative destination");
                masterRelativeDestination = SetDestination(masterRelativeDestination);
                LeashPosition = masterRelativeDestination;
            } else {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not greater than " + usedMaxDistance);
            }

        }

        public void OnMasterAttack(BaseCharacter target) {
            SetTarget(target.CharacterUnit.gameObject);
        }

        public void OnMasterDropCombat() {
            baseCharacter.CharacterCombat.TryToDropCombat();
        }

        public void UpdateTarget() {
            //Debug.Log(gameObject.name + ": UpdateTarget()");
            if (baseCharacter == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter is null!!!");
                return;
            }
            if (baseCharacter.CharacterCombat == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat is null. (ok for non combat units)");
                return;
            }
            if (baseCharacter.CharacterCombat.MyAggroTable == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat.MyAggroTable is null!!!");
                return;
            }
            AggroNode topNode;
            if (underControl) {
                topNode = masterUnit.CharacterCombat.MyAggroTable.MyTopAgroNode;
            } else {
                topNode = baseCharacter.CharacterCombat.MyAggroTable.MyTopAgroNode;
            }

            if (topNode == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget() and the topnode was null");
                if (Target != null) {
                    ClearTarget();
                }
                if (baseCharacter.CharacterCombat.GetInCombat() == true) {
                    baseCharacter.CharacterCombat.TryToDropCombat();
                }
                return;
            }
            /*
            if (MyTarget != null && MyTarget == topNode.aggroTarget.gameObject) {
                //Debug.Log(gameObject.name + ": UpdateTarget() and the target remained the same: " + topNode.aggroTarget.name);
            }
            */
            topNode.aggroValue = Mathf.Clamp(topNode.aggroValue, 0, float.MaxValue);
            if (Target == null) {
                //Debug.Log(gameObject.name + ".AIController.UpdateTarget(): target was null.  setting target: " + topNode.aggroTarget.gameObject.name);
                SetTarget(topNode.aggroTarget.gameObject);
                return;
            }
            if (Target != topNode.aggroTarget.gameObject) {
                //Debug.Log(gameObject.name + ".AIController.UpdateTarget(): " + topNode.aggroTarget.gameObject.name + "[" + topNode.aggroValue + "] stole agro from " + MyTarget);
                ClearTarget();
                SetTarget(topNode.aggroTarget.gameObject);
            }
        }

        public Vector3 SetDestination(Vector3 destination) {
            //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position);
            if (!(currentState is DeathState)) {
                CommonMovementNotifier();
                return BaseCharacter.UnitController.UnitMotor.MoveToPoint(destination);
            } else {
                //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position + ". WE ARE DEAD, DOING NOTHING");
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Meant to be called when the enemy has finished evading and returned to the spawn position
        /// </summary>
        public void Reset() {
            //Debug.Log(gameObject.name + ".AIController.Reset()");
            target = null;
            // testing - comment out below.  is there any time we ever expand or reduce it?  if not, then below line is not necessary ?
            //AggroRadius = initialAggroRange;
            if (baseCharacter != null) {
                baseCharacter.CharacterStats.ResetResourceAmounts();
                if (baseCharacter.UnitController != null && baseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.MyMovementSpeed = MovementSpeed;
                    BaseCharacter.UnitController.UnitMotor.ResetPath();
                } else {
                    //Debug.Log(gameObject.name + ".AIController.Reset(): baseCharacter.myanimatedunit was null!");
                }
            } else {
                //Debug.Log(gameObject.name + ".AIController.Reset(): baseCharacter was null!");
            }
        }

        public void DisableAggro() {
            //Debug.Log(gameObject.name + "AIController.DisableAggro()");
            if (unitComponentController.AggroRangeController != null) {
                unitComponentController.AggroRangeController.DisableAggro();
                return;
            }
            //Debug.Log(gameObject.name + "AIController.DisableAggro(): AGGRORANGE IS NULL!");
        }

        public void EnableAggro() {
            //Debug.Log(gameObject.name + "AIController.EnableAggro()");
            if (unitComponentController.AggroRangeController != null) {
                unitComponentController.AggroRangeController.EnableAggro();
            }
        }

        public bool AggroEnabled() {
            if (unitComponentController.AggroRangeController != null) {
                return unitComponentController.AggroRangeController.AggroEnabled();
            }
            return false;
        }

        public float GetMinAttackRange() {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                return BaseCharacter.CharacterCombat.GetMinAttackRange(MyCombatStrategy.GetAttackRangeAbilityList(BaseCharacter as BaseCharacter));
            } else {
                // get random attack if no strategy exists
                return BaseCharacter.CharacterCombat.GetMinAttackRange(BaseCharacter.CharacterCombat.GetAttackRangeAbilityList());
            }
        }


        public bool HasMeleeAttack() {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility meleeAbility = MyCombatStrategy.GetMeleeAbility(BaseCharacter as BaseCharacter);
                if (meleeAbility != null) {
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                BaseAbility validAttackAbility = BaseCharacter.CharacterCombat.GetMeleeAbility();
                if (validAttackAbility != null) {
                    //Debug.Log(gameObject.name + ".AIController.CanGetValidAttack(" + beginAttack + "): Got valid attack ability: " + validAttackAbility.MyName);
                    return true;
                }
            }

            return false;
        }


        public void ResetCombat() {

            // PUT CODE HERE TO CHECK IF THIS ACTUALLY HAS MUSIC PROFILE, OTHERWISE MOBS WITH A STRATEGY BUT NO PROFILE THAT DIE MID BOSS FIGHT CAN RESET MUSIC

            if (MyCombatStrategy != null) {
                if (MyCombatStrategy.HasMusic() == true) {
                    //Debug.Log(gameObject.name + ".AIController.ResetCombat(): attempting to turn off fight music");
                    AudioProfile musicProfile = LevelManager.MyInstance.GetActiveSceneNode().BackgroundMusicProfile;
                    if (musicProfile != null) {
                        //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): music profile was set");
                        if (musicProfile.AudioClip != null && AudioManager.MyInstance.MusicAudioSource.clip != musicProfile.AudioClip) {
                            //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): playing default music");
                            AudioManager.MyInstance.PlayMusic(musicProfile.AudioClip);
                        }
                    } else {
                        // There was no music, turn it off instead
                        AudioManager.MyInstance.StopMusic();
                    }
                }
                ResetCombatStrategy();
            }
        }

        public void ResetCombatStrategy() {
            startedPhaseNodes.Clear();
        }

        public void SetUseRootMotion(bool useRootMotion) {
            if (UnitMotor != null) {
                UnitMotor.MyUseRootMotion = false;
            }
        }

        private void HandleMovementAudio() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);
            if (baseCharacter.UnitProfile == null || baseCharacter.UnitProfile.MovementAudioProfiles == null || baseCharacter.UnitProfile.MovementAudioProfiles.Count == 0 || baseCharacter.UnitProfile.PlayOnFootstep == true) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            if (apparentVelocity >= (baseCharacter.CharacterStats.RunSpeed / 2f)) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
                if (!unitComponentController.MovementIsPlaying()) {
                    unitComponentController.PlayMovement(MovementLoopProfile.AudioClip, true);
                }
            } else {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): not up to run speed");
                if (unitComponentController.MovementIsPlaying()) {
                    unitComponentController.StopMovement();
                }
            }
        }

        public void UpdateApparentVelocity() {
            // yes this is being called in update, not fixedupdate, but it's only checked when we are standing still trying to cast, so framerates shouldn't be an issue
            if (BaseCharacter != null && BaseCharacter.CharacterUnit != null) {
                apparentVelocity = Vector3.Distance(BaseCharacter.CharacterUnit.transform.position, lastPosition) * (1 / Time.deltaTime);
                lastPosition = BaseCharacter.CharacterUnit.transform.position;
            }

        }

        public void ProcessLevelUnload() {
            ClearTarget();
            CancelMountEffects();
        }

        public void Agro(CharacterUnit agroTarget) {
            // at this level, we are just pulling both parties into combat.

            if (currentState is DeathState) {
                // can't be in combat when dead
                return;
            }

            CharacterUnit targetCharacterUnit = agroTarget;
            if (targetCharacterUnit == null) {
                //Debug.Log("no character unit on target");
            } else if (targetCharacterUnit.BaseCharacter == null) {
                // nothing for now
            } else if (targetCharacterUnit.BaseCharacter.CharacterCombat == null) {
                //Debug.Log("no character combat on target");
            } else {
                if (baseCharacter.CharacterCombat == null) {
                    //Debug.Log("for some strange reason, combat is null????");
                    // like inanimate units
                } else {
                    // moved liveness check into EnterCombat to centralize logic because there are multiple entry points to EnterCombat
                    targetCharacterUnit.BaseCharacter.CharacterCombat.EnterCombat(BaseCharacter);
                    baseCharacter.CharacterCombat.EnterCombat(targetCharacterUnit.BaseCharacter);
                }
                //Debug.Log("combat is " + combat.ToString());
                //Debug.Log("mytarget is " + MyTarget.ToString());
            }
        }

        public void FreezeCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.FreezeCharacter(): ");
            frozen = true;
            if (BaseCharacter.UnitController != null) {
                baseCharacter.UnitController.FreezePositionXZ();
                if (BaseCharacter.UnitController.UnitAnimator != null) {
                    BaseCharacter.UnitController.UnitAnimator.MyAnimator.enabled = false;
                }
                if (BaseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.FreezeCharacter();
                }
            }
        }

        public void UnFreezeCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.UnFreezeCharacter(): ");
            frozen = false;
            FreezeRotation();
                if (unitAnimator != null) {
                    UnitAnimator.MyAnimator.enabled = true;
                }
                if (unitMotor != null) {
                    unitMotor.UnFreezeCharacter();
                }
        }

        public void StunCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): ");
            if (stunned == true) {
                // testing -- avoid triggering stun animation multiple times
                return;
            }
            stunned = true;
            if (BaseCharacter.UnitController != null) {
                baseCharacter.UnitController.FreezePositionXZ();
                if (BaseCharacter.UnitController.UnitAnimator != null) {
                    BaseCharacter.UnitController.UnitAnimator.HandleStunned();
                } else {
                    //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): characteranimator was null");
                }
                if (BaseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.FreezeCharacter();
                } else {
                    //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): charactermotor was null");
                }
            } else {
                //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): animated unit was null");
            }
        }

        public void UnStunCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.UnStunCharacter(): ");
            stunned = false;
            if (BaseCharacter.UnitController != null) {
                baseCharacter.UnitController.FreezeRotation();
                if (BaseCharacter.UnitController.UnitAnimator != null) {
                    BaseCharacter.UnitController.UnitAnimator.HandleUnStunned();
                } else {
                    //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): characteranimator was null");
                }
                if (BaseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.UnFreezeCharacter();
                } else {
                    //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): charactermotor was null");
                }
            } else {
                //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): animated unit was null");
            }
        }

        public void LevitateCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.LevitateCharacter(): ");
            levitated = true;
            if (BaseCharacter.UnitController != null) {
                baseCharacter.UnitController.FreezePositionXZ();
                if (BaseCharacter.UnitController.UnitAnimator != null) {
                    BaseCharacter.UnitController.UnitAnimator.HandleLevitated();
                }
                if (BaseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.FreezeCharacter();
                }
            }
        }

        public void UnLevitateCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.UnLevitateCharacter(): ");
            levitated = false;
            if (BaseCharacter.UnitController != null) {
                baseCharacter.UnitController.FreezeRotation();
                if (BaseCharacter.UnitController.UnitAnimator != null) {
                    BaseCharacter.UnitController.UnitAnimator.HandleUnLevitated();
                }
                if (BaseCharacter.UnitController.UnitMotor != null) {
                    BaseCharacter.UnitController.UnitMotor.UnFreezeCharacter();
                }
            }
        }


        public void SetTarget(GameObject newTarget) {
            //Debug.Log(gameObject.name + ": BaseController: setting target: " + newTarget.name);
            if (unitControllerMode == UnitControllerMode.AI) {
                if (currentState is DeathState || currentState is EvadeState) {
                    return;
                }
                if (Target == null) {
                    target = newTarget;
                }
                //Debug.Log("my target is " + MyTarget.ToString());

                // moved this whole block inside the evade check because it doesn't make sense to agro anything while you are evading
                CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    Agro(targetCharacterUnit);
                }
            } else {
                target = newTarget;
            }
            OnSetTarget(target);
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ": basecontroller.ClearTarget()");
            target = null;
            // FIX ME (reenable possibly?)
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitMotor != null) {
                baseCharacter.UnitController.UnitMotor.StopFollowingTarget();
            }
            OnClearTarget();
        }

        private Vector3 GetHitBoxCenter() {
            //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter()");
            if (baseCharacter == null) {
                //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): baseCharacter is null!");
                return Vector3.zero;
            }
            if (baseCharacter.CharacterUnit == null) {
                //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): baseCharacter.MyCharacterUnit is null!");
                return Vector3.zero;
            }
            Vector3 returnValue = baseCharacter.CharacterUnit.transform.TransformPoint(baseCharacter.CharacterUnit.gameObject.GetComponent<CapsuleCollider>().center) + (baseCharacter.CharacterUnit.transform.forward * (baseCharacter.CharacterUnit.HitBoxSize / 2f));
            //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter() Capsule Collider Center is:" + baseCharacter.MyCharacterUnit.transform.TransformPoint(baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().center));
            return returnValue;
        }

        public Vector3 GetHitBoxSize() {
            if (baseCharacter == null) {
                return Vector3.zero;
            }
            if (baseCharacter.CharacterUnit == null) {
                return Vector3.zero;
            }
            // testing disable size multiplier and just put it straight into the hitbox.  it is messing with character motor because we stop moving toward a character that is 0.5 units outside of the hitbox
            //return new Vector3(baseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier, baseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().height * hitBoxSizeMultiplier, baseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier);
            return new Vector3(baseCharacter.CharacterUnit.HitBoxSize, baseCharacter.CharacterUnit.MyCapsuleCollider.bounds.extents.y * 3f, baseCharacter.CharacterUnit.HitBoxSize);
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
                if (baseCharacter != null && baseCharacter.CharacterUnit != null && baseCharacter.CharacterUnit.gameObject.GetComponent<CapsuleCollider>() == null) {
                    return;
                }

                //Debug.Log(gameObject.name + ".BaseController.OnDrawGizmos(): hit box center is :" + GetHitBoxCenter());
                Gizmos.color = Color.red;
                //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
                Gizmos.DrawWireCube(GetHitBoxCenter(), GetHitBoxSize());
            }
        }

        public void OnDestroy() {
            StopAllCoroutines();
        }

        public void CommonMovementNotifier() {
            OnManualMovement();
        }

        public bool CanGetValidAttack(bool beginAttack = false) {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility validCombatStrategyAbility = MyCombatStrategy.GetValidAbility(BaseCharacter as BaseCharacter);
                if (validCombatStrategyAbility != null) {
                    BaseCharacter.CharacterAbilityManager.BeginAbility(validCombatStrategyAbility);
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                BaseAbility validAttackAbility = BaseCharacter.CharacterCombat.GetValidAttackAbility();
                if (validAttackAbility != null) {
                    //Debug.Log(gameObject.name + ".AIController.CanGetValidAttack(" + beginAttack + "): Got valid attack ability: " + validAttackAbility.MyName);
                    BaseCharacter.CharacterAbilityManager.BeginAbility(validAttackAbility);
                    return true;
                }
            }

            return false;
        }

        public void FreezePositionXZ() {
            RigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }

        public void FreezeAll() {
            RigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void FreezeRotation() {
            RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void EnableAgent() {
            //Debug.Log(gameObject.name + ".AnimatedUnit.EnableAgent()");
            if (NavMeshAgent != null && useAgent == true) {
                NavMeshAgent.enabled = true;
            }
        }

        public void DisableAgent() {
            if (NavMeshAgent != null) {
                NavMeshAgent.enabled = false;
            }
        }
    }

    public enum UnitControllerMode { Preview, Player, AI, Mount, Pet, Inanimate };

}