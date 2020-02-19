using AnyRPG;
using UnityEngine;
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
        private float maxDistanceFromMasterOnMove = 3f;

        [SerializeField]
        private float maxCombatDistanceFromMasterOnMove = 15f;

        /// <summary>
        /// A reference to the agro range script 
        /// </summary>
        [SerializeField]
        private AggroRange aggroRange;

        [SerializeField]
        private string combatStrategyName;

        private CombatStrategy combatStrategy;

        private Vector3 startPosition;

        private float distanceToTarget;

        private IState currentState;

        private SphereCollider sphereCollider;

        public Vector3 MyStartPosition {
            get {
                return startPosition;
            }
            set {
                startPosition = value;
                MyLeashPosition = startPosition;
            }
        }
        public Vector3 MyLeashPosition { get; set; }

        private AIPatrol aiPatrol;

        public float MyDistanceToTarget { get => distanceToTarget; }
        public float MyEvadeRunSpeed { get => evadeSpeed; }
        public IState MyCurrentState { get => currentState; set => currentState = value; }
        public float MyLeashDistance { get => leashDistance; }
        public AIPatrol MyAiPatrol { get => aiPatrol; }
        public CombatStrategy MyCombatStrategy { get => combatStrategy; set => combatStrategy = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".AIController.Awake()");
            base.Awake();

            baseCharacter = GetComponent<AICharacter>();
            aiPatrol = GetComponent<AIPatrol>();

            MyAggroRange = initialAggroRange;

            GetCombatStrategy();
        }

        public void GetCombatStrategy() {
            //Debug.Log(gameObject.name + ".AIController.GetCombatStategy()");
            string usedStrategyName = combatStrategyName;
            if (usedStrategyName == null || usedStrategyName == string.Empty) {
                //Debug.Log(gameObject.name + ".AIController.GetCombatStategy(): no strategy configured");
                if (baseCharacter != null && baseCharacter.MyCharacterName != null && baseCharacter.MyCharacterName != string.Empty) {
                    //Debug.Log(gameObject.name + ".AIController.GetCombatStategy(): no strategy configured: using characterName: " + baseCharacter.MyCharacterName);
                    usedStrategyName = baseCharacter.MyCharacterName;
                }
            }
            if (usedStrategyName != null && usedStrategyName != string.Empty) {
                //Debug.Log(gameObject.name + ".AIController.GetCombatStategy(): no strategy configured: using usedStrategyName: " + usedStrategyName);
                combatStrategy = SystemCombatStrategyManager.MyInstance.GetNewResource(usedStrategyName);
                if (combatStrategy == null) {
                    //Debug.Log(gameObject.name + ".AIController.GetCombatStategy(): " + usedStrategyName + " was null");
                } else {
                    //Debug.Log(gameObject.name + ".AIController.GetCombatStategy(): " + usedStrategyName + " GOT COMBAT STRATEGY");
                }
                /*
                if (combatStrategy == null) {
                    Debug.LogError("Unable to get combat strategy: " + usedStrategyName);
                }
                */
            }
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".AIController.Start()");

            // moved next 2 lines here from awake because we need some references first for them to work
            Vector3 correctedPosition = Vector3.zero;
            if (MyBaseCharacter != null && MyBaseCharacter.MyCharacterUnit != null && MyBaseCharacter.MyAnimatedUnit != null && MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor != null) {
                correctedPosition = MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.CorrectedNavmeshPosition(transform.position);
            } else {
                //Debug.Log(gameObject.name + ".AIController.Start(): unable to get a corrected navmesh position for start point because there were no references to a charactermotor");
            }
            MyStartPosition = (correctedPosition != Vector3.zero ? correctedPosition : transform.position);

            // ensure base.Start is run before change to IdleState
            base.Start();

            if (baseCharacter != null && baseCharacter.MySpawnDead == true) {
                ChangeState(new DeathState());
            } else {
                ChangeState(new IdleState());
            }

            // detect if unit has spherecollider (non agro units don't need one)
            SphereCollider sphereCollider = baseCharacter.MyCharacterUnit.GetComponentInChildren<SphereCollider>();
            if (sphereCollider != null) {
                sphereCollider.radius = initialAggroRange;
            }
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
                masterUnit.MyCharacterController.OnClearTarget += ClearTarget;
                masterUnit.MyCharacterAbilityManager.OnAttack += OnMasterAttack;
                masterUnit.MyCharacterCombat.OnDropCombat += OnMasterDropCombat;
                masterUnit.MyCharacterController.OnManualMovement += OnMasterMovement;

                // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
                MyBaseCharacter.MyCharacterCombat.MyAggroTable.ClearTable();
                baseCharacter.MyCharacterFactionManager.NotifyOnReputationChange();
                SetMasterRelativeDestination();
            } else {
                //Debug.Log("Can only be under the control of one master at a time");
            }
        }

        public void RemoveControlEffects() {
            if (underControl && masterUnit != null) {
                //masterUnit.MyCharacterController.OnSetTarget -= SetTarget;
                masterUnit.MyCharacterController.OnClearTarget -= ClearTarget;
                masterUnit.MyCharacterAbilityManager.OnAttack -= OnMasterAttack;
                masterUnit.MyCharacterCombat.OnDropCombat -= OnMasterDropCombat;
                masterUnit.MyCharacterController.OnManualMovement -= OnMasterMovement;
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
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not under control");
                return;
            }
            //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination()");

            // stand to the right of master by one meter
            Vector3 masterRelativeDestination = masterUnit.MyCharacterUnit.gameObject.transform.position + masterUnit.MyCharacterUnit.gameObject.transform.TransformDirection(Vector3.right);
            float usedMaxDistance = 0f;
            if (baseCharacter.MyCharacterCombat.GetInCombat() == true) {
                usedMaxDistance = maxCombatDistanceFromMasterOnMove;
            } else {
                usedMaxDistance = maxDistanceFromMasterOnMove;
            }

            if (Vector3.Distance(gameObject.transform.position, masterUnit.MyCharacterUnit.gameObject.transform.position) > usedMaxDistance && Vector3.Distance(MyLeashPosition, masterUnit.MyCharacterUnit.gameObject.transform.position) > usedMaxDistance) {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): setting master relative destination");
                masterRelativeDestination = SetDestination(masterRelativeDestination);
                MyLeashPosition = masterRelativeDestination;
            } else {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not greater than " + usedMaxDistance);
            }

        }

        public void OnMasterAttack(BaseCharacter target) {
            SetTarget(target.MyCharacterUnit.gameObject);
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
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter is null!!!");
                return;
            }
            if (baseCharacter.MyCharacterCombat == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat is null. (ok for non combat units)");
                return;
            }
            if (baseCharacter.MyCharacterCombat.MyAggroTable == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): baseCharacter.MyCharacterCombat.MyAggroTable is null!!!");
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
                CommonMovementNotifier();
                return MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MoveToPoint(destination);
            } else {
                //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position + ". WE ARE DEAD, DOING NOTHING");
            }
            return Vector3.zero;
        }

        public void FollowTarget(GameObject target) {
            //Debug.Log(gameObject.name + ": AIController.FollowTarget()");
            if (!(currentState is DeathState)) {
                MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.FollowTarget(target);
            }
        }

        /*
        public void AttackCombatTarget() {
            //Debug.Log(gameObject.name + ".AIController.AttackCombatTarget()");
            if (!(currentState is DeathState)) {
                if (target != null) {
                    baseCharacter.MyCharacterCombat.Attack(target.GetComponent<CharacterUnit>().MyCharacter);
                }
            }
        }
        */

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
            if (baseCharacter != null) {
                baseCharacter.MyCharacterStats.ResetHealth();
                if (baseCharacter.MyAnimatedUnit != null) {
                    MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyMovementSpeed = MyMovementSpeed;
                    MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.ResetPath();
                } else {
                    //Debug.Log(gameObject.name + ".AIController.Reset(): baseCharacter.myanimatedunit was null!");
                }
            } else {
                //Debug.Log(gameObject.name + ".AIController.Reset(): baseCharacter was null!");
            }
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

        public void ResetCombat() {

            // PUT CODE HERE TO CHECK IF THIS ACTUALLY HAS MUSIC PROFILE, OTHERWISE MOBS WITH A STRATEGY BUT NO PROFILE THAT DIE MID BOSS FIGHT CAN RESET MUSIC

            if (MyCombatStrategy != null) {
                if (MyCombatStrategy.HasMusic() == true) {
                    //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): combat strategy was not null");
                    if (LevelManager.MyInstance.GetActiveSceneNode().MyBackgroundMusicProfile != null && LevelManager.MyInstance.GetActiveSceneNode().MyBackgroundMusicProfile != null) {
                        //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): music profile was set");
                        AudioProfile musicProfile = LevelManager.MyInstance.GetActiveSceneNode().MyBackgroundMusicProfile;
                        if (musicProfile != null && musicProfile.MyAudioClip != null && AudioManager.MyInstance.MyMusicAudioSource.clip != musicProfile.MyAudioClip) {
                            //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): playing default music");

                            AudioManager.MyInstance.PlayMusic(musicProfile.MyAudioClip);
                        }
                    }
                }
                GetCombatStrategy();
            }
        }

        public bool CanGetValidAttack(bool beginAttack = false) {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility validCombatStrategyAbility = MyCombatStrategy.GetValidAbility(MyBaseCharacter as BaseCharacter);
                if (validCombatStrategyAbility != null) {
                    MyBaseCharacter.MyCharacterAbilityManager.BeginAbility(validCombatStrategyAbility);
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                BaseAbility validAttackAbility = (MyBaseCharacter.MyCharacterCombat as AICombat).GetValidAttackAbility();
                if (validAttackAbility != null) {
                    //Debug.Log(gameObject.name + ".AIController.CanGetValidAttack(" + beginAttack + "): Got valid attack ability: " + validAttackAbility.MyName);
                    MyBaseCharacter.MyCharacterAbilityManager.BeginAbility(validAttackAbility);
                    return true;
                }
            }

            return false;
        }
    }

}