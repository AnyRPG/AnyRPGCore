using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitController : NamePlateUnit, IPersistentObjectOwner {

        public event System.Action<Interactable> OnSetTarget = delegate { };
        public event System.Action<Interactable> OnClearTarget = delegate { };
        public event System.Action OnManualMovement = delegate { };
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
        public event System.Action<BaseCharacter> OnCastComplete = delegate { };
        public event System.Action<BaseCharacter> OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };
        public event System.Action<string> OnMessageFeed = delegate { };
        public override event System.Action OnCameraTargetReady = delegate { };
        //public event System.Action OnDespawn = delegate { };

        // by default, a unit will enter AI mode if no mode is set before Init()
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
        private LootableCharacter lootableCharacter = null;
        private PatrolController patrolController = null;
        private BehaviorController behaviorController = null;
        private UnitModelController unitModelController = null;
        private UnitMountManager unitMountManager = null;
        private UUID uuid = null;

        // control logic
        private IState currentState;
        private List<CombatStrategyNode> startedPhaseNodes = new List<CombatStrategyNode>();

        // targeting
        private Interactable target = null;
        private float distanceToTarget = 0f;
        // keep track of target position to determine of distance check is needed
        private Vector3 lastTargetPosition = Vector3.zero;
        // avoid use of local variables for garbage collection
        private AggroNode topNode = null;


        // track current state
        private bool mounted = false;
        private bool walking = false;
        private bool frozen = false;
        private bool stunned = false;
        private bool levitated = false;
        private bool motorEnabled = true;
        private bool despawning = false;

        // movement parameters
        private bool useAgent = false;
        private Vector3 startPosition = Vector3.zero;
        private float evadeSpeed = 5f;
        private float leashDistance = 40f;
        private float maxDistanceFromMasterOnMove = 3f;
        private float maxCombatDistanceFromMasterOnMove = 15f;

        // movement tracking
        private float apparentVelocity = 0f;
        private Vector3 lastPosition = Vector3.zero;

        // is this unit under the control of a master unit
        private bool underControl = false;
        private BaseCharacter masterUnit = null;

        // rider information
        private UnitController riderUnitController = null;

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
                if (unitControllerMode == UnitControllerMode.Pet) {
                    return 0f;
                }
                if (characterUnit.BaseCharacter != null && unitProfile != null) {
                    return unitProfile.AggroRadius;
                }
                return 20f;
            }
            set {

            }
        }
        public CombatStrategy CombatStrategy {
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
                if (SystemGameManager.Instance.LevelManager.GetActiveSceneNode()?.MovementLoopProfile != null) {
                    return SystemGameManager.Instance.LevelManager.GetActiveSceneNode().MovementLoopProfile;
                }
                if (characterUnit?.BaseCharacter != null && unitProfile?.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
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
                if (SystemGameManager.Instance.LevelManager.GetActiveSceneNode()?.MovementHitProfile != null) {
                    return SystemGameManager.Instance.LevelManager.GetActiveSceneNode().MovementHitProfile;
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
        public IUUID UUID {
            get {
                if (unitProfile != null && unitProfile.OverwriteUnitUUID) {
                    return unitProfile;
                }
                return uuid;
            }
        }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }
        public UnitProfile UnitProfile { get => unitProfile; }
        public override NamePlateProps NamePlateProps {
            get {
                if (unitProfile?.UnitPrefabProps != null) {
                    return unitProfile.UnitPrefabProps.NamePlateProps;
                }
                return namePlateProps;
            }
        }

        public UnitMountManager UnitMountManager { get => unitMountManager; set => unitMountManager = value; }
        public BehaviorController BehaviorController { get => behaviorController; set => behaviorController = value; }

        public override GameObject InteractableGameObject {
            get {
                // allow collider checks to consider this collider to be the collider of the rider when this unit is the mount
                // this allows things like the mount hittin a portal or entering an enemy agro range to trigger the player interaction
                if (unitControllerMode == UnitControllerMode.Mount && riderUnitController != null) {
                    return riderUnitController.gameObject;
                }
                // allow collider checks to consider this collider to be the collider of the mount when the unit is mounted
                // this allows things like projectile effects and hitbox collider checks to aim for the active collider on the mount
                // instead of the inactive one on the rider
                if (mounted && unitMountManager.MountUnitController != null) {
                    return unitMountManager.MountUnitController.gameObject;
                }
                return base.InteractableGameObject;
            }
        }

        public override Interactable InteractableTarget {
            get {
                // allow collider checks to consider this collider to be the collider of the rider when this unit is the mount
                // this allows things like the mount hittin a portal or entering an enemy agro range to trigger the player interaction
                if (unitControllerMode == UnitControllerMode.Mount && riderUnitController != null) {
                    return riderUnitController;
                }
                // allow collider checks to consider this collider to be the collider of the mount when the unit is mounted
                // this allows things like projectile effects and hitbox collider checks to aim for the active collider on the mount
                // instead of the inactive one on the rider
                if (mounted && unitMountManager.MountUnitController != null) {
                    return unitMountManager.MountUnitController;
                }
                return base.InteractableTarget;
            }
        }

        public override float InteractionMaxRange {
            get {
                if (unitProfile != null) {
                    return unitProfile.InteractionMaxRange;
                }
                return base.InteractionMaxRange;
            }
        }

        public override bool CameraTargetReady {
            get {
                return unitModelController.ModelReady;
            }
        }

        public override bool NonCombatOptionsAvailable {
            get {
                if (characterUnit.BaseCharacter.CharacterStats.IsAlive == false) {
                    return false;
                }
                return base.NonCombatOptionsAvailable;
            }
        }


        public override bool CombatOnly {
            get {
                if (unitControllerMode == UnitControllerMode.Player) {
                    return true;
                }
                return base.CombatOnly;
            }
        }

        public bool UseAgent { get => useAgent; }
        public MovementSoundArea MovementSoundArea { get => movementSoundArea; set => movementSoundArea = value; }
        public UnitModelController UnitModelController { get => unitModelController; }

        public override void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized == true) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);

        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
        }

        public void HandleReputationChange(string eventName, EventParamProperties eventParamProperties) {
            // minimap indicator can change color if reputation changed
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }
            characterUnit.CallMiniMapStatusUpdateHandler();
        }

        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            characterUnit.BaseCharacter.CharacterPetManager.DespawnAllPets();
            unitMountManager.SetMountedState(mountUnitController, mountUnitProfile);
        }

        public void SetRider(UnitController riderUnitController) {
            this.riderUnitController = riderUnitController;
        }

        public override void EnableInteraction() {
            // do nothing intentionally, don't want collider disabled or unit will fall through world
            //base.EnableInteraction();
        }

        public override void DisableInteraction() {
            // do nothing intentionally, don't want collider disabled or unit will fall through world
            //base.DisableInteraction();
        }

        /// <summary>
        /// set this unit to be a stationary preview
        /// </summary>
        private void SetPreviewMode() {
            //Debug.Log(gameObject.name + ".UnitController.SetPreviewMode()");
            SetUnitControllerMode(UnitControllerMode.Preview);
            unitModelController.SetDefaultLayer(SystemGameManager.Instance.SystemConfigurationManager.DefaultCharacterUnitLayer);
            useAgent = false;
            DisableAgent();

            // prevent preview unit from moving around
            if (rigidBody != null) {
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                rigidBody.useGravity = false;
            }

            MonoBehaviour[] monoBehaviours = GetComponents<MonoBehaviour>();

            // disable third party components
            foreach (MonoBehaviour monoBehaviour in monoBehaviours) {
                bool safe = false;
                if ((monoBehaviour as BaseCharacter) is BaseCharacter) {
                    safe = true;
                }
                if ((monoBehaviour as UnitController) is UnitController) {
                    safe = true;
                }
                if (unitModelController.KeepMonoBehaviorEnabled(monoBehaviour)) {
                    safe = true;
                }
                if (!safe) {
                    monoBehaviour.enabled = false;
                }
            }
        }

        /// <summary>
        /// set this unit to be the pet of characterUnit.BaseCharacter
        /// </summary>
        /// <param name="characterUnit.BaseCharacter"></param>
        public void SetPetMode(BaseCharacter masterBaseCharacter, bool enableMode = false) {
            //Debug.Log(gameObject.name + ".UnitController.SetPetMode(" + (masterBaseCharacter == null ? "null" : masterBaseCharacter.gameObject.name) + ")");
            SetUnitControllerMode(UnitControllerMode.Pet);
            unitModelController.SetDefaultLayer(SystemGameManager.Instance.SystemConfigurationManager.DefaultCharacterUnitLayer);
            if (masterBaseCharacter != null) {
                characterUnit.BaseCharacter.CharacterStats.SetLevel(masterBaseCharacter.CharacterStats.Level);
                //characterUnit.BaseCharacter.CharacterStats.ApplyControlEffects(masterBaseCharacter);
                ApplyControlEffects(masterBaseCharacter);
                if (enableMode == true) {
                    ChangeState(new IdleState());
                    SetAggroRange();
                }
                SetMasterRelativeDestination(true);
                startPosition = LeashPosition;
            }
        }

        private void EnablePetMode() {
            //Debug.Log(gameObject.name + ".UnitController.EnablePetMode()");
            InitializeNamePlateController();
            EnableAICommon();

            // it is necessary to keep track of leash position because it was already set as destination by setting pet mode
            // enabling idle state will reset the destination so we need to re-enable it
            //Vector3 leashDestination = LeashPosition;
            ChangeState(new IdleState());
            SetAggroRange();
            SetDestination(LeashPosition);
        }

        /// <summary>
        /// set this unit to be a mount
        /// </summary>
        private void SetMountMode() {
            //Debug.Log(gameObject.name + ".UnitController.SetMountMode()");

            // mount namePlates do not need full initialization, only the position to be set
            namePlateController.SetNamePlatePosition();

            SetUnitControllerMode(UnitControllerMode.Mount);
            unitModelController.SetDefaultLayer(SystemGameManager.Instance.SystemConfigurationManager.DefaultCharacterUnitLayer);
            if (myCollider != null) {
                myCollider.isTrigger = false;
            }
            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
            useAgent = false;
            DisableAgent();
        }

        /// <summary>
        /// set this unit to be a player
        /// </summary>
        private void EnablePlayer() {
            //Debug.Log(gameObject.name + "UnitController.EnablePlayer()");
            InitializeNamePlateController();

            unitModelController.SetDefaultLayer(SystemGameManager.Instance.SystemConfigurationManager.DefaultPlayerUnitLayer);
            DisableAggro();

            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            myCollider.isTrigger = false;

            unitComponentController.InteractableRange.gameObject.SetActive(false);
            useAgent = false;
            DisableAgent();

            // this code is a quick way to set speed on third party controllers when the player spawns
            if (characterUnit.BaseCharacter.CharacterStats != null) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = characterUnit.BaseCharacter.CharacterStats.RunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);

                eventParam.simpleParams.FloatParam = characterUnit.BaseCharacter.CharacterStats.SprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);

            }
            if (SystemGameManager.Instance.SystemConfigurationManager.UseThirdPartyMovementControl) {
                SystemGameManager.Instance.KeyBindManager.SendKeyBindEvents();
            }
        }

        /// <summary>
        /// set this unit to be an AI unit
        /// </summary>
        private void EnableAI() {
            //Debug.Log(gameObject.name + ".UnitController.EnableAI()");
            InitializeNamePlateController();
            EnableAICommon();

            if (characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.SpawnDead == true) {
                ChangeState(new DeathState());
            } else {
                ChangeState(new IdleState());
            }
            SetAggroRange();
        }

        private void EnableAICommon() {
            unitModelController.SetDefaultLayer(SystemGameManager.Instance.SystemConfigurationManager.DefaultCharacterUnitLayer);

            // enable agent needs to be done before changing state or idle -> patrol transition will not work because of an inactive navmeshagent
            if (unitProfile != null && unitProfile.IsMobile == true) {
                useAgent = true;
            }
            EnableAgent();

            // ensure player cannot physically push AI units around
            // first set collision mode to avoid unity errors about dynamic detection not supported for kinematic rigidbodies
            rigidBody.useGravity = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rigidBody.isKinematic = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            // ensure player is not physically blocked or pushed around by AI units
            myCollider.isTrigger = true;
        }

        public void ConfigurePlayer() {
            //Debug.Log(gameObject.name + ".UnitController.ConfigurePlayer()");
            SystemGameManager.Instance.PlayerManager.SetUnitController(this);
        }

        public void SetUnitControllerMode(UnitControllerMode unitControllerMode) {
            //Debug.Log(gameObject.name + ".UnitController.SetUnitControllerMode(" + unitControllerMode + ")");
            this.unitControllerMode = unitControllerMode;
            if (unitControllerMode == UnitControllerMode.Player) {
                ConfigurePlayer();
            }
        }

        public void ActivateUnitControllerMode() {
            //Debug.Log(gameObject.name + ".UnitController.ActivateUnitControllerMode() to " + unitControllerMode);
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

        protected override void OnEnable() {
            //Debug.Log(gameObject.name + ".UnitController.OnEnable()");
            base.OnEnable();

            // create components here instead?  which ones rely on other things like unit profile being set before start?
            namePlateController = new UnitNamePlateController(this);
            unitMotor = new UnitMotor(this);
            unitAnimator = new UnitAnimator(this);
            patrolController = new PatrolController(this);
            behaviorController = new BehaviorController(this);
            unitModelController = new UnitModelController(this);
            unitMountManager = new UnitMountManager(this);
            persistentObjectComponent.Setup(this);

            // allow the base character to initialize.
            characterUnit.BaseCharacter.Init();
        }

        public override void ProcessInit() {
            //Debug.Log(gameObject.name + ".UnitController.ProcessInit()");
            if (characterUnit.BaseCharacter.UnitProfile == null
                && characterUnit.BaseCharacter.UnitProfileName != null
                && characterUnit.BaseCharacter.UnitProfileName != string.Empty) {
                SetUnitProfile(UnitProfile.GetUnitProfileReference(characterUnit.BaseCharacter.UnitProfileName), UnitControllerMode.AI);
            }

            ConfigureAnimator();

            base.ProcessInit();

            persistentObjectComponent.Init();

            SetStartPosition();

            ActivateUnitControllerMode();

            behaviorController.Init();
            patrolController.Init();

        }

        public override void InitializeNamePlateController() {
            //Debug.Log(gameObject.name + ".UnitController.InitializeNamePlateController()");
            // mounts and preview units shouldn't have a namePlateController active
            if (unitControllerMode != UnitControllerMode.Mount && unitControllerMode != UnitControllerMode.Preview) {
                base.InitializeNamePlateController();
            }
        }

        public void Despawn(float delayTime = 0f) {
            if (delayTime == 0f) {
                DespawnImmediate();
                return;
            }
            StartCoroutine(DespawnDelay(delayTime));
        }

        private IEnumerator DespawnDelay(float delayTime) {
            yield return new WaitForSeconds(delayTime);
            DespawnImmediate();
        }

        private void DespawnImmediate() {
            //Debug.Log(gameObject.name + ".UnitController.DespawnImmediate()");
            despawning = true;
            StopAllCoroutines();
            RemoveControlEffects();
            ProcessPointerExit();

            // give the equipment manager a chance to remove the equipment models
            characterUnit?.BaseCharacter?.HandleCharacterUnitDespawn();

            // now that the model is unequipped, return the model to the pool
            unitModelController.DespawnModel();

            persistentObjectComponent.Cleanup();
            if (behaviorController != null) {
                behaviorController.Cleanup();
            }
            OnUnitDestroy(unitProfile);
            ResetSettings();
            ObjectPooler.Instance.ReturnObjectToPool(gameObject);
        }

        /// <summary>
        /// reset all variables to default values for object pooling
        /// </summary>
        public override void ResetSettings() {
            Debug.Log(gameObject.name + ".UnitController.ResetSettings()");
            
            // agents should be disabled so when pool and re-activated they don't throw errors if they are a preview unit
            DisableAgent();

            // cleanup unit model type specific settings
            unitModelController.ResetSettings();

            unitProfile = null;

            // components
            agent = null;
            rigidBody = null;
            unitMotor = null;
            unitAnimator = null;
            lootableCharacter = null;
            patrolController = null;
            behaviorController = null;
            unitModelController = null;
            unitMountManager = null;
            uuid = null;

            currentState = null;
            target = null;
            distanceToTarget = 0f;
            lastTargetPosition = Vector3.zero;
            topNode = null;

            mounted = false;
            walking = false;
            frozen = false;
            stunned = false;
            levitated = false;
            motorEnabled = true;
            despawning = false;

            useAgent = false;
            startPosition = Vector3.zero;
            evadeSpeed = 5f;
            leashDistance = 40f;
            maxDistanceFromMasterOnMove = 3f;
            maxCombatDistanceFromMasterOnMove = 15f;

            apparentVelocity = 0f;
            lastPosition = Vector3.zero;
            underControl = false;
            masterUnit = null;
            riderUnitController = null;
            movementSoundArea = null;

            base.ResetSettings();
        }

        private void ProcessPointerExit() {
            if (isMouseOverUnit == true) {
                isMouseOverUnit = false;
                OnMouseOut();
            }
        }

        // for interactions
        public override float PerformFactionCheck(BaseCharacter sourceCharacter) {
            return Faction.RelationWith(sourceCharacter, characterUnit.BaseCharacter);
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

                // now that the characterUnit is available
                unitComponentController?.HighlightController?.ConfigureOwner(characterUnit);
                AddInteractable(characterUnit);
            }

            base.GetComponentReferences();

            uuid = GetComponent<UUID>();
            lootableCharacter = GetComponent<LootableCharacter>();
            agent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// transfer any persistence settings from the unit profile to the persistent object component
        /// </summary>
        private void SetPersistenceProperties() {
            if (UnitProfile.PersistObjectPosition == true) {
                persistentObjectComponent.PersistObjectPosition = true;
            }
            if (UnitProfile.SaveOnGameSave == true) {
                persistentObjectComponent.SaveOnGameSave = true;
            }
            if (UnitProfile.SaveOnLevelUnload == true) {
                persistentObjectComponent.SaveOnLevelUnload = true;
            }
        }

        /// <summary>
        /// This method is meant to be called after OnEnable() and before Init()
        /// </summary>
        /// <param name="unitProfile"></param>
        public void SetUnitProfile(UnitProfile unitProfile, UnitControllerMode unitControllerMode, int unitLevel = -1) {
            //Debug.Log(gameObject.name + "UnitController.SetUnitProfile()");
            this.unitProfile = unitProfile;

            SetPersistenceProperties();

            if (characterUnit?.BaseCharacter != null) {
                characterUnit.BaseCharacter.SetUnitProfile(unitProfile, true, unitLevel, (unitControllerMode == UnitControllerMode.Player ? false : true));
            }
            SetUnitControllerMode(unitControllerMode);

            // testing - not necessary here since it was actually not doing anything and code is now in Init that used to be in Start which requires the model spawned
            //Init();

            unitModelController.SpawnUnitModel();

            SetUnitProfileInteractables();
        }

        private void SetUnitProfileInteractables() {
            //Debug.Log(gameObject.name + "UnitController.SetUnitProfileInteractables()");

            if (unitProfile == null) {
                return;
            }

            // mounts, pets, players, and preview units should not create interaction options or subscribe to prerequisite updates
            if (unitControllerMode != UnitControllerMode.AI) {
                return;
            }

            // built-in interactable options
            if (unitProfile.LootableCharacterProps.AutomaticCurrency == true || unitProfile.LootableCharacterProps.LootTableNames.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.LootableCharacterProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                //interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.DialogProps.DialogList.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.DialogProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                //interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.QuestGiverProps.Quests.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.QuestGiverProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                //interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.VendorProps.VendorCollections.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.VendorProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                //interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            if (unitProfile.BehaviorProps.BehaviorNames.Count > 0) {
                InteractableOptionComponent interactableOptionComponent = unitProfile.BehaviorProps.GetInteractableOption(this);
                interactables.Add(interactableOptionComponent);
                //interactableOptionComponent.HandlePrerequisiteUpdates();
            }

            // named interactable options
            foreach (InteractableOptionConfig interactableOption in unitProfile.InteractableOptionConfigs) {
                if (interactableOption.InteractableOptionProps != null) {
                    InteractableOptionComponent interactableOptionComponent = interactableOption.InteractableOptionProps.GetInteractableOption(this);
                    interactables.Add(interactableOptionComponent);
                    //interactableOptionComponent.HandlePrerequisiteUpdates();
                }
            }

            // this will cause the minimap to be instantiated so it should be done after all interactables are added so the layers can be created properly
            foreach (InteractableOptionComponent interactableOptionComponent in interactables) {
                interactableOptionComponent.HandlePrerequisiteUpdates();
            }
        }

        private void SetStartPosition() {
            //Debug.Log(gameObject.name + ".UnitController.SetStartPosition()");
            // pets have their start position set by master
            if (unitControllerMode != UnitControllerMode.Pet) {
                Vector3 correctedPosition = transform.position;
                if (unitMotor != null) {
                    correctedPosition = unitMotor.CorrectedNavmeshPosition(transform.position);
                }
                MyStartPosition = correctedPosition;
            }

            // prevent apparent velocity on first update by setting lastposition to currentposition
            lastPosition = transform.position;

        }

        public void ConfigureAnimator() {
            //Debug.Log(gameObject.name + ".UnitController.ConfigureAnimator(" + (newUnitModel == null ? "null" : newUnitModel.name) + ")");

            // most (but not all) units have animators
            // find an animator if one exists and initialize it
            Animator animator = GetComponentInChildren<Animator>();

            // if the unit was not spawned, the unit model will be the animator
            // references to the dynamicCharacterAvatar must exist before the unit animator is initialized
            // they are needed for the animator to properly set the override controller on the avatar
            unitModelController.FindUnitModel(animator);

            if (animator != null) {
                unitAnimator.Init(animator);
            }

            unitModelController.ConfigureUnitModel();
        }

        public void SetModelReady() {
            //Debug.Log(gameObject.name + ".UnitController.SetModelReady()");
            OnCameraTargetReady();
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
            //Debug.Log(gameObject.name + ".AIController.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + ")");
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

        protected override void Update() {
            base.Update();
            if (characterUnit?.BaseCharacter?.CharacterStats?.IsAlive == false) {
                // can't handle movement when dead
                return;
            }

            if (motorEnabled) {
                unitMotor?.Update();
            }
            UpdateApparentVelocity();
            if (ApparentVelocity > 0.1f) {
                characterUnit?.BaseCharacter?.CharacterAbilityManager?.HandleManualMovement();
            }
            HandleMovementAudio();
        }

        public void FixedUpdate() {
            if (target != null) {
                // prevent distance calculation if no movement has occured
                if (transform.position != lastPosition || target.transform.position != lastTargetPosition) {
                    distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
                }
                lastTargetPosition = target.transform.position;
            }
            lastPosition = transform.position;
            if (ControlLocked) {
                // can't allow any action if we are stunned/frozen/etc
                //Debug.Log(gameObject.name + ".AIController.FixedUpdate(): controlLocked: " + MyControlLocked);
                return;
            }
            if (currentState != null) {
                currentState.Update();
            }
            if (motorEnabled) {
                unitMotor?.FixedUpdate();
            }
        }

        public void SetAggroRange() {
            if (unitComponentController.AggroRangeController != null) {
                unitComponentController.AggroRangeController.SetAgroRange(AggroRadius, characterUnit.BaseCharacter);
                if (!(currentState is DeathState)) {
                    unitComponentController.AggroRangeController.StartEnableAggro();
                }
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
                masterUnit.UnitController.OnClearTarget += HandleClearTarget;
                masterUnit.CharacterAbilityManager.OnAttack += HandleMasterAttack;
                masterUnit.CharacterCombat.OnDropCombat += HandleMasterDropCombat;
                masterUnit.UnitController.OnManualMovement += HandleMasterMovement;

                // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
                characterUnit.BaseCharacter.CharacterCombat.AggroTable.ClearTable();
                characterUnit.BaseCharacter.CharacterFactionManager.NotifyOnReputationChange();
            } else {
                //Debug.Log("Can only be under the control of one master at a time");
            }
        }

        public void RemoveControlEffects() {
            if (underControl && masterUnit != null) {
                //masterUnit.MyCharacterController.OnSetTarget -= SetTarget;
                masterUnit.UnitController.OnClearTarget -= HandleClearTarget;
                masterUnit.CharacterAbilityManager.OnAttack -= HandleMasterAttack;
                masterUnit.CharacterCombat.OnDropCombat -= HandleMasterDropCombat;
                masterUnit.UnitController.OnManualMovement -= HandleMasterMovement;
            }
            masterUnit = null;
            underControl = false;

            // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
            characterUnit?.BaseCharacter?.CharacterCombat?.AggroTable?.ClearTable();

            // nothing past this point needs to happen if the unit is despawning
            if (despawning) {
                return;
            }
            characterUnit?.BaseCharacter?.CharacterFactionManager?.NotifyOnReputationChange();

            // should we reset leash position to start position here ?
        }

        public void HandleMasterMovement() {
            //Debug.Log(gameObject.name + ".AIController.OnMasterMovement()");
            SetMasterRelativeDestination();
        }

        public void SetMasterRelativeDestination(bool forceUpdate = false) {
            //Debug.Log(gameObject.name + ".UnitController.SetMasterRelativeDestination(" + forceUpdate + ")");
            if (UnderControl == false) {
                // only do this stuff if we actually have a master
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not under control");
                return;
            }
            //Debug.Log(gameObject.name + ".UnitController.SetMasterRelativeDestination()");

            // stand to the right of master by one meter
            Vector3 masterRelativeDestination = masterUnit.UnitController.InteractableGameObject.transform.position + masterUnit.UnitController.InteractableGameObject.transform.TransformDirection(Vector3.right);
            float usedMaxDistance = 0f;
            if (characterUnit.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                usedMaxDistance = maxCombatDistanceFromMasterOnMove;
            } else {
                usedMaxDistance = maxDistanceFromMasterOnMove;
            }

            if (forceUpdate
                || (Vector3.Distance(gameObject.transform.position, masterUnit.UnitController.gameObject.transform.position) > usedMaxDistance
                && Vector3.Distance(LeashPosition, masterUnit.UnitController.gameObject.transform.position) > usedMaxDistance)) {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): setting master relative destination");
                masterRelativeDestination = SetDestination(masterRelativeDestination);
                LeashPosition = masterRelativeDestination;
            } else {
                //Debug.Log(gameObject.name + ".AIController.SetMasterRelativeDestination(): not greater than " + usedMaxDistance);
            }

        }

        public void HandleMasterAttack(BaseCharacter target) {
            //Debug.Log(gameObject.name + ".OnMasterAttack()");
            SetTarget(target.UnitController);
        }

        public void HandleMasterDropCombat() {
            characterUnit.BaseCharacter.CharacterCombat.TryToDropCombat();
            SetMasterRelativeDestination(true);
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
            topNode = null;
            if (underControl) {
                topNode = masterUnit.CharacterCombat.AggroTable.TopAgroNode;
            } else {
                topNode = characterUnit.BaseCharacter.CharacterCombat.AggroTable.TopAgroNode;
            }
            if (topNode == null) {
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
            //Debug.Log(gameObject.name + ".UnitController.SetDestination(" + destination + "). current location: " + transform.position);
            if ((currentState is DeathState) == false) {
                //if ((currentState is DeathState) == false && characterUnit?.BaseCharacter?.CharacterStats?.IsReviving == false) {
                CommonMovementNotifier();
                return UnitMotor.MoveToPoint(destination);
            } else {
                //Debug.Log(gameObject.name + ": aicontroller.SetDestination(" + destination + "). current location: " + transform.position + ". WE ARE DEAD, DOING NOTHING");
            }
            return transform.position;
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

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                return characterUnit.BaseCharacter.CharacterCombat.GetMinAttackRange(CombatStrategy.GetAttackRangeAbilityList(characterUnit.BaseCharacter));
            } else {
                // get random attack if no strategy exists
                return characterUnit.BaseCharacter.CharacterCombat.GetMinAttackRange(characterUnit.BaseCharacter.CharacterCombat.GetAttackRangeAbilityList());
            }
        }


        public bool HasMeleeAttack() {

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility meleeAbility = CombatStrategy.GetMeleeAbility(characterUnit.BaseCharacter);
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

            if (CombatStrategy != null) {
                if (CombatStrategy.HasMusic() == true) {
                    //Debug.Log(gameObject.name + ".AIController.ResetCombat(): attempting to turn off fight music");
                    AudioProfile musicProfile = SystemGameManager.Instance.LevelManager.GetActiveSceneNode().BackgroundMusicProfile;
                    if (musicProfile != null) {
                        //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): music profile was set");
                        if (musicProfile.AudioClip != null && SystemGameManager.Instance.AudioManager.MusicAudioSource.clip != musicProfile.AudioClip) {
                            //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): playing default music");
                            SystemGameManager.Instance.AudioManager.PlayMusic(musicProfile.AudioClip);
                        }
                    } else {
                        // There was no music, turn it off instead
                        SystemGameManager.Instance.AudioManager.StopMusic();
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

        /// <summary>
        /// play or stop movement loop
        /// </summary>
        private void HandleMovementAudio() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);

            // if this unit has no configured audio, or is set to use footstep events and is not in a movement area with no footstep events do nothing
            if (unitProfile?.MovementAudioProfiles == null
                || unitProfile.MovementAudioProfiles.Count == 0
                || (unitProfile.PlayOnFootstep == true && (movementSoundArea == null || (movementSoundArea.MovementHitProfile != null && movementSoundArea.MovementLoopProfile == null)))
                || (unitProfile.PlayOnFootstep == false && movementSoundArea != null && (movementSoundArea.MovementHitProfile != null && movementSoundArea.MovementLoopProfile == null))
                ) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            // note : this will not work for third paty controllers without these parameters.  Third party controllers should be setup to use footstep hit audio
            if (unitAnimator != null
                && UnitAnimator.IsInAir() == false
                && mounted == false
                && ControlLocked == false
                //&& (apparentVelocity >= (characterUnit.BaseCharacter.CharacterStats.RunSpeed / 2f))) {
                && unitAnimator.GetBool("Moving") == true) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
                if (!unitComponentController.MovementIsPlaying()) {
                    PlayMovement(MovementLoopProfile.AudioClip, true);
                    //unitComponentController.PlayMovement(MovementLoopProfile.AudioClip, true);
                }
            } else {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): not up to run speed");
                if (unitComponentController?.MovementIsPlaying() == true) {
                    unitComponentController.StopMovement();
                }
            }
        }

        public void PlayMovement(AudioClip audioClip, bool loop) {
            unitComponentController.PlayMovement(audioClip, loop);
        }

        /// <summary>
        /// reset velocity calculation so that casting in the same frame as the unit stops will not be cancelled
        /// </summary>
        public void ResetApparentVelocity() {
            lastPosition = transform.position;
            apparentVelocity = 0f;
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
            // moved to baseCharacter
            //Despawn();
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

        private void ApplyControlLock() {
            if (characterUnit?.BaseCharacter?.CharacterAbilityManager != null) {
                characterUnit.BaseCharacter.CharacterAbilityManager.StopCasting();
            }
        }

        public void FreezeCharacter() {
            //Debug.Log(gameObject.name + ".BaseController.FreezeCharacter(): ");
            ApplyControlLock();
            frozen = true;
            FreezePositionXZ();
            if (UnitAnimator != null) {
                UnitAnimator.Animator.enabled = false;
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
                UnitAnimator.Animator.enabled = true;
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
            ApplyControlLock();
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
            ApplyControlLock();
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
                if (target != null) {
                    ClearTarget();
                }
                target = newTarget;
            }
            OnSetTarget(target);
            target.OnInteractableDisable += HandleTargetDisable;
        }

        public void HandleTargetDisable() {
            ClearTarget();
        }

        // receive messages from master and pass them on
        public void HandleClearTarget(Interactable oldTarget) {
            ClearTarget();
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ": basecontroller.ClearTarget()");
            if (target != null) {
                target.OnInteractableDisable -= HandleTargetDisable;
            }
            Interactable oldTarget = target;
            target = null;
            // FIX ME (reenable possibly?)
            if (UnitMotor != null) {
                UnitMotor.StopFollowingTarget();
            }
            OnClearTarget(oldTarget);
        }

        private Vector3 GetHitBoxCenter() {
            //Debug.Log(gameObject.name + ".BaseController.GetHitBoxCenter()");
            if (characterUnit == null) {
                //Debug.Log(gameObject.name + "BaseController.GetHitBoxCenter(): characterUnit.BaseCharacter.MyCharacterUnit is null!");
                return myCollider.bounds.center;
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
            //Debug.Log(gameObject.name + ".UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + ")");
            if (newTarget == null) {
                return false;
            }
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int interactableMask = 1 << LayerMask.NameToLayer("Interactable");
            int triggerMask = 1 << LayerMask.NameToLayer("Triggers");
            int validMask = (playerMask | characterMask | interactableMask | triggerMask);

            //Collider[] hitColliders = Physics.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, Quaternion.identity, validMask);
            Collider[] hitColliders = Physics.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, transform.rotation, validMask);
            //Debug.Log(gameObject.name + ".UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "); center: " + hitBoxCenter.x + " " + hitBoxCenter.y + " " + hitBoxCenter.z + "; size: " + GetHitBoxSize() + "; navEnabled: " + agent.enabled);
            int i = 0;
            //Check when there is a new collider coming into contact with the box
            while (i < hitColliders.Length) {
                //Debug.Log(gameObject.name + ".UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "); center: " + GetHitBoxCenter() + "; size: " + GetHitBoxSize() + "Hit : " + hitColliders[i].gameObject.name + "[" + i + "]");

                if (hitColliders[i].gameObject == newTarget.InteractableGameObject) {
                    //Debug.Log(gameObject.name + ".UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "): Hit : " + hitColliders[i].gameObject.name + "[" + i + "] return true");
                    return true;
                }
                i++;
            }
            //Debug.Log(gameObject.name + ".UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "): return false");
            return false;
        }

        // leave this function here for debugging hitboxes
        void OnDrawGizmos() {
            if (Application.isPlaying) {
                if (myCollider == null) {
                    return;
                }

                Gizmos.color = Color.red;
                //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
                Gizmos.color = new Color(1, 0, 0);
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(transform.InverseTransformPoint(GetHitBoxCenter()), GetHitBoxSize());
            }
        }

        public void CommonMovementNotifier() {
            if ((currentState is DeathState) == true || characterUnit?.BaseCharacter?.CharacterStats?.IsReviving == true) {
                return;
            }
            OnManualMovement();
        }

        public bool CanGetValidAttack(bool beginAttack = false) {

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                BaseAbility validCombatStrategyAbility = CombatStrategy.GetValidAbility(CharacterUnit.BaseCharacter);
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
            //Debug.Log(gameObject.name + ".UnitController.FreezeRotation()");
            RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        /// <summary>
        /// if this unit is configured to use the agent, enable it
        /// </summary>
        public void EnableAgent() {
            //Debug.Log(gameObject.name + ".UnitController.EnableAgent()");
            if (NavMeshAgent != null && useAgent == true && NavMeshAgent.enabled == false) {
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

        public override void UpdateMiniMapIndicator() {
            if (unitControllerMode != UnitControllerMode.Player) {
                return;
            }
            base.UpdateMiniMapIndicator();
            if (miniMapIndicatorReady == true) {
                miniMapManager.UpdateIndicatorRotation(this);
                /*
                if (mainMapIndicator != null) {
                    mainMapIndicator.transform.rotation = miniMapIndicator.transform.rotation;
                }
                */
            }
        }

        public override void UpdateMainMapIndicator() {
            if (unitControllerMode != UnitControllerMode.Player) {
                return;
            }
            base.UpdateMainMapIndicator();
            if (miniMapIndicatorReady == true) {
                mainMapManager.UpdateIndicatorRotation(this);
            }
        }

        public override void ProcessPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".UnitController.ProcessPlayerUnitSpawn()");

            // players do not need to react to their own spawn, and previews should never react
            // now players do because they need their minimap to show up
            //if (unitControllerMode == UnitControllerMode.Player || unitControllerMode == UnitControllerMode.Preview) {
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }

            behaviorController.HandlePlayerUnitSpawn();
            base.ProcessPlayerUnitSpawn();
        }

        public void HandleMovementSpeedUpdate() {
            if (UnitMotor != null) {
                UnitMotor.MovementSpeed = MovementSpeed;
            }
        }

        #region EventNotifications

        public void NotifyOnReputationChange() {
            // minimap indicator can change color if reputation changed
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }
            characterUnit.CallMiniMapStatusUpdateHandler();
            OnReputationChange();
            unitComponentController.HighlightController.UpdateColors();
        }

        public void NotifyOnBeforeDie(CharacterStats characterStats) {
            unitComponentController.StopMovement();
            unitComponentController.HighlightController.UpdateColors();
            OnBeforeDie(characterStats);

        }

        public void NotifyOnAfterDie(CharacterStats characterStats) {
            if (GetCurrentInteractables().Count == 0) {
                RevertMaterialChange();
            }
        }

        public void NotifyOnReviveComplete() {
            FreezeRotation();
            InitializeNamePlate();
            CharacterUnit.HandleReviveComplete();
            unitComponentController.HighlightController.UpdateColors();
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
        public void NotifyOnCastComplete(BaseCharacter baseCharacter) {
            OnCastComplete(baseCharacter);
        }
        public void NotifyOnCastCancel(BaseCharacter baseCharacter) {
            OnCastCancel(baseCharacter);
        }
        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            OnActivateMountedState(mountUnitController);
        }
        public void NotifyOnDeActivateMountedState() {
            OnDeActivateMountedState();
        }
        public void NotifyOnMessageFeed(string message) {
            //Debug.Log(gameObject.name + ".NotifyOnMessageFeed(" + message + ")");
            OnMessageFeed(message);
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
            //Debug.Log(gameObject.name + ".UnitController.BeginAbility()");
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