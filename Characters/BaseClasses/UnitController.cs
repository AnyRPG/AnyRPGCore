using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitController : NamePlateUnit, IPersistentObjectOwner {

        public event System.Action<Interactable> OnSetTarget = delegate { };
        public event System.Action OnClearTarget = delegate { };
        public event System.Action OnManualMovement = delegate { };
        public event System.Action OnModelReady = delegate { };
        public event System.Action OnReputationChange = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<CharacterStats> OnBeforeDie = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };
        public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<Faction, Faction> OnFactionChange = delegate { };
        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action<IAbilityCaster, BaseAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };

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

        [Header("Persistence")]

        [SerializeField]
        private PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        // unit profile and settings
        private UnitProfile unitProfile = null;

        // components
        private NavMeshAgent agent = null;
        private Rigidbody rigidBody = null;
        private UnitMotor unitMotor = null;
        private UnitAnimator unitAnimator = null;
        private DynamicCharacterAvatar dynamicCharacterAvatar = null;
        private LootableCharacter lootableCharacter = null;
        private PatrolController patrolController = null;
        private BehaviorController behaviorController = null;
        private UnitMountManager unitMountManager = null;
        //private UnitNamePlateController namePlateController = null;
        private UUID uuid = null;
        private GameObject unitModel = null;

        // track startup state
        private bool profileReady = false;
        private bool modelReady = false;

        // control logic
        private IState currentState;
        private List<CombatStrategyNode> startedPhaseNodes = new List<CombatStrategyNode>();
        //private INamePlateTarget namePlateTarget = null;

        // targeting
        private Interactable target;
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

        //public INamePlateTarget NamePlateTarget { get => namePlateTarget; set => namePlateTarget = value; }
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
        public Interactable Target { get => target; }
        //public BaseCharacter BaseCharacter { get => characterUnit.BaseCharacter; }
        public float MovementSpeed {
            get {
                if (UnderControl == true && MasterUnit != null && MasterUnit.UnitController != null) {
                    return MasterUnit.UnitController.MovementSpeed;
                }
                return (walking == false ? characterUnit.BaseCharacter.CharacterStats.RunSpeed : characterUnit.BaseCharacter.CharacterStats.WalkSpeed);
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
                if (characterUnit.BaseCharacter != null && unitProfile != null) {
                    return unitProfile.AggroRadius;
                }
                return 20f;
            }
            set {

            }
        }
        public CombatStrategy MyCombatStrategy {
            get {
                if (characterUnit.BaseCharacter != null && unitProfile != null) {
                    return unitProfile.CombatStrategy;
                }
                return null;
            }
        }
        public UnitControllerMode UnitControllerMode {
            get => unitControllerMode;
        }
        public LootableCharacter LootableCharacter { get => lootableCharacter; set => lootableCharacter = value; }
        public bool Walking { get => walking; set => walking = value; }
        public AudioProfile MovementLoopProfile {
            get {
                if (movementSoundArea != null && movementSoundArea.MovementLoopProfile != null) {
                    return movementSoundArea.MovementLoopProfile;
                }
                if (LevelManager.MyInstance.GetActiveSceneNode().MovementLoopProfile != null) {
                    return LevelManager.MyInstance.GetActiveSceneNode().MovementLoopProfile;
                }
                if (characterUnit.BaseCharacter != null && unitProfile != null && unitProfile.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
                    return unitProfile.MovementAudioProfiles[0];
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
                if (characterUnit.BaseCharacter != null && unitProfile != null && unitProfile.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
                    return unitProfile.MovementAudioProfiles[0];
                }
                return null;
            }
        }

        public List<string> PatrolNames { get => patrolNames; set => patrolNames = value; }
        public bool Mounted { get => mounted; set => mounted = value; }
        public List<string> BehaviorNames { get => behaviorNames; set => behaviorNames = value; }
        public bool UseBehaviorCopy { get => useBehaviorCopy; set => useBehaviorCopy = value; }
        public UUID UUID { get => uuid; set => uuid = value; }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }
        public DynamicCharacterAvatar DynamicCharacterAvatar { get => dynamicCharacterAvatar; set => dynamicCharacterAvatar = value; }
        public UnitProfile UnitProfile { get => unitProfile; }
        public bool ModelReady { get => modelReady; set => modelReady = value; }
        public override NamePlateProps NamePlateProps {
            get {
                if (unitProfile != null) {
                    return unitProfile.UnitPrefabProps.NamePlateProps;
                }
                return namePlateProps;
            }
        }

        public UnitMountManager UnitMountManager { get => unitMountManager; set => unitMountManager = value; }
        public BehaviorController BehaviorController { get => behaviorController; set => behaviorController = value; }

        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            unitMountManager.SetMountedState(mountUnitController, mountUnitProfile);
        }

        public override void EnableInteraction() {
            // do nothing intentionally, don't want collider disabled or unit will fall through world
            //base.EnableInteraction();
        }

        public override void DisableInteraction() {
            // do nothing intentionally, don't want collider disabled or unit will fall through world
            //base.DisableInteraction();
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

        protected virtual void SetDefaultLayer(string layerName) {
            if (layerName != null && layerName != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(layerName);
                int finalmask = (1 << defaultLayer);
                if (!IsInLayerMask(gameObject.layer, finalmask)) {
                    gameObject.layer = defaultLayer;
                    //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): object was not set to correct layer: " + layerName + ". Setting automatically");
                }
                if (unitModel != null && !IsInLayerMask(unitModel.layer, finalmask)) {
                    UIManager.MyInstance.SetLayerRecursive(unitModel, defaultLayer);
                    //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): model was not set to correct layer: " + layerName + ". Setting automatically");
                }
            }
        }


        /// <summary>
        /// set this unit to be a stationary preview
        /// </summary>
        private void SetPreviewMode() {
            //Debug.Log(gameObject.name + ".UnitController.SetPreviewMode()");
            SetUnitControllerMode(UnitControllerMode.Preview);
            SetDefaultLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);
            DisableAgent();

            // prevent preview unit from moving around
            if (rigidBody != null) {
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                rigidBody.useGravity = false;
            }
        }

        /// <summary>
        /// set this unit to be the pet of characterUnit.BaseCharacter
        /// </summary>
        /// <param name="characterUnit.BaseCharacter"></param>
        public void SetPetMode(BaseCharacter masterBaseCharacter) {
            //Debug.Log(gameObject.name + ".UnitController.SetPetMode()");
            SetUnitControllerMode(UnitControllerMode.Pet);
            SetDefaultLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);

            if (masterBaseCharacter != null) {
                characterUnit.BaseCharacter.CharacterStats.SetLevel(masterBaseCharacter.CharacterStats.Level);
                characterUnit.BaseCharacter.CharacterStats.ApplyControlEffects(masterBaseCharacter);
            }
        }

        private void EnablePetMode() {
            //Debug.Log(gameObject.name + ".UnitController.EnablePetMode()");
            InitializeNamePlateController();
            EnableAICommon();
            ChangeState(new IdleState());
            SetAggroRange();
        }

        /// <summary>
        /// set this unit to be a mount
        /// </summary>
        private void SetMountMode() {
            //Debug.Log(gameObject.name + ".UnitController.SetMountMode()");

            // mount namePlates do not need full initialization, only the position to be set
            namePlateController.SetNamePlatePosition();

            SetUnitControllerMode(UnitControllerMode.Mount);
            SetDefaultLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);
            if (myCollider != null) {
                myCollider.isTrigger = false;
            }
            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
            DisableAgent();
        }

        /// <summary>
        /// set this unit to be a player
        /// </summary>
        private void EnablePlayer() {
            //Debug.Log(gameObject.name + "UnitController.EnablePlayer()");
            InitializeNamePlateController();

            SetDefaultLayer(SystemConfigurationManager.MyInstance.DefaultPlayerUnitLayer);
            unitComponentController.AggroRangeController.DisableAggro();
            unitComponentController.InteractableRange.gameObject.SetActive(false);
            DisableAgent();

            // this code is a quick way to set speed on third party controllers when the player spawns
            if (characterUnit.BaseCharacter.CharacterStats != null) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = characterUnit.BaseCharacter.CharacterStats.RunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);

                eventParam.simpleParams.FloatParam = characterUnit.BaseCharacter.CharacterStats.SprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);

            }
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl) {
                KeyBindManager.MyInstance.SendKeyBindEvents();
            }

            // test - move this here to its initialized
            NamePlateController.NamePlate.SetPlayerOwnerShip();

        }

        /// <summary>
        /// set this unit to be an AI unit
        /// </summary>
        private void EnableAI() {
            Debug.Log(gameObject.name + ".UnitController.EnableAI()");
            InitializeNamePlateController();
            EnableAICommon();

            if (characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.SpawnDead == true) {
                Debug.Log(gameObject.name + ".UnitController.EnableAI() entering death state");
                ChangeState(new DeathState());
            } else {
                ChangeState(new IdleState());
            }
            SetAggroRange();
        }

        private void EnableAICommon() {
            SetDefaultLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);

            // enable agent needs to be done before changing state or idle -> patrol transition will not work because of an inactive navmeshagent
            useAgent = true;
            EnableAgent();

            // ensure player cannot physically push AI units around
            // first set collision mode to avoid unity errors about dynamic detection not supported for kinematic rigidbodies
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rigidBody.isKinematic = true;

            // ensure player is not physically blocked or pushed around by AI units
            myCollider.isTrigger = true;
        }

        public void ConfigurePlayer() {
            PlayerManager.MyInstance.SetUnitController(this);
        }

        public void SetUnitControllerMode(UnitControllerMode unitControllerMode) {
            this.unitControllerMode = unitControllerMode;
            if (unitControllerMode == UnitControllerMode.Player) {
                ConfigurePlayer();
            }
        }

        public void ActivateUnitControllerMode() {
            if (unitControllerMode == UnitControllerMode.AI) {
                EnableAI();
            } else if (unitControllerMode == UnitControllerMode.Player) {
                EnablePlayer();
            } else if (unitControllerMode == UnitControllerMode.Mount) {
                SetMountMode();
            } else if (unitControllerMode == UnitControllerMode.Preview) {
                SetPreviewMode();
            } else if (unitControllerMode == UnitControllerMode.Pet) {
                EnablePetMode();
            }
        }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".UnitController.Awake()");
            base.Awake();

            // create components here instead?  which ones rely on other things like unit profile being set before start?
            namePlateController = new UnitNamePlateController(this);
            unitMotor = new UnitMotor(this);
            unitAnimator = new UnitAnimator(this);
            patrolController = new PatrolController(this);
            behaviorController = new BehaviorController(this);
            unitMountManager = new UnitMountManager(this);
            persistentObjectComponent.Setup(this);

            // allow the base character to initialize.
            characterUnit.BaseCharacter.Init();
        }

        protected override void Start() {

            if (characterUnit.BaseCharacter.UnitProfile == null
                && characterUnit.BaseCharacter.UnitProfileName != null
                && characterUnit.BaseCharacter.UnitProfileName != string.Empty) {
                SetUnitProfile(UnitProfile.GetUnitProfileReference(characterUnit.BaseCharacter.UnitProfileName), UnitControllerMode.AI);
                // setUnitProfile will have spawned a model if it contained one.  If it did not,
                // look for an animator.  If one is found, the model is already attached to this unit.
                if (unitModel == null) {
                    ConfigureAnimator();
                }
            }

            base.Start();
            //Debug.Log(gameObject.name + ".UnitController.Start()");

            persistentObjectComponent.Init();

            SetStartPosition();

            // testing move to individual controller modes
            //InitializeNamePlateController();

            ActivateUnitControllerMode();

            behaviorController.Init();
            patrolController.Init();

        }

        public override void InitializeNamePlateController() {
            // mounts and preview units shouldn't have a namePlateController active
            if (unitControllerMode != UnitControllerMode.Mount && unitControllerMode != UnitControllerMode.Preview) {
                base.InitializeNamePlateController();
            }
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".UnitController.OnDisable()");
            base.OnDisable();
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this);
            }
            RemoveControlEffects();
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".UnitController.GetComponentReferences()");

            if (componentReferencesInitialized == true) {
                return;
            }
            
            // if base character exists, create a character unit and link them
            // do this before the base because the base will create things that need to query the character
            BaseCharacter baseCharacter = GetComponent<BaseCharacter>();
            if (baseCharacter != null) {
                //Debug.Log(gameObject.name + ".UnitController.GetComponentReferences(): found baseCharacter, creating characterUnit");
                characterUnit = new CharacterUnit(this as Interactable, new InteractableOptionProps());
                characterUnit.SetBaseCharacter(baseCharacter);
                baseCharacter.SetUnitController(this);
                AddInteractable(characterUnit);
            }

            base.GetComponentReferences();

            uuid = GetComponent<UUID>();
            lootableCharacter = GetComponent<LootableCharacter>();
            agent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();

            

        }

        /// <summary>
        /// This method is meant to be called after Awake() (automatically run on gameobject creation) and before Start()
        /// </summary>
        /// <param name="unitProfile"></param>
        public void SetUnitProfile(UnitProfile unitProfile, UnitControllerMode unitControllerMode, int unitLevel = -1) {
            //Debug.Log(gameObject.name + "UnitController.SetUnitProfile()");
            this.unitProfile = unitProfile;
            if (characterUnit.BaseCharacter != null) {
                characterUnit.BaseCharacter.SetUnitProfile(unitProfile, true, unitLevel);
            }
            SetUnitControllerMode(unitControllerMode);

            Init();

            SpawnUnitModel();

            SetUnitProfileInteractables();
        }

        private void SetUnitProfileInteractables() {
            //Debug.Log(gameObject.name + "UnitController.SetUnitProfileInteractables()");

            if (unitProfile == null) {
                return;
            }

            // built-in interactable options
            if (unitProfile.LootableCharacterProps.AutomaticCurrency == true || unitProfile.LootableCharacterProps.LootTableNames.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.LootableCharacterProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.DialogProps.DialogList.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.DialogProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.QuestGiverProps.Quests.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.QuestGiverProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.VendorProps.VendorCollections.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.VendorProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.BehaviorProps.BehaviorNames.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.BehaviorProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            // named interactable options
            foreach (InteractableOptionConfig interactableOption in unitProfile.InteractableOptionConfigs) {
                if (interactableOption.InteractableOptionProps != null) {
                    InteractableOptionComponent interactableOptionComponent = interactableOption.InteractableOptionProps.GetInteractableOption(this);
                    interactables.Add(interactableOptionComponent);
                    interactableOptionComponent.HandlePrerequisiteUpdates();
                }
            }
        }

        private void SetStartPosition() {
            Vector3 correctedPosition = Vector3.zero;
            if (unitMotor != null) {
                correctedPosition = unitMotor.CorrectedNavmeshPosition(transform.position);
            }
            MyStartPosition = (correctedPosition != Vector3.zero ? correctedPosition : transform.position);
        }

        public void SpawnUnitModel() {
            //Debug.Log(gameObject.name + "UnitController.SpawnUnitModel()");
            if (unitProfile != null && unitProfile != null && unitProfile.UnitPrefabProps.ModelPrefab != null) {
                unitModel = unitProfile.SpawnModelPrefab(transform, transform.position, transform.forward);
                ConfigureAnimator(unitModel);
            }
        }

        public void ConfigureAnimator(GameObject unitModel = null) {

            if (unitModel != null) {
                this.unitModel = unitModel;
            }

            // most (but not all) units have animators
            // find an animator if one exists and initialize it
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                unitAnimator.Init(animator);

                // this may have been called from a unit which already had a model attached
                // if so, the model is the animator gameobject, since no model will have been passed to this call
                if (unitModel == null) {
                    unitModel = animator.gameObject;
                }
            }

            ConfigureUnitModel();
        }

        public void ConfigureUnitModel() {
            if (unitModel != null) {
                dynamicCharacterAvatar = unitModel.GetComponent<DynamicCharacterAvatar>();
                if (dynamicCharacterAvatar != null) {
                    dynamicCharacterAvatar.Initialize();
                    SubscribeToUMACreate();
                } else {
                    // this is not an UMA model, therefore it is ready and its bone structure is already created
                    SetModelReady();
                }
            }
        }

        public void SetModelReady() {
            //Debug.Log(gameObject.name + "UnitController.SetModelReady()");
            modelReady = true;
            characterUnit.BaseCharacter.HandleCharacterUnitSpawn();
            OnModelReady();
        }

        public void UnsubscribeFromUMACreate() {
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
                //dynamicCharacterAvatar.umaData.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void SubscribeToUMACreate() {

            UMAData umaData = dynamicCharacterAvatar.umaData;
            umaData.OnCharacterCreated += HandleCharacterCreated;
            umaData.OnCharacterBeforeDnaUpdated += HandleCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += HandleCharacterBeforeUpdated;
            umaData.OnCharacterDnaUpdated += HandleCharacterDnaUpdated;
            umaData.OnCharacterDestroyed += HandleCharacterDestroyed;
            umaData.OnCharacterUpdated += HandleCharacterUpdated;
        }

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterCreated(): " + umaData);
            UnsubscribeFromUMACreate();
            SetModelReady();
        }

        public void HandleCharacterBeforeDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.BeforeDnaUpdated(): " + umaData);
        }
        public void HandleCharacterBeforeUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterBeforeUpdated(): " + umaData);
        }
        public void HandleCharacterDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDnaUpdated(): " + umaData);
        }
        public void HandleCharacterDestroyed(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDestroyed(): " + umaData);
        }
        public void HandleCharacterUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterUpdated(): " + umaData + "; frame: " + Time.frameCount);
            //HandleCharacterCreated(umaData);
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

        public void EnableMotor() {
            motorEnabled = true;
        }

        public void DisableMotor() {
            motorEnabled = false;
        }

        public virtual void CancelMountEffects() {
            //Debug.Log(gameObject.name + ".UnitController.CancelMountEffects()");

            if (mounted == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): canCast and character is mounted");

                foreach (StatusEffectNode statusEffectNode in characterUnit.BaseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects");
                    if (statusEffectNode.StatusEffect is MountEffect) {
                        //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects: found a mount effect");
                        statusEffectNode.CancelStatusEffect();
                        break;
                    }
                }
            }

            // update apparent velocity so any spellcast that caused the cancel mount is not interrupted
            LastPosition = transform.position;
        }



        public void FollowTarget(Interactable target, float minAttackRange = -1f) {
            //Debug.Log(gameObject.name + ": AIController.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + ")");
            if (!(currentState is DeathState)) {
                UnitMotor.FollowTarget(target, minAttackRange);
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

        protected override void Update() {
            base.Update();
            if (characterUnit.BaseCharacter.CharacterStats.IsAlive == false) {
                // can't handle movement when dead
                return;
            }

            if (motorEnabled) {
                unitMotor.Update();
            }
            UpdateApparentVelocity();
            if (ApparentVelocity > 0.1f) {
                characterUnit.BaseCharacter.CharacterAbilityManager.HandleManualMovement();
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
            if (currentState != null) {
                currentState.Update();
            }
            if (motorEnabled) {
                unitMotor.FixedUpdate();
            }
        }

        public void SetAggroRange() {
            if (unitComponentController.AggroRangeController != null) {
                //Debug.Log(gameObject.name + ".AIController.Awake(): setting aggro range");
                unitComponentController.AggroRangeController.SetAgroRange(AggroRadius, characterUnit.BaseCharacter);
                unitComponentController.AggroRangeController.StartEnableAggro();
            }
        }

        public bool StartCombatPhase(CombatStrategyNode combatStrategyNode) {
            if (!startedPhaseNodes.Contains(combatStrategyNode)) {
                startedPhaseNodes.Add(combatStrategyNode);
                return true;
            }
            return false;
        }

        public void ApplyControlEffects(BaseCharacter source) {
            //Debug.Log(gameObject.name + ".UnitController.ApplyControlEffects()");
            if (!underControl) {
                underControl = true;
                masterUnit = source;
                // done so pets of player unit wouldn't attempt to attack npcs questgivers etc
                //masterUnit.MyCharacterController.OnSetTarget += SetTarget;
                if (masterUnit == null) {
                    //Debug.Log(gameObject.name + ".AIController.ApplyControlEffects(): masterUnit is null, returning");
                    return;
                }
                masterUnit.UnitController.OnClearTarget += ClearTarget;
                masterUnit.CharacterAbilityManager.OnAttack += OnMasterAttack;
                masterUnit.CharacterCombat.OnDropCombat += OnMasterDropCombat;
                masterUnit.UnitController.OnManualMovement += OnMasterMovement;

                // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
                characterUnit.BaseCharacter.CharacterCombat.AggroTable.ClearTable();
                characterUnit.BaseCharacter.CharacterFactionManager.NotifyOnReputationChange();
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
            Vector3 masterRelativeDestination = masterUnit.UnitController.gameObject.transform.position + masterUnit.UnitController.gameObject.transform.TransformDirection(Vector3.right);
            float usedMaxDistance = 0f;
            if (characterUnit.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                usedMaxDistance = maxCombatDistanceFromMasterOnMove;
            } else {
                usedMaxDistance = maxDistanceFromMasterOnMove;
            }

            if (Vector3.Distance(gameObject.transform.position, masterUnit.UnitController.gameObject.transform.position) > usedMaxDistance && Vector3.Distance(LeashPosition, masterUnit.UnitController.gameObject.transform.position) > usedMaxDistance) {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): setting master relative destination");
                masterRelativeDestination = SetDestination(masterRelativeDestination);
                LeashPosition = masterRelativeDestination;
            } else {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not greater than " + usedMaxDistance);
            }

        }

        public void OnMasterAttack(BaseCharacter target) {
            //Debug.Log(gameObject.name + ".OnMasterAttack()");
            SetTarget(target.UnitController);
        }

        public void OnMasterDropCombat() {
            characterUnit.BaseCharacter.CharacterCombat.TryToDropCombat();
        }

        public void UpdateTarget() {
            //Debug.Log(gameObject.name + ": UpdateTarget()");
            if (characterUnit.BaseCharacter == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): characterUnit.BaseCharacter is null!!!");
                return;
            }
            if (characterUnit.BaseCharacter.CharacterCombat == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): characterUnit.BaseCharacter.MyCharacterCombat is null. (ok for non combat units)");
                return;
            }
            if (characterUnit.BaseCharacter.CharacterCombat.AggroTable == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget(): characterUnit.BaseCharacter.MyCharacterCombat.MyAggroTable is null!!!");
                return;
            }
            AggroNode topNode;
            if (underControl) {
                topNode = masterUnit.CharacterCombat.AggroTable.MyTopAgroNode;
            } else {
                topNode = characterUnit.BaseCharacter.CharacterCombat.AggroTable.MyTopAgroNode;
            }

            if (topNode == null) {
                //Debug.Log(gameObject.name + ": UpdateTarget() and the topnode was null");
                if (Target != null) {
                    ClearTarget();
                }
                if (characterUnit.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                    characterUnit.BaseCharacter.CharacterCombat.TryToDropCombat();
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
                SetTarget(topNode.aggroTarget.Interactable);
                return;
            }
            if (Target != topNode.aggroTarget.Interactable) {
                //Debug.Log(gameObject.name + ".AIController.UpdateTarget(): " + topNode.aggroTarget.gameObject.name + "[" + topNode.aggroValue + "] stole agro from " + MyTarget);
                ClearTarget();
                SetTarget(topNode.aggroTarget.Interactable);
            }
        }

        public Vector3 SetDestination(Vector3 destination) {
            //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position);
            if (!(currentState is DeathState)) {
                CommonMovementNotifier();
                return UnitMotor.MoveToPoint(destination);
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
            if (characterUnit.BaseCharacter != null) {
                characterUnit.BaseCharacter.CharacterStats.ResetResourceAmounts();
                if (UnitMotor != null) {
                    UnitMotor.MovementSpeed = MovementSpeed;
                    UnitMotor.ResetPath();
                } else {
                    //Debug.Log(gameObject.name + ".AIController.Reset(): characterUnit.BaseCharacter.myanimatedunit was null!");
                }
            } else {
                //Debug.Log(gameObject.name + ".AIController.Reset(): characterUnit.BaseCharacter was null!");
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
                return characterUnit.BaseCharacter.CharacterCombat.GetMinAttackRange(MyCombatStrategy.GetAttackRangeAbilityList(characterUnit.BaseCharacter));
            } else {
                // get random attack if no strategy exists
                return characterUnit.BaseCharacter.CharacterCombat.GetMinAttackRange(characterUnit.BaseCharacter.CharacterCombat.GetAttackRangeAbilityList());
            }
        }


        public bool HasMeleeAttack() {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility meleeAbility = MyCombatStrategy.GetMeleeAbility(characterUnit.BaseCharacter);
                if (meleeAbility != null) {
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                BaseAbility validAttackAbility = characterUnit.BaseCharacter.CharacterCombat.GetMeleeAbility();
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
                UnitMotor.UseRootMotion = false;
            }
        }

        private void HandleMovementAudio() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);
            if (unitProfile == null || unitProfile.MovementAudioProfiles == null || unitProfile.MovementAudioProfiles.Count == 0 || unitProfile.PlayOnFootstep == true) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            if (apparentVelocity >= (characterUnit.BaseCharacter.CharacterStats.RunSpeed / 2f)) {
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
            apparentVelocity = Vector3.Distance(transform.position, lastPosition) * (1 / Time.deltaTime);
            lastPosition = transform.position;

        }

        public override void ProcessLevelUnload() {
            //Debug.Log(gameObject.name + ".UnitController.ProcessLevelUnload()");
            base.ProcessLevelUnload();
            ClearTarget();
            CancelMountEffects();
        }

        public void Agro(CharacterUnit agroTarget) {
            //Debug.Log(gameObject.name + ".UnitController.Agro(" + agroTarget.DisplayName + ")");
            // at this level, we are just pulling both parties into combat.

            if (currentState is DeathState) {
                // can't be in combat when dead
                return;
            }

            if (agroTarget == null) {
                //Debug.Log("no character unit on target");
            } else if (agroTarget.BaseCharacter == null) {
                // nothing for now
            } else if (agroTarget.BaseCharacter.CharacterCombat == null) {
                //Debug.Log("no character combat on target");
            } else {
                if (characterUnit.BaseCharacter.CharacterCombat == null) {
                    //Debug.Log("for some strange reason, combat is null????");
                    // like inanimate units
                } else {
                    // moved liveness check into EnterCombat to centralize logic because there are multiple entry points to EnterCombat
                    agroTarget.BaseCharacter.CharacterCombat.EnterCombat(characterUnit.BaseCharacter);
                    characterUnit.BaseCharacter.CharacterCombat.EnterCombat(agroTarget.BaseCharacter);
                }
                //Debug.Log("combat is " + combat.ToString());
                //Debug.Log("mytarget is " + MyTarget.ToString());
            }
        }

        public void FreezeCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.FreezeCharacter(): ");
            frozen = true;
            FreezePositionXZ();
            if (UnitAnimator != null) {
                UnitAnimator.MyAnimator.enabled = false;
            }
            if (UnitMotor != null) {
                UnitMotor.FreezeCharacter();
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
            FreezePositionXZ();
            if (UnitAnimator != null) {
                UnitAnimator.HandleStunned();
            } else {
                //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): characteranimator was null");
            }
            if (UnitMotor != null) {
                UnitMotor.FreezeCharacter();
            } else {
                //Debug.Log(gameObject.name + ".BaseController.StunCharacter(): charactermotor was null");
            }
        }

        public void UnStunCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.UnStunCharacter(): ");
            stunned = false;
            FreezeRotation();
            if (UnitAnimator != null) {
                UnitAnimator.HandleUnStunned();
            }
            if (UnitMotor != null) {
                UnitMotor.UnFreezeCharacter();
            }
        }

        public void LevitateCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.LevitateCharacter(): ");
            levitated = true;
            FreezePositionXZ();
            if (UnitAnimator != null) {
                UnitAnimator.HandleLevitated();
            }
            if (UnitMotor != null) {
                UnitMotor.FreezeCharacter();
            }
        }

        public void UnLevitateCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.UnLevitateCharacter(): ");
            levitated = false;
            FreezeRotation();
            if (UnitAnimator != null) {
                UnitAnimator.HandleUnLevitated();
            }
            if (UnitMotor != null) {
                UnitMotor.UnFreezeCharacter();
            }
        }


        public void SetTarget(Interactable newTarget) {
            //Debug.Log(gameObject.name + ": UnitController: setting target: " + (newTarget == null ? "null" : newTarget.gameObject.name));
            if (unitControllerMode == UnitControllerMode.AI || unitControllerMode == UnitControllerMode.Pet) {
                if (currentState is DeathState || currentState is EvadeState) {
                    return;
                }
                if (Target == null) {
                    target = newTarget;
                }
                //Debug.Log("my target is " + MyTarget.ToString());

                // moved this whole block inside the evade check because it doesn't make sense to agro anything while you are evading
                CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
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
            if (UnitMotor != null) {
                UnitMotor.StopFollowingTarget();
            }
            OnClearTarget();
        }

        private Vector3 GetHitBoxCenter() {
            //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter()");
            if (characterUnit == null) {
                //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): characterUnit.BaseCharacter.MyCharacterUnit is null!");
                return Vector3.zero;
            }
            Vector3 returnValue = myCollider.bounds.center + (transform.forward * (characterUnit.HitBoxSize / 2f));
            //Vector3 returnValue = transform.TransformPoint(myCollider.bounds.center) + (transform.forward * (characterUnit.HitBoxSize / 2f));
            //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter() Capsule Collider Center is:" + characterUnit.BaseCharacter.MyCharacterUnit.transform.TransformPoint(characterUnit.BaseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().center));
            return returnValue;
        }

        public Vector3 GetHitBoxSize() {
            if (characterUnit == null) {
                return Vector3.zero;
            }
            // testing disable size multiplier and just put it straight into the hitbox.  it is messing with character motor because we stop moving toward a character that is 0.5 units outside of the hitbox
            //return new Vector3(characterUnit.BaseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier, characterUnit.BaseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().height * hitBoxSizeMultiplier, characterUnit.BaseCharacter.MyCharacterStats.MyHitBox * hitBoxSizeMultiplier);
            //return new Vector3(characterUnit.BaseCharacter.CharacterUnit.HitBoxSize, characterUnit.BaseCharacter.CharacterUnit.MyCapsuleCollider.bounds.extents.y * 3f, characterUnit.BaseCharacter.CharacterUnit.HitBoxSize);
            return new Vector3(characterUnit.HitBoxSize, myCollider.bounds.extents.y * 3f, characterUnit.HitBoxSize);
        }

        public bool IsTargetInHitBox(Interactable newTarget) {
            //Debug.Log(gameObject.name + ".BaseController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + ")");
            if (newTarget == null) {
                return false;
            }
            Collider[] hitColliders = Physics.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, Quaternion.identity);
            //Debug.Log(gameObject.name + ".BaseController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "); center: " + GetHitBoxCenter() + "; size: " + GetHitBoxSize());
            int i = 0;
            //Check when there is a new collider coming into contact with the box
            while (i < hitColliders.Length) {
                //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "]");
                if (hitColliders[i].gameObject == newTarget.gameObject) {
                    //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "] MATCH!!");
                    return true;
                }
                i++;
            }
            //Debug.Log(gameObject.name + ".BaseController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "): return false");
            return false;
        }

        // leave this function here for debugging hitboxes
        void OnDrawGizmos() {
            if (Application.isPlaying) {
                if (myCollider == null) {
                    return;
                }

                //Debug.Log(gameObject.name + ".BaseController.OnDrawGizmos(): hit box center is :" + GetHitBoxCenter());
                Gizmos.color = Color.red;
                //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
                Gizmos.DrawWireCube(GetHitBoxCenter(), GetHitBoxSize());
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            StopAllCoroutines();
            persistentObjectComponent.Cleanup();
            behaviorController.Cleanup();
            OnUnitDestroy(unitProfile);
        }

        public void CommonMovementNotifier() {
            OnManualMovement();
        }

        public bool CanGetValidAttack(bool beginAttack = false) {

            if (MyCombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility validCombatStrategyAbility = MyCombatStrategy.GetValidAbility(CharacterUnit.BaseCharacter);
                if (validCombatStrategyAbility != null) {
                    characterUnit.BaseCharacter.CharacterAbilityManager.BeginAbility(validCombatStrategyAbility);
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                BaseAbility validAttackAbility = characterUnit.BaseCharacter.CharacterCombat.GetValidAttackAbility();
                if (validAttackAbility != null) {
                    //Debug.Log(gameObject.name + ".AIController.CanGetValidAttack(" + beginAttack + "): Got valid attack ability: " + validAttackAbility.MyName);
                    characterUnit.BaseCharacter.CharacterAbilityManager.BeginAbility(validAttackAbility);
                    return true;
                }
            }

            //Debug.Log(gameObject.name + ".CanGetValidAttack(): return false");
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
            //Debug.Log(gameObject.name + ".UnitController.EnableAgent()");
            if (NavMeshAgent != null && useAgent == true) {
                NavMeshAgent.enabled = true;
            }
        }

        public void DisableAgent() {
            //Debug.Log(gameObject.name + ".UnitController.DisableAgent()");
            if (NavMeshAgent != null) {
                NavMeshAgent.enabled = false;
            }
        }

        public void DeActivateMountedState() {
            unitMountManager.DeActivateMountedState();
        }

        public override void ProcessPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".UnitController.ProcessPlayerUnitSpawn()");
            behaviorController.HandlePlayerUnitSpawn();
            base.ProcessPlayerUnitSpawn();
        }

        #region EventNotifications

        public void NotifyOnReputationChange() {
            OnReputationChange();
        }
        public void NotifyOnBeforeDie(CharacterStats characterStats) {
            OnBeforeDie(characterStats);
        }
        public void NotifyOnReviveComplete() {
            OnReviveComplete();
        }
        public void NotifyOnLevelChanged(int newLevel) {
            OnLevelChanged(newLevel);
        }
        public void NotifyOnUnitTypeChange(UnitType newUnitType, UnitType oldUnitType) {
            OnUnitTypeChange(newUnitType, oldUnitType);
        }
        public void NotifyOnRaceChange(CharacterRace newCharacterRace, CharacterRace oldCharacterRace) {
            OnRaceChange(newCharacterRace, oldCharacterRace);
        }
        public void NotifyOnClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(newCharacterClass, oldCharacterClass);
        }
        public void NotifyOnSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(newClassSpecialization, oldClassSpecialization);
        }
        public void NotifyOnFactionChange(Faction newFaction, Faction oldFaction) {
            OnFactionChange(newFaction, oldFaction);
        }
        public void NotifyOnNameChange(string newName) {
            OnNameChange(newName);
        }
        public void NotifyOnTitleChange(string newTitle) {
            OnTitleChange(newTitle);
        }
        public void NotifyOnResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            OnResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }
        public void NotifyOnStatusEffectAdd(StatusEffectNode statusEffectNode) {
            //Debug.Log(gameObject.name + ".NotifyOnStatusEffectAdd()");
            OnStatusEffectAdd(statusEffectNode);
        }
        public void NotifyOnCastTimeChanged(IAbilityCaster source, BaseAbility baseAbility, float castPercent) {
            OnCastTimeChanged(source, baseAbility, castPercent);
        }
        public void NotifyOnCastStop(BaseCharacter baseCharacter) {
            OnCastStop(baseCharacter);
        }
        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            OnActivateMountedState(mountUnitController);
        }
        public void NotifyOnDeActivateMountedState() {
            OnDeActivateMountedState();
        }

        #endregion

        #region MessagePassthroughs

        public void BeginDialog(string dialogName) {
            dialogController.BeginDialog(dialogName);
        }

        public void BeginPatrol(string patrolName) {
            patrolController.BeginPatrol(patrolName);
        }

        public void BeginAbility(string abilityName) {
            characterUnit.BaseCharacter.AbilityManager.BeginAbility(abilityName);
        }

        public void StopBackgroundMusic() {
            behaviorController.StopBackgroundMusic();
        }

        public void StartBackgroundMusic() {
            behaviorController.StartBackgroundMusic();
        }

        #endregion
    }

    public enum UnitControllerMode { Preview, Player, AI, Mount, Pet, Inanimate };

}