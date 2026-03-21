using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitController : NamePlateUnit, IPersistentObjectOwner, IAbilityCaster {


        public override event System.Action OnCameraTargetReady = delegate { };
        //public event System.Action OnDespawn = delegate { };

        [Header("Unit Controller")]

        // by default, a unit will enter AI mode if no mode is set before Init()
        [SerializeField]
        private UnitControllerMode unitControllerMode = UnitControllerMode.AI;

        /*
        [Tooltip("If true, this unit will turn to face any target that interacts with it")]
        [SerializeField]
        private bool faceInteractionTarget = true;
        */

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

        // unique instance identifier
        private int characterId = -1;

        // components
        private UnitEventController unitEventController = new UnitEventController();
        private NavMeshAgent agent = null;
        private Rigidbody rigidBody = null;
        private UnitMotor unitMotor = null;
        private UnitAnimator unitAnimator = null;
        private LootableCharacterComponent lootableCharacter = null;
        private PatrolController patrolController = null;
        private BehaviorController behaviorController = null;
        private UnitModelController unitModelController = null;
        private UnitMountManager unitMountManager = null;
        private UnitMaterialController unitMaterialController = null;
        private UnitVoiceController unitVoiceController = null;
        private UnitActionManager unitActionManager = null;
        private UUID uuid = null;
        private BaseCharacter baseCharacter = null;
        private CharacterCombat characterCombat = null;
        private CharacterAbilityManager characterAbilityManager = null;
        private CharacterSkillManager characterSkillManager = null;
        private CharacterPetManager characterPetManager = null;
        private CharacterFactionManager characterFactionManager = null;
        private CharacterEquipmentManager characterEquipmentManager = null;
        private CharacterGroupManager characterGroupManager = null;
        private CharacterGuildManager characterGuildManager = null;
        private CharacterStats characterStats = null;
        private CharacterCurrencyManager characterCurrencyManager = null;
        private CharacterRecipeManager characterRecipeManager = null;
        private CharacterCraftingManager characterCraftingManager = null;
        private CharacterInventoryManager characterInventoryManager = null;
        private CharacterQuestLog characterQuestLog = null;
        private CharacterSaveManager characterSaveManager = null;
        private CharacterActionBarManager characterActionBarManager = null;
        private CharacterDialogManager characterDialogManager = null;

        // spawn state
        private bool isStateReset = false;

        // control logic
        private IState currentState;
        private List<CombatStrategyNode> startedPhaseNodes = new List<CombatStrategyNode>();
        private bool aggroEnabled = false;
        private UnitMovementController unitMovementController = null;
        private uint offlineTickCounter = 0;

        // targeting
        private List<Interactable> inRangeInteractables = new List<Interactable>();
        private Interactable target = null;
        private float distanceToTarget = 0f;
        // keep track of target position to determine of distance check is needed
        private Vector3 lastTargetPosition = Vector3.zero;
        // avoid use of local variables for garbage collection
        private AggroNode topNode = null;

        // capabilities
        private bool canFly = false;
        private bool canFlyOverride = false;
        private bool canGlide = false;
        private bool canGlideOverride = false;

        // track current state
        private bool isMounted = false;
        private bool walking = false;
        private bool frozen = false;
        private bool stunned = false;
        private bool levitated = false;
        private bool motorEnabled = true;
        private bool despawning = false;
        private bool inWater = false;
        private bool swimming = false;
        private bool flying = false;
        private bool isStealth = false;

        private List<WaterBody> currentWater = new List<WaterBody>();

        private Coroutine despawnCoroutine = null;

        // unit configuration
        private float floatHeight = 1.5f;

        // movement parameters
        private bool useAgent = false;
        private Vector3 startPosition = Vector3.zero;
        private float evadeSpeed = 5f;
        private float leashDistance = 40f;
        private float maxDistanceFromMasterOnMove = 3f;
        private float maxCombatDistanceFromMasterOnMove = 15f;
        private bool enableLeashing = false;

        // track the current movement sound overrides
        private List<MovementSoundArea> movementSoundAreas = new List<MovementSoundArea>();
        private MovementSoundArea movementSoundArea = null;

        // movement tracking
        private float apparentVelocity = 0f;
        private float lastApparentVelocity = 0f;
        private Vector3 lastPosition = Vector3.zero;
        private Vector3 lastFrozenPosition = Vector3.zero;
        private FootstepType footstepType = FootstepType.Environment;
        private AudioProfile environmentFootstepAudioProfile = null;
        private AudioProfile unitFootstepAudioProfile = null;
        private int unitStepIndex = 0;
        private int environmentStepIndex = 0;

        // is this unit under the control of a master unit
        private bool underControl = false;
        private UnitController masterUnit = null;

        // rider information
        private UnitController riderUnitController = null;

        // network state
        private bool isOwner = false;
        private bool isServerOwned = false;
        private bool isDisconnected = false;

        // initial configuration
        private CharacterRequestData characterRequestData = null;
        bool characterConfigured = false;

        // game manager references
        protected LevelManagerClient levelManagerClient = null;
        protected KeyBindManager keyBindManager = null;
        protected AudioManager audioManager = null;
        protected CharacterManager characterManager = null;
        protected SystemAchievementManager systemAchievementManager = null;
        protected SceneUtilityService sceneUtilityService = null;
        protected InteractionManagerServer interactionManagerServer = null;


        //public INamePlateTarget NamePlateTarget { get => namePlateTarget; set => namePlateTarget = value; }
        public NavMeshAgent NavMeshAgent { get => agent; set => agent = value; }
        public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }
        public UnitMotor UnitMotor { get => unitMotor; set => unitMotor = value; }
        public UnitAnimator UnitAnimator { get => unitAnimator; set => unitAnimator = value; }
        public Vector3 StartPosition {
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
                if (UnderControl == true && MasterUnit != null) {
                    return MasterUnit.MovementSpeed;
                }
                return (walking == false ? characterStats.RunSpeed : characterStats.WalkSpeed);
            }
        }
        public float SwimSpeed {
            get {
                if (UnderControl == true && MasterUnit != null) {
                    return MasterUnit.SwimSpeed;
                }
                return characterStats.SwimSpeed;
            }
        }
        public float FlySpeed {
            get {
                if (UnderControl == true && MasterUnit != null) {
                    return MasterUnit.FlySpeed;
                }
                return characterStats.FlySpeed;
            }
        }
        public float GlideSpeed {
            get {
                if (UnderControl == true && MasterUnit != null) {
                    return MasterUnit.GlideSpeed;
                }
                return characterStats.GlideSpeed;
            }
        }

        public float GlideFallSpeed {
            get {
                if (UnderControl == true && MasterUnit != null) {
                    return MasterUnit.GlideFallSpeed;
                }
                return characterStats.GlideFallSpeed;
            }
        }

        public bool UnderControl { get => underControl; set => underControl = value; }
        public UnitController MasterUnit { get => masterUnit; set => masterUnit = value; }
        public bool Frozen { get => frozen; }
        public bool Stunned { get => stunned; set => stunned = value; }
        public bool Levitated { get => levitated; set => levitated = value; }
        public bool ControlLocked {
            get {
                //Debug.Log($"{gameObject.name}.UnitController.MyControlLocked: frozen: " + MyFrozen + "; stunned: "  + MyStunned + "; mylevitated: " + MyLevitated);
                return (Frozen || Stunned || Levitated);
            }
        }
        //public Vector3 LastPosition { get => lastPosition; set => lastPosition = value; }
        public float ApparentVelocity { get => apparentVelocity; set => apparentVelocity = value; }
        public float AggroRadius {
            get {
                if (unitControllerMode == UnitControllerMode.Pet) {
                    return 0f;
                }
                if (unitProfile != null) {
                    return unitProfile.AggroRadius;
                }
                return 20f;
            }
            set {

            }
        }
        public CombatStrategy CombatStrategy {
            get {
                if (unitProfile != null) {
                    return unitProfile.CombatStrategy;
                }
                return null;
            }
        }
        public UnitControllerMode UnitControllerMode {
            get => unitControllerMode;
        }
        public LootableCharacterComponent LootableCharacter { get => lootableCharacter; set => lootableCharacter = value; }
        public bool Walking { get => walking; set => walking = value; }
        public AudioProfile MovementLoopProfile {
            get {
                if (unitProfile != null && unitProfile.FootstepType == FootstepType.None) {
                    return null;
                }
                if (unitProfile == null || unitProfile.FootstepType == FootstepType.Environment || unitProfile.FootstepType == FootstepType.UnitFallback) {
                    if (movementSoundArea != null && movementSoundArea.MovementLoopProfile != null) {
                        return movementSoundArea.MovementLoopProfile;
                    }
                    if (levelManagerClient.GetActiveSceneNode()?.MovementLoopProfile != null) {
                        return levelManagerClient.GetActiveSceneNode().MovementLoopProfile;
                    }
                }
                if (unitProfile != null && (unitProfile.FootstepType == FootstepType.Unit || unitProfile.FootstepType == FootstepType.UnitFallback)) {
                    if (unitProfile?.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
                        return unitProfile.MovementAudioProfiles[0];
                    }
                }
                return null;
            }
        }

        /*
        public AudioProfile MovementHitProfile {
            get {
                // movement sound areas override everything
                if (movementSoundArea != null && movementSoundArea.MovementHitProfile != null) {
                    //Debug.Log($"{gameObject.name}.CharacterUnit.GetMovementHitProfile: return movementSoundArea.MovementHitProfile");
                    return movementSoundArea.MovementHitProfile;
                }

                // try the terrain layer based movement profile of the active scene node
                if (levelManager.GetTerrainFootStepProfile(transform.position) != null) {
                    return levelManager.GetTerrainFootStepProfile(transform.position);
                }

                // try the default footstep profile of the active scene node
                if (levelManager.GetActiveSceneNode()?.MovementHitProfile != null) {
                    return levelManager.GetActiveSceneNode().MovementHitProfile;
                }

                // default to the character movement audio profile
                if (characterUnit.BaseCharacter != null && unitProfile != null && unitProfile.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
                    return unitProfile.MovementAudioProfiles[0];
                }
                return null;
            }
        }
        */

        public List<string> PatrolNames { get => patrolNames; set => patrolNames = value; }
        public bool IsMounted { get => isMounted; set => isMounted = value; }
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
        public UnitMaterialController UnitMaterialController { get => unitMaterialController; set => unitMaterialController = value; }
        public UnitEventController UnitEventController { get => unitEventController; }
        public UnitActionManager UnitActionManager { get => unitActionManager; set => unitActionManager = value; }
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
                if (isMounted && unitMountManager.MountUnitController != null) {
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
                if (isMounted && unitMountManager.MountUnitController != null) {
                    return unitMountManager.MountUnitController;
                }
                return base.InteractableTarget;
            }
        }

        public override Interactable CharacterTarget {
            get {
                // allow collider checks to consider this collider to be the collider of the rider when this unit is the mount
                // this allows things like the mount hittin a portal or entering an enemy agro range to trigger the player interaction
                if (unitControllerMode == UnitControllerMode.Mount && riderUnitController != null) {
                    return riderUnitController;
                }
                return base.CharacterTarget;
            }
        }

        public override Interactable PhysicalTarget {
            get {
                // allow collider checks to consider this collider to be the collider of the mount when the unit is mounted
                // this allows things like projectile effects and hitbox collider checks to aim for the active collider on the mount
                // instead of the inactive one on the rider
                if (isMounted && unitMountManager.MountUnitController != null) {
                    return unitMountManager.MountUnitController;
                }
                return base.InteractableTarget;
            }
        }

        public Transform CameraTransform {
            get {
                if (unitModelController?.UnitModel != null) {
                    return unitModelController.UnitModel.transform;
                }
                return transform;
            }
        }


        public override float InteractionMaxRange {
            get {
                //Debug.Log($"{gameObject.name}.UnitController.InteractionMaxRange: unitProfile.InteractionMaxRange: {(unitProfile != null ? unitProfile.InteractionMaxRange.ToString() : "null")} base.InteractionMaxRange: {base.InteractionMaxRange}");
                if (unitProfile != null) {
                    return unitProfile.InteractionMaxRange;
                }
                return base.InteractionMaxRange;
            }
        }

        public override bool CameraTargetReady {
            get {
                return unitModelController.ModelCreated && unitModelController.IsBuilding() == false;
            }
        }

        public override bool NonCombatOptionsAvailable {
            get {
                if (characterStats.IsAlive == false) {
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
        // unitControllers should always override the interactable range collider size to match the unit profile settings, so return true here to enable that functionality in the base class
        public override bool OverrideInteractionColliderSize => true;

        public bool UseAgent { get => useAgent; }
        public MovementSoundArea MovementSoundArea { get => movementSoundArea; set => movementSoundArea = value; }
        public UnitModelController UnitModelController { get => unitModelController; }
        public bool InWater { get => inWater; }
        public List<WaterBody> CurrentWater { get => currentWater; set => currentWater = value; }
        public float FloatHeight { get => floatHeight; set => floatHeight = value; }
        //public bool Swimming { get => swimming; }
        //public bool Flying { get => flying; }
        public bool CanFly { get => (canFly || canFlyOverride); set => canFly = value; }
        public bool CanGlide { get => (canGlide || canGlideOverride); set => canGlide = value; }
        public bool CanFlyOverride { get => canFlyOverride; set => canFlyOverride = value; }
        public bool CanGlideOverride { get => canGlideOverride; set => canGlideOverride = value; }
        public BaseCharacter BaseCharacter { get => baseCharacter; }
        public CharacterStats CharacterStats { get => characterStats; }
        public CharacterCombat CharacterCombat { get => characterCombat; }
        public CharacterAbilityManager CharacterAbilityManager { get => characterAbilityManager; }
        public IAbilityManager AbilityManager { get => characterAbilityManager; }
        public MonoBehaviour MonoBehaviour { get => this; }
        public CharacterSkillManager CharacterSkillManager { get => characterSkillManager; }
        public CharacterFactionManager CharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }
        public CharacterGroupManager CharacterGroupManager { get => characterGroupManager; set => characterGroupManager = value; }
        public CharacterGuildManager CharacterGuildManager { get => characterGuildManager; set => characterGuildManager = value; }
        public CharacterInventoryManager CharacterInventoryManager { get => characterInventoryManager; }
        public CharacterQuestLog CharacterQuestLog { get => characterQuestLog; }
        public CharacterPetManager CharacterPetManager { get => characterPetManager; set => characterPetManager = value; }
        public CharacterRecipeManager CharacterRecipeManager { get => characterRecipeManager; set => characterRecipeManager = value; }
        public CharacterCraftingManager CharacterCraftingManager { get => characterCraftingManager; set => characterCraftingManager = value; }
        public CharacterCurrencyManager CharacterCurrencyManager { get => characterCurrencyManager; set => characterCurrencyManager = value; }
        public CharacterSaveManager CharacterSaveManager { get => characterSaveManager; }
        public CharacterDialogManager CharacterDialogManager { get => characterDialogManager; }
        public bool IsOwner { get => isOwner; set => isOwner = value; }
        public bool IsServerOwned { get => isServerOwned; set => isServerOwned = value; }
        public CharacterRequestData CharacterRequestData { get => characterRequestData; }
        public CharacterActionBarManager CharacterActionBarManager { get => characterActionBarManager; }
        public bool CharacterConfigured { get => characterConfigured; }
        public bool EnableLeashing { get => enableLeashing; set => enableLeashing = value; }
        public UnitController RiderUnitController { get => riderUnitController; set => riderUnitController = value; }
        public bool IsDisconnected { get => isDisconnected; set => isDisconnected = value; }
        public bool IsStealth { get => isStealth; set => isStealth = value; }
        public int CharacterId { get => characterId; set => characterId = value; }
        public bool AggroEnabled { get => aggroEnabled; }
        public UnitMovementController UnitMovementController { get => unitMovementController; set => unitMovementController = value; }

        public override void AutoConfigure(SystemGameManager systemGameManager) {
            // don't do anything here.  Unitcontrollers should never be autoconfigured
            //base.AutoConfigure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}.UnitController.Configure() scene: {gameObject.scene.name} default physics scene: {(gameObject.scene.GetPhysicsScene() == Physics.defaultPhysicsScene)}");

            base.Configure(systemGameManager);
            isStateReset = false;
            // create components here instead?  which ones rely on other things like unit profile being set before start?
            unitEventController.Configure(this, systemGameManager);
            namePlateController = new UnitNamePlateController(this, systemGameManager);
            unitMotor = new UnitMotor(this, systemGameManager);
            unitAnimator = new UnitAnimator(this, systemGameManager);
            patrolController = new PatrolController(this, systemGameManager);
            behaviorController = new BehaviorController(this, systemGameManager);
            unitModelController = new UnitModelController(this, systemGameManager);
            
            unitVoiceController = new UnitVoiceController(this, systemGameManager);
            unitMountManager = new UnitMountManager(this, systemGameManager);
            unitActionManager = new UnitActionManager(this, systemGameManager);
            persistentObjectComponent.Setup(this, systemGameManager);

            baseCharacter = new BaseCharacter(this, systemGameManager);
            characterStats = new CharacterStats(this, systemGameManager);
            characterInventoryManager = new CharacterInventoryManager(this, systemGameManager);
            characterEquipmentManager = new CharacterEquipmentManager(this, systemGameManager);
            characterFactionManager = new CharacterFactionManager(this, systemGameManager);
            characterGroupManager = new CharacterGroupManager(this, systemGameManager);
            characterGuildManager = new CharacterGuildManager(this, systemGameManager);
            characterPetManager = new CharacterPetManager(this, systemGameManager);
            characterCombat = new CharacterCombat(this, systemGameManager);
            characterSkillManager = new CharacterSkillManager(this, systemGameManager);
            characterCurrencyManager = new CharacterCurrencyManager(this, systemGameManager);
            characterRecipeManager = new CharacterRecipeManager(this, systemGameManager);
            characterCraftingManager = new CharacterCraftingManager(this, systemGameManager);
            characterAbilityManager = new CharacterAbilityManager(this, systemGameManager);
            characterQuestLog = new CharacterQuestLog(this, systemGameManager);
            characterSaveManager = new CharacterSaveManager(this, systemGameManager);
            characterActionBarManager = new CharacterActionBarManager(this, systemGameManager);
            characterDialogManager = new CharacterDialogManager(this, systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManagerClient = systemGameManager.LevelManagerClient;
            keyBindManager = systemGameManager.KeyBindManager;
            audioManager = systemGameManager.AudioManager;
            characterManager = systemGameManager.CharacterManager;
            systemAchievementManager = systemGameManager.SystemAchievementManager;
            sceneUtilityService = systemGameManager.SceneUtilityService;
            interactionManagerServer = systemGameManager.InteractionManagerServer;
        }

        public void SetCharacterRequestData(CharacterRequestData characterRequestData) {
            //Debug.Log($"{gameObject.name}.UnitController.SetCharacterRequestData() characterId: {characterRequestData.characterId}");

            this.characterRequestData = characterRequestData;
            this.characterId = characterRequestData.characterId;
        }

        protected override void CreateMaterialController() {
            // intentionally not calling base
            unitMaterialController = new UnitMaterialController(this, systemGameManager);
            objectMaterialController = unitMaterialController;
        }

        public override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();
            systemEventManager.OnReputationChange += HandleReputationChange;
            //systemEventManager.OnLevelLoad += HandleLevelLoad;
        }

        public override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnReputationChange -= HandleReputationChange;
            //systemEventManager.OnLevelLoad -= HandleLevelLoad;
        }

        public override void ProcessInit() {
            //Debug.Log($"{gameObject.name}.UnitController.ProcessInit()");

            ConfigureAnimator();

            base.ProcessInit();

            persistentObjectComponent.Init();

            SetStartPosition();

            ActivateUnitControllerMode();

            behaviorController.Init();
            patrolController.Init();
        }

        protected override void PostInit() {
            base.PostInit();
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && levelManagerClient.IsCutscene() == false) {
                // if this is a client in a network game, don't enable the collider because the server will handle it
                return;
            }
            // interactable range did not pick this unit up when it spawned because it wasn't initalized yet
            // so force it to trigger interactable ranges now that initialization is complete
            DisableCollider();
            EnableCollider();
        }

        protected override void CheckEnableInteractableRange() {
            // do nothing here, unit controller will handle enabling and disabling the interactable range based on the unit controller mode
        }

        public override void InitializeNamePlateController() {
            //Debug.Log($"{gameObject.name}.UnitController.InitializeNamePlateController()");
            // mounts and preview units shouldn't have a namePlateController active
            if (unitControllerMode != UnitControllerMode.Mount && unitControllerMode != UnitControllerMode.Preview) {
                base.InitializeNamePlateController();
            }
        }

        public override void ConfigureUnitFrame(UnitFramePanel unitFramePanelBase, bool previewCameraExists) {
            //Debug.Log($"{gameObject.name}.UnitController.ConfigureUnitFrame()");

            if (unitProfile != null && (unitProfile.UnitPrefabProps.NamePlateProps.UseSnapShot == false || previewCameraExists == false)) {
                unitFramePanelBase.ConfigurePortrait(unitProfile.Icon);
                return;
            }

            base.ConfigureUnitFrame(unitFramePanelBase, previewCameraExists);
        }

        public override void ConfigureDialogPanel(DialogPanel dialogPanelController) {

            if (unitProfile != null && unitProfile.UnitPrefabProps.NamePlateProps.UseSnapShot == false) {
                dialogPanelController.ConfigurePortrait(unitProfile.Icon);
                return;
            }

            base.ConfigureDialogPanel(dialogPanelController);
        }

        private void SetUnitFootstepAudioProfile() {

            if (unitProfile?.MovementAudioProfiles != null && unitProfile.MovementAudioProfiles.Count > 0) {
                unitFootstepAudioProfile = unitProfile.MovementAudioProfiles[0];
            }
        }

        private void SetEnvironmentFootStepAudioProfile() {

            if (footstepType == FootstepType.None
                || unitProfile.FootstepType == FootstepType.Unit) {
                // footstep type is none or unit only.  nothing to do.
                return;
            }

            environmentFootstepAudioProfile = null;

            // movement sound areas override everything
            if (movementSoundArea != null && movementSoundArea.MovementHitProfile != null) {
                //Debug.Log($"{gameObject.name}.CharacterUnit.GetMovementHitProfile: return movementSoundArea.MovementHitProfile");
                environmentFootstepAudioProfile = movementSoundArea.MovementHitProfile;
                return;
            }

            // try the terrain layer based movement profile of the active scene node
            environmentFootstepAudioProfile = levelManagerClient.GetTerrainFootStepProfile(transform.position);
            if (environmentFootstepAudioProfile != null) {
                return;
            }

            // try the default footstep profile of the active scene node
            environmentFootstepAudioProfile = levelManagerClient.GetActiveSceneNode()?.MovementHitProfile;
            if (environmentFootstepAudioProfile != null) {
                return;
            }

        }

        public override void InteractWithPlayer(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitController.InteractWithPlayer({factionValue})");

            float factionValue = PerformFactionCheck(sourceUnitController);
            if (factionValue >= 0f
                && characterStats.IsAlive == true
                && unitControllerMode == UnitControllerMode.AI) {

                if (unitProfile != null && unitProfile.FaceInteractionTarget == true) {
                    unitMotor.FaceTarget(sourceUnitController);
                }
            }
        }

        public override void ProcessStartInteract() {
            base.ProcessStartInteract();
            unitEventController.NotifyOnStartInteract();
        }

        public override void ProcessStopInteract() {
            base.ProcessStopInteract();
            unitEventController.NotifyOnStopInteract();
        }

        public override void ProcessStartInteractWithOption(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            base.ProcessStartInteractWithOption(interactableOptionComponent, componentIndex, choiceIndex);
            unitEventController.NotifyOnStartInteractWithOption(interactableOptionComponent, componentIndex, choiceIndex);
        }

        public override void ProcessStopInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            base.ProcessStopInteractWithOption(interactableOptionComponent);
            unitEventController.NotifyOnStopInteractWithOption(interactableOptionComponent);
        }

        public void HandleReputationChange(UnitController targetUnitController) {
            // minimap indicator can change color if reputation changed
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }
            characterUnit.CallMiniMapStatusUpdateHandler();
        }

        /*
        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            characterStats.ProcessLevelLoad();
        }
        */

        /*
        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            characterPetManager.DespawnAllPets();
            unitMountManager.SetMountedState(mountUnitController, mountUnitProfile);
        }
        */

        public void SetRider(UnitController riderUnitController) {
            //Debug.Log($"{gameObject.name}.UnitController.SetRider({(riderUnitController == null ? "null" : riderUnitController.gameObject.name)})");

            this.riderUnitController = riderUnitController;
            characterStats.CalculateRunSpeed();
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
            //Debug.Log($"{gameObject.name}.UnitController.SetPreviewMode()");
            SetUnitControllerMode(UnitControllerMode.Preview);
            unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultPreviewUnitLayer);
            useAgent = false;
            DisableAgent();

            // prevent preview unit from moving around
            if (rigidBody != null) {
                rigidBody.interpolation = RigidbodyInterpolation.None;
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                rigidBody.useGravity = false;
            }

            MonoBehaviour[] monoBehaviours = GetComponents<MonoBehaviour>();

            // disable third party components
            foreach (MonoBehaviour monoBehaviour in monoBehaviours) {
                bool safe = false;
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
        public void SetPetMode(UnitController masterUnitController) {
            //Debug.Log($"{gameObject.name}.UnitController.SetPetMode({(masterUnitController == null ? "null" : masterUnitController.gameObject.name)})");

            SetUnitControllerMode(UnitControllerMode.Pet);
            unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultCharacterUnitLayer);
            if (masterUnitController != null) {
                ApplyControlEffects(masterUnitController);
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene()) {
                    characterStats.SetLevelInternal(masterUnitController.CharacterStats.Level);
                    ChangeState(new IdleState());
                    SetAggroRange();
                    SetMasterRelativeDestination(true);
                    startPosition = LeashPosition;
                } else {
                    // network client pets should have no state
                    if (currentState != null) {
                        currentState.Exit();
                    }
                }
            }
        }

        private void EnablePetMode() {
            //Debug.Log($"{gameObject.name}.UnitController.EnablePetMode()");

            InitializeNamePlateController();
            EnableAICommon();

            // it is necessary to keep track of leash position because it was already set as destination by setting pet mode
            // enabling idle state will reset the destination so we need to re-enable it
            //Vector3 leashDestination = LeashPosition;

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene()) {
                ChangeState(new IdleState());
                SetAggroRange();
                SetDestination(LeashPosition);
            }
        }

        /// <summary>
        /// set this unit to be a mount
        /// </summary>
        private void SetMountMode() {
            //Debug.Log($"{gameObject.name}.UnitController.SetMountMode()");

            // mount namePlates do not need full initialization, only the position to be set
            namePlateController.SetNameplatePosition();

            SetUnitControllerMode(UnitControllerMode.Mount);
            unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultCharacterUnitLayer);
            if (myCollider != null) {
                myCollider.isTrigger = false;
            }
            rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || (systemGameManager.GameMode == GameMode.Network && isOwner == true)) {
                //Debug.Log($"{gameObject.name}.UnitController.SetMountMode() set kinematic false");
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
                FreezePositionXZ();
            } else {
                // gravity and physics should not be applied on non authoritative clients.  They are moved by networkTransform
                //Debug.Log($"{gameObject.name}.UnitController.SetMountMode() set kinematic true");
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
                FreezeAll();
            }

            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (systemConfigurationManager.AllowClickToMove == true && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true)) {
                useAgent = true;
            } else {
                // agents are only used in single player or on the server
                useAgent = false;
            }
            DisableAgent();
            unitMovementController.Init();
        }

        /// <summary>
        /// set this unit to be a player
        /// </summary>
        private void EnablePlayer() {
            //Debug.Log($"{gameObject.name}.UnitController.EnablePlayer()");

            InitializeNamePlateController();

            if (systemGameManager.GameMode == GameMode.Local || (networkManagerServer.ServerModeActive == false && isOwner == true)) {
                // to allow the player to click on objects through their model, the player unit on authoritative clients
                // needs to be on the player layer
                unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultPlayerUnitLayer);
            } else {
                unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultCharacterUnitLayer);
            }
            DisableAggro();

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || (systemGameManager.GameMode == GameMode.Network && isOwner == true)) {
                //Debug.Log($"{gameObject.name}.UnitController.EnablePlayer() set kinematic false");
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
                FreezePositionXZ();
            } else {
                // gravity and physics should not be applied on non authoritative clients.  They are moved by networkTransform
                //Debug.Log($"{gameObject.name}.UnitController.EnablePlayer() set kinematic true");
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
                FreezeAll();
            }
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            if (systemGameManager.GameMode == GameMode.Local) {
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            } else {
                // do not interpolate in network mode or it will interfere with client side prediction and predictionRigidbody
                rigidBody.interpolation = RigidbodyInterpolation.None;
            }


            myCollider.isTrigger = false;

            if (systemConfigurationManager.AllowClickToMove == true && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true)) {
                useAgent = true;
            } else {
                // agents are only used in single player or on the server
                useAgent = false;
            }
            agent.avoidancePriority = 0;
            DisableAgent();
            unitMovementController.Init();
        }

        /// <summary>
        /// set this unit to be an AI unit
        /// </summary>
        private void EnableAI() {
            //Debug.Log($"{gameObject.name}.UnitController.EnableAI()");

            EnableInteractableRange();
            InitializeNamePlateController();
            EnableAICommon();

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene()) {
                if (systemConfigurationManager.EnableLeashing == true) {
                    enableLeashing = true;
                }
                if (characterStats.IsAlive == false) {
                    ChangeState(new DeathState());
                } else {
                    ChangeState(new IdleState());
                }
                SetAggroRange();
            }
        }

        private void EnableAICommon() {
            unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultCharacterUnitLayer);

            // enable agent needs to be done before changing state or idle -> patrol transition will not work because of an inactive navmeshagent
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene()) {
                if (unitProfile != null && unitProfile.IsMobile == true) {
                    useAgent = true;
                }
                EnableAgent();
            }

            rigidBody.interpolation = RigidbodyInterpolation.None;
            if (unitProfile != null && unitProfile.IsMobile == true) {
                // ensure player cannot physically push AI units around
                // first set collision mode to avoid unity errors about dynamic detection not supported for kinematic rigidbodies
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.isKinematic = true;
                rigidBody.useGravity = true;
                rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            } else if (unitProfile != null && unitProfile.IsMobile == false) {
                rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidBody.interpolation = RigidbodyInterpolation.None;
                //rigidBody.detectCollisions = false;
                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            }

            // ensure player is not physically blocked or pushed around by AI units
            myCollider.isTrigger = true;
        }

        /*
        public void ConfigurePlayer() {
            //Debug.Log($"{gameObject.name}.UnitController.ConfigurePlayer()");
            playerManager.SetUnitController(this);

            // setting default layer here in case layer is wrong during buildModelAppearance calls that happen later in initialization
            // disabled for now, testing new method that checks for renderers before changing layer and allowing it to happen in EnablePlayer()
            //unitModelController.SetDefaultLayer(systemConfigurationManager.DefaultPlayerUnitLayer);
        }
        */

        public void SetUnitControllerMode(UnitControllerMode unitControllerMode) {
            //Debug.Log($"{gameObject.name}.UnitController.SetUnitControllerMode({unitControllerMode})");

            this.unitControllerMode = unitControllerMode;
            if (unitControllerMode == UnitControllerMode.Player || unitControllerMode == UnitControllerMode.Mount) {
                unitMovementController = new UnitMovementController(this, systemGameManager);
                SceneData sceneData = levelManagerServer.GetSceneData(gameObject.scene);
                if (sceneData != null && sceneData.HasNavMesh) {
                    unitMovementController.useMeshNav = true;
                } else {
                    unitMovementController.useMeshNav = false;
                }
            }
        }

        public void ActivateUnitControllerMode() {
            //Debug.Log($"{gameObject.name}.UnitController.ActivateUnitControllerMode() to {unitControllerMode}");

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

        public void TryToDespawn() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.TryToDespawn()");

            if (UnitProfile != null && unitProfile.PreventAutoDespawn == true) {
                return;
            }
            if (lootableCharacter != null) {
                // lootable character handles its own despawn logic
                return;
            }

            Despawn(0, true, false);
        }


        public void CancelDespawnDelay() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.CancelDespawnDelay()");
            if (despawnCoroutine != null) {
                StopCoroutine(despawnCoroutine);
                despawnCoroutine = null;
            }
        }

        public void Despawn(float delayTime = 0f, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log($"{gameObject.name}.UnitController.Despawn({delayTime}, {addSystemDefaultTime}, {forceDespawn}) {GetInstanceID()}");

            // if an error happens and the model request is never complete, intialized will be false
            // therefore this is commented out to allow the despawn to complete so references to this character
            // can be cleaned up properly.
            
            if (isStateReset == true) {
                return;
            }
            

            if (forceDespawn == true) {
                DespawnImmediate();
                return;
            }

            if (despawnCoroutine == null && gameObject.activeSelf == true && isActiveAndEnabled) {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ") starting despawn coroutine");
                despawnCoroutine = StartCoroutine(DespawnDelay(delayTime, addSystemDefaultTime));
            }
        }

        private IEnumerator DespawnDelay(float delayTime, bool addSystemDefaultTime) {
            //Debug.Log($"{gameObject.name}.UnitController.DespawnDelay({delayTime}, {addSystemDefaultTime})");

            // add all possible delays together
            float extraTime = 0f;
            if (addSystemDefaultTime) {
                extraTime = systemConfigurationManager.DefaultDespawnTimer;
            }
            float totalDelay = delayTime + extraTime;
            while (totalDelay > 0f) {
                yield return null;
                totalDelay -= Time.deltaTime;
            }

            despawnCoroutine = null;
            if (characterStats.IsAlive == false && characterStats.IsReviving == false) {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): despawning");
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
                DespawnImmediate();
            }
        }

        private void DespawnImmediate() {
            //Debug.Log($"{gameObject.name}.UnitController.DespawnImmediate()");
            
            if (isStateReset == true) {
                return;
            }

            despawning = true;

            // clear target before notify despawn or nothing will be subscribed clearTarget event
            ClearTarget();

            // cancel mount effects before notify because the notify will clear the PlayerManager.UnitController if this is a player
            CancelMountEffects();

            unitEventController.NotifyOnDespawn(this);

            // this could be a mount which has no base character - check for nulls
            characterStats.HandleCharacterUnitDespawn();
            characterAbilityManager.HandleCharacterUnitDespawn();
            characterCombat.HandleCharacterUnitDespawn();
            characterPetManager.HandleCharacterUnitDespawn();
            characterQuestLog.HandleCharacterUnitDespawn();

            unitActionManager.HandleCharacterUnitDespawn();


            StopAllCoroutines();
            RemoveControlEffects();
            ProcessPointerExit();

            // now that the model is unequipped, return the model to the pool
            unitModelController.DespawnModel();

            persistentObjectComponent.Cleanup();
            if (behaviorController != null) {
                behaviorController.Cleanup();
            }
            UnitEventController.NotifyOnUnitDestroy(unitProfile);
            ResetSettings();
            characterManager.PoolUnitController(this);
            IsDisconnected = false;
        }

        /// <summary>
        /// reset all variables to default values for object pooling
        /// </summary>
        public override void ResetSettings() {
            //Debug.Log($"{gameObject.name}.UnitController.ResetSettings()");

            // agents should be disabled so when pool and re-activated they don't throw errors if they are a preview unit
            DisableAgent();

            // cleanup unit model type specific settings
            unitModelController.ResetSettings();

            unitAnimator.ResetSettings();
            unitVoiceController.ResetSettings();

            unitProfile = null;

            // components
            unitEventController = new UnitEventController();
            agent = null;
            rigidBody = null;
            unitMotor = null;
            unitAnimator = null;
            lootableCharacter = null;
            patrolController = null;
            behaviorController = null;
            unitModelController = null;
            unitActionManager = null;
            unitMountManager = null;
            unitVoiceController = null;
            unitMovementController = null;

            uuid = null;

            baseCharacter = null;
            characterCombat = null;
            characterAbilityManager = null;
            characterSkillManager = null;
            characterPetManager = null;
            characterFactionManager = null;
            characterEquipmentManager = null;
            characterGroupManager = null;
            characterGuildManager = null;
            characterStats = null;
            characterCurrencyManager = null;
            characterRecipeManager = null;
            characterCraftingManager = null;
            characterSaveManager = null;
            characterActionBarManager = null;
            characterQuestLog = null;
            characterDialogManager = null;

            currentState = null;
            target = null;
            distanceToTarget = 0f;
            lastTargetPosition = Vector3.zero;
            topNode = null;

            canFly = false;
            canFlyOverride = false;
            canGlide = false;
            canGlideOverride = false;

            isMounted = false;
            walking = false;
            frozen = false;
            stunned = false;
            levitated = false;
            motorEnabled = true;
            despawning = false;
            inWater = false;
            swimming = false;
            flying = false;

            currentWater.Clear();

            floatHeight = 1.5f;

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

            characterConfigured = false;

            interactionPoints.Clear();

            isStateReset = true;

            base.ResetSettings();
        }

        private void ProcessPointerExit() {
            if (isMouseOverUnit == true) {
                isMouseOverUnit = false;
                OnMouseOut();
            }
        }

        // for interactions
        public override float PerformFactionCheck(UnitController sourceUnitController) {
            if (characterUnit == null) {
                Debug.LogWarning($"{gameObject.name}.UnitController.PerformFactionCheck({sourceUnitController.gameObject.name}) characterUnit is null");
            }
            return Faction.RelationWith(sourceUnitController, this);
        }

        public override void GetComponentReferences() {
            //Debug.Log($"{gameObject.name}.UnitController.GetComponentReferences() instanceId: {GetInstanceID()}");

            if (componentReferencesInitialized == true) {
                //Debug.Log($"{gameObject.name}.UnitController.GetComponentReferences() already initialized");
                return;
            }

            // do this before the base because the base will create things that need to query the character
            characterUnit = new CharacterUnit(this, new InteractableOptionProps(), systemGameManager);

            // now that the characterUnit is available
            AddInteractableOption(characterUnit);

            base.GetComponentReferences();

            uuid = GetComponent<UUID>();
            lootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(this);
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
        public void SetCharacterConfiguration() {
            //Debug.Log($"{gameObject.name}.UnitController.SetCharacterConfiguration({characterRequestData.isServerOwned})");

            CharacterConfigurationRequest characterConfigurationRequest = characterRequestData.characterConfigurationRequest;

            isOwner = characterRequestData.isOwner;
            isServerOwned = characterRequestData.isServerOwned;

            if (characterRequestData.saveData != null) {
                characterSaveManager.SetSaveData(characterRequestData);
            }

            characterGroupManager.SetGroupId(characterRequestData.characterGroupId);

            characterGuildManager.SetGuildId(characterRequestData.characterGuildId, characterRequestData.characterGuildName);

            characterInventoryManager.PerformSetupActivities();

            unitModelController.LoadInitialSavedAppearance(characterConfigurationRequest.characterAppearanceData);

            // get a snapshot of the current state
            //CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(baseCharacter, systemGameManager);

            unitProfile = characterConfigurationRequest.unitProfile;
            if (string.IsNullOrEmpty(characterConfigurationRequest.characterName) == false) {
                baseCharacter.SetCharacterName(characterConfigurationRequest.characterName);
            }
            if (string.IsNullOrEmpty(characterConfigurationRequest.characterTitle) == false) {
                baseCharacter.SetCharacterTitle(characterConfigurationRequest.characterTitle);
            }
            if (characterConfigurationRequest.unitType != null) {
                baseCharacter.SetUnitType(characterConfigurationRequest.unitType);
            }
            if (characterConfigurationRequest.characterRace != null) {
                baseCharacter.SetCharacterRace(characterConfigurationRequest.characterRace);
            }
            if (characterConfigurationRequest.characterClass != null) {
                baseCharacter.SetCharacterClass(characterConfigurationRequest.characterClass);
            }
            if (characterConfigurationRequest.classSpecialization != null) {
                baseCharacter.SetClassSpecialization(characterConfigurationRequest.classSpecialization);
            }
            if (characterConfigurationRequest.faction != null) {
                baseCharacter.SetCharacterFaction(characterConfigurationRequest.faction);
            }
            if (characterConfigurationRequest.unitToughness != null) {
                baseCharacter.SetUnitToughness(characterConfigurationRequest.unitToughness);
            }
            characterStats.CurrentXP = characterConfigurationRequest.currentExperience;
            baseCharacter.SpawnDead = unitProfile.SpawnDead;
            if (characterConfigurationRequest.isDead) {
                characterStats.SetSpawnDead();
            }

            // get a snapshot of the new state
            //CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(baseCharacter, systemGameManager);

            //baseCharacter.ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, false);
            baseCharacter.CapabilityConsumerProcessor.UpdateCapabilityProviderList();
            baseCharacter.UpdateStatProviderList();

            characterStats.LoadLevel(characterConfigurationRequest.unitLevel);

            if (characterRequestData.saveData != null) {
                characterSaveManager.LoadSaveDataToCharacter();
                // safely visit scene node now that save data has been loaded and state will not be overwritten
                characterSaveManager.VisitSceneNode();
                characterStats.SetLevelInternal(characterConfigurationRequest.unitLevel);
                if (characterRequestData.saveData.InitializeResourceAmounts == true) {
                    characterStats.SetResourceAmountsToMaximum();
                    characterRequestData.saveData.InitializeResourceAmounts = false;
                } else {
                    // cap the resources to their max amount in case they were higher for some reason
                    characterStats.ClipResourceAmounts();
                }
            } else {
                characterStats.SetLevelInternal(characterConfigurationRequest.unitLevel);
                if (characterStats.IsAlive == true) {
                    characterStats.SetResourceAmountsToMaximum();
                }
            }

            // this must be called after setting the level in case the character has gear that is higher than level 1
            characterEquipmentManager.LoadDefaultEquipment((characterConfigurationRequest.unitControllerMode == UnitControllerMode.Player ? false : true));

            if (characterConfigurationRequest.unitControllerMode == UnitControllerMode.Player
                && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene())) {
                systemAchievementManager.AcceptAchievements(this);
            }

            // there could have been patches that provide new capabilities since the data was saved, so we need to capture the current state again
            // this is also necessary in order to transfer settings like toughness and faction to network clients for pets
            characterSaveManager.SaveGameData();

            //if (characterRequestData.saveData != null) {
                // now that the save data has been loaded, we can create event subscriptions to monitor changes to the character
                characterSaveManager.CreateEventSubscriptions();
            //}

            footstepType = unitProfile.FootstepType;

            if (unitProfile.FlightCapable == true) {
                canFly = true;
            }
            if (unitProfile.GlideCapable == true) {
                canGlide = true;
            }

            SetPersistenceProperties();


            // now that equipment has had a chance to be equipped, give character combat a chance to add default unit profile hit effects
            // in case no weapons were equipped
            characterCombat.AddUnitProfileHitEffects();

            // Trying to spawn dead relies on reading properties set in the previous method
            //characterStats.TrySpawnDead();

            SetUnitControllerMode(characterConfigurationRequest.unitControllerMode);

            unitModelController.Initialize();

            // get the unit model controller to fetch the appearance controller from the unit profile
            unitModelController.SetAppearanceController(unitProfile);

            unitModelController.SpawnUnitModel();

            SetUnitProfileInteractables();

            SetUnitFootstepAudioProfile();

            characterConfigured = true;
            unitEventController.NotifyOnCharacterConfigured();
        }

        private void SetUnitProfileInteractables() {
            //Debug.Log($"{gameObject.name}UnitController.SetUnitProfileInteractables()");

            if (unitProfile == null) {
                return;
            }

            // mounts, pets, players, and preview units should not create interaction options or subscribe to prerequisite updates
            if (unitControllerMode != UnitControllerMode.AI) {
                return;
            }

            // named interactable options
            foreach (InteractableOptionProps interactableOption in unitProfile.InteractableOptionConfigs) {
                if (interactableOption != null) {
                    InteractableOptionComponent interactableOptionComponent = interactableOption.GetInteractableOption(this);
                    AddInteractableOption(interactableOptionComponent);
                    
                    // because this profile is loaded after the save data is loaded, we need to send it to the new components manually
                    if (_interactableSaveData != null) {
                        interactableOptionComponent.LoadFromSaveData(_interactableSaveData);
                    }
                    //interactableOptionComponent.HandlePrerequisiteUpdates();
                }
            }

            // this will cause the minimap to be instantiated so it should be done after all interactables are added so the layers can be created properly
            foreach (InteractableOptionComponent interactableOptionComponent in interactables.Values) {
                interactableOptionComponent.HandleOptionStateChange();
            }

            // this is done earlier in GetComponentReferences, but has to be redone here because the component didn't exist until the unit profile created it
            lootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(this);
        }

        private void SetStartPosition() {
            //Debug.Log($"{gameObject.name}.UnitController.SetStartPosition(): {transform.position}");

            if (unitControllerMode == UnitControllerMode.Preview) {
                // preview units should not have their position changed at all by the unit controller
                return;
            }

            // pets have their start position set by master
            if (unitControllerMode != UnitControllerMode.Pet) {
                Vector3 correctedPosition = transform.position;
                if (unitMotor != null) {
                    correctedPosition = unitMotor.CorrectedNavmeshPosition(transform.position);
                }
                StartPosition = correctedPosition;
            }

            // prevent apparent velocity on first update by setting lastposition to currentposition
            lastPosition = rigidBody.position;
        }

        public void ConfigureAnimator() {
            //Debug.Log($"{gameObject.name}.UnitController.ConfigureAnimator()");

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
            //Debug.Log($"{gameObject.name}.UnitController.SetModelReady() {GetInstanceID()}");

            unitMaterialController.ProcessSetModelReady();
            OnCameraTargetReady();
            if (UnitModelController.UnitModel != null) {
                //Debug.Log($"{gameObject.name}.UnitController.SetModelReady() setting nameplate transform to unit model transform");
                nameplateTransform = UnitModelController.UnitModel.transform;
            }
        }

        public void SetMovementSoundArea(MovementSoundArea movementSoundArea) {
            //Debug.Log($"{gameObject.name}.CharacterUnit.SetMovementSoundArea()");
            if (movementSoundAreas.Contains(movementSoundArea) == false) {
                movementSoundAreas.Add(movementSoundArea);
                this.movementSoundArea = movementSoundArea;
            }
        }

        public void UnsetMovementSoundArea(MovementSoundArea movementSoundArea) {
            //Debug.Log($"{gameObject.name}.CharacterUnit.UnsetMovementSoundArea()");
            if (movementSoundAreas.Contains(movementSoundArea) == true) {
                movementSoundAreas.Remove(movementSoundArea);
                if (movementSoundAreas.Count == 0) {
                    this.movementSoundArea = null;
                } else {
                    this.movementSoundArea = movementSoundAreas[movementSoundAreas.Count - 1];
                }
            }
            if (unitControllerMode == UnitControllerMode.Mount && riderUnitController != null) {
                riderUnitController.UnsetMovementSoundArea(movementSoundArea);
            }
        }

        public void EnableMotor() {
            motorEnabled = true;
        }

        public void DisableMotor() {
            motorEnabled = false;
        }

        public virtual void CancelMountEffects() {
            //Debug.Log($"{gameObject.name}.UnitController.CancelMountEffects() instanceId: {GetInstanceID()}");

            if (isMounted == true) {
                //Debug.Log($"{gameObject.name}.UnitController.CancelMountEffects(): unit is mounted");

                foreach (StatusEffectNode statusEffectNode in characterStats.StatusEffects.Values) {
                    //Debug.Log($"{gameObject.name}.CharacterAbilityManager.PerformAbilityCast(): looping through status effects");
                    if (statusEffectNode.StatusEffect is MountEffectProperties) {
                        //Debug.Log($"{gameObject.name}.CharacterAbilityManager.PerformAbilityCast(): looping through status effects: found a mount effect");
                        statusEffectNode.CancelStatusEffect();
                        break;
                    }
                }
            }

            // update apparent velocity so any spellcast that caused the cancel mount is not interrupted
            lastPosition = rigidBody.position;
        }



        public void FollowAttackTarget(Interactable target, float minAttackRange) {
            //Debug.Log($"{gameObject.name}.AIController.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + ")");
            if (!(currentState is DeathState)) {
                UnitMotor.FollowAttackTarget(target, minAttackRange);
            }
        }

        public void ChangeState(IState newState) {
            //Debug.Log($"{gameObject.name}: ChangeState(" + newState.ToString() + ")");
            if (currentState != null) {
                currentState.Exit();
            }
            currentState = newState;

            currentState.Enter(this);
        }

        protected void Update() {
            // with new network code that requests save data before the configuration is complete, we need to check if the unit is initialized
            // because it may take a while to get the save data back
            if (isInitialized == false || isStateReset == true) {
                return;
            }

            if (characterStats.IsAlive == false) {
                // can't handle movement when dead
                return;
            }

            // no need to update if this is a preview unit
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }

            // in network mode, this is handled by the FishNetUnitController inside a [Replicate] call for client side prediction
            if ((unitControllerMode == UnitControllerMode.Mount || unitControllerMode == UnitControllerMode.Player) && systemGameManager.GameMode == GameMode.Local) {
                MovementData movementData = unitMovementController.ProcessGatheredInput();
                offlineTickCounter++;
                //Debug.Log($"{gameObject.name}.UnitController.Update() offlineTickCounter: {offlineTickCounter}");
                movementData.SimulatedTick = offlineTickCounter;
                unitMovementController.StateUpdate(movementData, Time.deltaTime, false);
            }

            if (motorEnabled) {
                unitMotor.Tick();
            }

            /*
            UpdateApparentVelocity();
            if (ApparentVelocity > 0.1f) {
                //Debug.Log($"{gameObject.name}.UnitController.Update() position: {transform.position}; apparentVelocity: {apparentVelocity}");
                characterAbilityManager.HandleManualMovement();
                unitActionManager.HandleManualMovement();
                unitEventController.NotifyOnMovement();
            }
            */

            HandleMovementAudio();

            characterCombat.Tick();

            // do this after combat so regen ticks can use the proper combat state
            characterStats.Update();
        }

        public void FixedUpdate() {
            if (isInitialized == false || isStateReset == true) {
                return;
            }
            if (target != null) {
                // prevent distance calculation if no movement has occured
                if (rigidBody.position != lastPosition || target.transform.position != lastTargetPosition) {
                    distanceToTarget = Vector3.Distance(target.transform.position, rigidBody.position);
                }
                lastTargetPosition = target.transform.position;
            }

            UpdateApparentVelocity();
            CalculateVelocityEffects();

            if (ControlLocked) {
                // can't allow any action if we are stunned/frozen/etc
                //Debug.Log($"{gameObject.name}.AIController.FixedUpdate(): controlLocked: " + MyControlLocked);
                return;
            }
            if (currentState != null) {
                currentState.Update();
            }
            if (motorEnabled) {
                unitMotor?.FixedTick();
            }
        }

        public void CalculateVelocityEffects() {
            //Debug.Log($"{gameObject.name}.UnitController.CalculateVelocityEffects() position: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z}) lastFrozenPosition: ({lastFrozenPosition.x}, {lastFrozenPosition.y}, {lastFrozenPosition.z}) apparentVelocity: {apparentVelocity}");

            if (apparentVelocity > 0.1f && lastApparentVelocity > 0.1f) {
                //Debug.Log($"{gameObject.name}.UnitController.CalculateVelocityEffects() position: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z}) ; apparentVelocity: {apparentVelocity}");
                /*
                if (unitControllerMode == UnitControllerMode.Player && networkManagerServer.ServerModeActive == true) {
                    Physics.SyncTransforms();
                }
                */
                characterAbilityManager.HandleApparentMovement();
                unitActionManager.HandleApparentMovement();
                unitEventController.NotifyOnMovement();
            }
        }

        public void SetAggroRange() {
            //Debug.Log($"{gameObject.name}.UnitController.SetAggroRange()");

            unitEventController.NotifyOnSetAggroRange(AggroRadius);
            if (!(currentState is DeathState)) {
                if (UnitProfile.IsAggressive == true) {
                    EnableAggro();
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

        public void ApplyControlEffects(UnitController masterUnitController) {
            //Debug.Log($"{gameObject.name}.UnitController.ApplyControlEffects({masterUnitController.gameObject.name})");

            if (!underControl) {
                underControl = true;
                masterUnit = masterUnitController;
                // done so pets of player unit wouldn't attempt to attack npcs questgivers etc
                //masterUnit.MyCharacterController.OnSetTarget += SetTarget;
                if (masterUnit == null) {
                    //Debug.Log($"{gameObject.name}.AIController.ApplyControlEffects(): masterUnit is null, returning");
                    return;
                }
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManagerClient.IsCutscene()) {
                    masterUnit.UnitEventController.OnClearTarget += HandleClearTarget;
                    masterUnit.UnitEventController.OnBeginCastOnEnemy += HandleMasterAttack;
                    masterUnit.UnitEventController.OnDropCombat += HandleMasterDropCombat;
                    masterUnit.UnitEventController.OnMovement += HandleMasterMovement;
                    masterUnit.UnitEventController.OnLevelChanged += HandleMasterLevelChanged;

                    // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
                    characterCombat.AggroTable.ClearTable();
                }

                CharacterFactionManager.NotifyOnReputationChange();
            } else {
                //Debug.Log("Can only be under the control of one master at a time");
            }
        }

        public void RemoveControlEffects() {
            if (underControl && masterUnit != null) {
                //masterUnit.MyCharacterController.OnSetTarget -= SetTarget;
                masterUnit.UnitEventController.OnClearTarget -= HandleClearTarget;
                masterUnit.UnitEventController.OnBeginCastOnEnemy -= HandleMasterAttack;
                masterUnit.UnitEventController.OnDropCombat -= HandleMasterDropCombat;
                masterUnit.UnitEventController.OnMovement -= HandleMasterMovement;
                masterUnit.UnitEventController.OnLevelChanged -= HandleMasterLevelChanged;
            }
            masterUnit = null;
            underControl = false;

            // CLEAR AGRO TABLE OR NOTIFY REPUTATION CHANGE - THIS SHOULD PREVENT ATTACKING SOMETHING THAT SUDDENLY IS UNDER CONTROL AND NOW YOUR FACTION WHILE YOU ARE INCOMBAT WITH IT
            characterCombat.AggroTable?.ClearTable();

            // nothing past this point needs to happen if the unit is despawning
            if (despawning) {
                return;
            }
            characterFactionManager.NotifyOnReputationChange();

            // should we reset leash position to start position here ?
        }

        private void HandleMasterLevelChanged(int newLevel) {
            //Debug.Log($"{gameObject.name}.UnitController.HandleMasterLevelChanged({newLevel})");

            characterStats.SetLevel(newLevel);
        }

        public void HandleMasterMovement() {
            //Debug.Log($"{gameObject.name}.AIController.OnMasterMovement()");
            SetMasterRelativeDestination();
        }

        public void SetMasterRelativeDestination(bool forceUpdate = false) {
            //Debug.Log($"{gameObject.name}.UnitController.SetMasterRelativeDestination(" + forceUpdate + ")");
            if (UnderControl == false) {
                // only do this stuff if we actually have a master
                //Debug.Log($"{gameObject.name}.AIController.SetMasterRelativeDestination(): not under control");
                return;
            }
            //Debug.Log($"{gameObject.name}.UnitController.SetMasterRelativeDestination()");

            // stand to the right of master by one meter
            Vector3 masterRelativeDestination = masterUnit.InteractableGameObject.transform.position + masterUnit.InteractableGameObject.transform.TransformDirection(Vector3.right);
            float usedMaxDistance = 0f;
            if (characterCombat.GetInCombat() == true) {
                usedMaxDistance = maxCombatDistanceFromMasterOnMove;
            } else {
                usedMaxDistance = maxDistanceFromMasterOnMove;
            }

            if (forceUpdate
                || (Vector3.Distance(gameObject.transform.position, masterUnit.gameObject.transform.position) > usedMaxDistance
                && Vector3.Distance(LeashPosition, masterUnit.gameObject.transform.position) > usedMaxDistance)) {
                //Debug.Log($"{gameObject.name}.AIController.SetMasterRelativeDestination(): setting master relative destination");
                masterRelativeDestination = SetDestination(masterRelativeDestination);
                LeashPosition = masterRelativeDestination;
            } else {
                //Debug.Log($"{gameObject.name}.AIController.SetMasterRelativeDestination(): not greater than " + usedMaxDistance);
            }

        }

        public void HandleMasterAttack(UnitController targetUnitController) {
            //Debug.Log($"{gameObject.name}.HandleMasterAttack({targetUnitController.gameObject.name})");

            SetTarget(targetUnitController);
        }

        public void HandleMasterDropCombat() {
            CharacterCombat.TryToDropCombat();
            SetMasterRelativeDestination(true);
        }

        public void UpdateTarget() {
            //Debug.Log($"{gameObject.name}: UpdateTarget()");
            if (characterCombat == null) {
                //Debug.Log($"{gameObject.name}: UpdateTarget(): MyCharacterCombat is null. (ok for non combat units)");
                return;
            }
            if (characterCombat.AggroTable == null) {
                //Debug.Log($"{gameObject.name}: UpdateTarget(): MyCharacterCombat.MyAggroTable is null!!!");
                return;
            }
            topNode = null;
            if (underControl) {
                topNode = masterUnit.CharacterCombat.AggroTable.TopAgroNode;
            } else {
                topNode = characterCombat.AggroTable.TopAgroNode;
            }
            if (topNode == null) {
                if (Target != null) {
                    ClearTarget();
                }
                if (characterCombat.GetInCombat() == true) {
                    characterCombat.TryToDropCombat();
                }
                return;
            }
            /*
            if (MyTarget != null && MyTarget == topNode.aggroTarget.gameObject) {
                //Debug.Log($"{gameObject.name}: UpdateTarget() and the target remained the same: " + topNode.aggroTarget.name);
            }
            */
            //Debug.Log($"{gameObject.name}.AIController.UpdateTarget(): topNode: {(topNode.aggroTarget != null ? topNode.aggroTarget.UnitController.name : "null")} with agro value: {topNode.aggroValue}");
            topNode.aggroValue = Mathf.Clamp(topNode.aggroValue, 0, float.MaxValue);
            if (Target == null) {
                //Debug.Log($"{gameObject.name}.AIController.UpdateTarget(): target was null.  setting target: " + topNode.aggroTarget.gameObject.name);
                SetTarget(topNode.aggroTarget.Interactable);
                return;
            }
            if (Target != topNode.aggroTarget.Interactable) {
                //Debug.Log($"{gameObject.name}.AIController.UpdateTarget(): " + topNode.aggroTarget.gameObject.name + "[" + topNode.aggroValue + "] stole agro from " + MyTarget);
                ClearTarget();
                SetTarget(topNode.aggroTarget.Interactable);
            }
        }

        public Vector3 SetDestination(Vector3 destination) {
            //Debug.Log($"{gameObject.name}.UnitController.SetDestination({destination}) current location: {transform.position}");

            if ((currentState is DeathState) == false) {
                //if ((currentState is DeathState) == false && characterStats.IsReviving == false) {
                CommonMovementNotifier();
                return UnitMotor.MoveToPoint(destination);
            } else {
                //Debug.Log($"{gameObject.name}: aicontroller.SetDestination(" + destination + "). current location: " + transform.position + ". WE ARE DEAD, DOING NOTHING");
            }
            return transform.position;
        }

        /// <summary>
        /// Meant to be called when the enemy has finished evading and returned to the spawn position
        /// </summary>
        public void Reset() {
            //Debug.Log($"{gameObject.name}.AIController.Reset()");
            target = null;
            // testing - comment out below.  is there any time we ever expand or reduce it?  if not, then below line is not necessary ?
            //AggroRadius = initialAggroRange;
            characterStats.SetResourceAmountsToMaximum();
            if (UnitMotor == null) {
                return;
            }
            UnitMotor.MovementSpeed = MovementSpeed;
            UnitMotor.ResetPath();
        }

        public void DisableAggro() {
            //Debug.Log($"{gameObject.name}.UnitController.DisableAggro()");

            aggroEnabled = false;
            unitEventController.NotifyOnDisableAggro();
        }

        public void EnableAggro() {
            //Debug.Log($"{gameObject.name}.UnitController.EnableAggro()");

            aggroEnabled = true;
            unitEventController.NotifyOnEnableAggro();
        }

        public float GetMinAttackRange() {

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                return characterCombat.GetMinAttackRange(CombatStrategy.GetAttackRangeAbilityList(this));
            } else {
                // get from random attacks if no strategy exists
                return characterCombat.GetMinAttackRange(characterCombat.GetAttackRangeAbilityList());
            }
        }


        public bool HasMeleeAttack() {

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                AbilityProperties meleeAbility = CombatStrategy.GetMeleeAbility(this);
                if (meleeAbility != null) {
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                AbilityProperties validAttackAbility = characterCombat.GetMeleeAbility();
                if (validAttackAbility != null) {
                    return true;
                }
            }

            return false;
        }


        public void ResetCombat() {

            // PUT CODE HERE TO CHECK IF THIS ACTUALLY HAS MUSIC PROFILE, OTHERWISE MOBS WITH A STRATEGY BUT NO PROFILE THAT DIE MID BOSS FIGHT CAN RESET MUSIC

            if (CombatStrategy != null) {
                if (CombatStrategy.HasMusic() == true && networkManagerServer.ServerModeActive == false) {
                    //Debug.Log($"{gameObject.name}.AIController.ResetCombat(): attempting to turn off fight music");
                    SceneNode sceneNode = sceneUtilityService.GetSceneNodeBySceneName(gameObject.scene.name);
                    if (sceneNode != null) {
                        AudioClip musicClip = sceneNode.BackgroundMusicAudio;
                        if (musicClip != null) {
                            //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): music profile was set");
                            if (audioManager.MusicAudioSource.clip != musicClip) {
                                //Debug.Log(aiController.gameObject.name + "ReturnState.Enter(): playing default music");
                                audioManager.PlayMusic(musicClip);
                            }
                        } else {
                            // There was no music, turn it off instead
                            audioManager.StopMusic();
                        }
                    } else {
                        audioManager.StopMusic();
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
                UnitMotor.UseRootMotion = useRootMotion;
            }
        }

        /// <summary>
        /// play or stop movement loop
        /// </summary>
        private void HandleMovementAudio() {
            //Debug.Log($"{gameObject.name}.HandleMovementAudio() velocity: " + apparentVelocity);

            // if this unit has no configured audio, or is set to use footstep events and is not in a movement area with no footstep events do nothing
            if (unitProfile?.MovementAudioProfiles == null
                || unitProfile.MovementAudioProfiles.Count == 0
                || (unitProfile.PlayOnFootstep == true && (movementSoundArea == null || (movementSoundArea.MovementHitProfile != null && movementSoundArea.MovementLoopProfile == null)))
                || (unitProfile.PlayOnFootstep == false && movementSoundArea != null && (movementSoundArea.MovementHitProfile != null && movementSoundArea.MovementLoopProfile == null))
                ) {
                //Debug.Log($"{gameObject.name}.HandleMovementAudio(): nothing to do, returning");
                return;
            }

            if (unitAnimator != null
                && UnitAnimator.IsInAir() == false
                && isMounted == false
                && ControlLocked == false
                && swimming == false
                && flying == false
                //&& (apparentVelocity >= (characterStats.RunSpeed / 2f))) {
                && unitAnimator.GetBool("Moving") == true
                && MovementLoopProfile != null) {
                //Debug.Log($"{gameObject.name}.HandleMovementAudio(): up to run speed");
                if (!componentController.MovementSoundIsPlaying(true)) {
                    PlayMovementSound(MovementLoopProfile.AudioClip, true);
                    //unitComponentController.PlayMovement(MovementLoopProfile.AudioClip, true);
                }
            } else {
                //Debug.Log($"{gameObject.name}.HandleMovementAudio(): not up to run speed");
                if (componentController?.MovementSoundIsPlaying(true) == true) {
                    interactableEventController.NotifyOnStopMovementSound(false);
                }
            }
        }

        public void StopMovementSound() {
            //Debug.Log($"{gameObject.name}.StopMovementSound()");

            // stop playing sound in case movement sounds will change
            // only apply if no movement sound area is found, or the current movement sound area is using a loop
            // this should allow the sound of the current footstep to finish instead of getting cut off if it's a hit sound
            if (movementSoundArea == null
                || (movementSoundArea != null && movementSoundArea.MovementLoopProfile != null)) {
                interactableEventController.NotifyOnStopMovementSound(false);
            }
        }

        public void PlayMovementSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}.PlayMovementSound(" + (audioClip == null ? "null" : audioClip.name) + ", " + loop + ")");

            interactableEventController.NotifyOnPlayMovementSound(audioClip, loop);
        }

        public void PlayFootStep() {

            if (unitProfile != null && unitProfile.PlayOnFootstep == false) {
                // footstep sounds are movement loop only.  nothing to do
                return;
            }

            if (footstepType == FootstepType.None) {
                // do not play any footsteps
                return;
            }

            if (footstepType == FootstepType.Unit) {
                PlayUnitFootstep();
                return;
            }

            SetEnvironmentFootStepAudioProfile();

            if (footstepType == FootstepType.Environment) {
                PlayEnvironmentFootstep();
                return;
            }

            if (footstepType == FootstepType.Both) {
                PlayUnitFootstep();
                PlayEnvironmentFootstep();
                return;
            }

            // we got this far, the only choice left is unit fallback

            if ((environmentFootstepAudioProfile == null ||
                            environmentFootstepAudioProfile?.AudioClips == null ||
                            environmentFootstepAudioProfile.AudioClips.Count == 0)
                            ) {
                PlayUnitFootstep();
                return;
            }

            // we got this far, the only choice left is environment footstep
            PlayEnvironmentFootstep();
        }

        private void PlayUnitFootstep() {
            if ((unitFootstepAudioProfile == null ||
                                        unitFootstepAudioProfile.AudioClips == null ||
                                        unitFootstepAudioProfile.AudioClips.Count == 0)
                                        ) {
                return;
            }

            if (unitStepIndex >= unitFootstepAudioProfile.AudioClips.Count) {
                unitStepIndex = 0;
            }

            PlayMovementSound(unitFootstepAudioProfile.AudioClips[unitStepIndex], false);

            unitStepIndex++;
        }

        private void PlayEnvironmentFootstep() {
            if ((environmentFootstepAudioProfile == null ||
                                        environmentFootstepAudioProfile.AudioClips == null ||
                                        environmentFootstepAudioProfile.AudioClips.Count == 0)
                                        ) {
                return;
            }

            if (environmentStepIndex >= environmentFootstepAudioProfile.AudioClips.Count) {
                environmentStepIndex = 0;
            }

            PlayMovementSound(environmentFootstepAudioProfile.AudioClips[environmentStepIndex], false);

            environmentStepIndex++;
        }

        public void PlaySwimSound() {
            // play swim sound only if near surface
            if (currentWater.Count > 0
                && currentWater[0].SwimHitsAudioProfile?.AudioClip != null
                && Collider.bounds.max.y > currentWater[0].SurfaceHeight) {
                interactableEventController.NotifyOnPlayMovementSound(currentWater[0].SwimHitsAudioProfile?.AudioClip, false);
            }
        }

        /// <summary>
        /// reset velocity calculation so that casting in the same frame as the unit stops will not be cancelled
        /// </summary>
        public void ResetApparentVelocity() {
            //Debug.Log($"{gameObject.name}.UnitController.ResetApparentVelocity() rigidbody position: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z})");

            lastPosition = rigidBody.position;
            //Debug.Log($"{gameObject.name}.UnitController.ResetApparentVelocity() set lastposition to: ({lastPosition.x}, {lastPosition.y}, {lastPosition.z})");
            apparentVelocity = 0f;

            if (rigidBody.isKinematic == false) {
                rigidBody.linearVelocity = Vector3.zero;
            }
        }

        public void UpdateApparentVelocity() {
            //Debug.Log($"{gameObject.name}.UnitController.UpdateApparentVelocity() currentPosition: {transform.position}, lastPosition: {lastPosition}, deltaTime: {Time.fixedDeltaTime}");

            lastApparentVelocity = apparentVelocity;
            apparentVelocity = Vector3.Distance(rigidBody.position, lastPosition) * (1 / Time.fixedDeltaTime);
            /*
            if (apparentVelocity > 0.01f) {
                //Debug.Log($"{gameObject.name}.UnitController.UpdateApparentVelocity() currentPosition: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z}), lastPosition: ({lastPosition.x}, {lastPosition.y}, {lastPosition.z}), deltaTime: {Time.fixedDeltaTime} velocity: {apparentVelocity}");
            } else {
                //Debug.Log($"{gameObject.name}.UnitController.UpdateApparentVelocity() currentPosition: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z}), lastPosition: ({lastPosition.x}, {lastPosition.y}, {lastPosition.z}), deltaTime: {Time.fixedDeltaTime} velocity: {apparentVelocity} NO VELOCITY CHANGE");
            }
            */
            lastPosition = rigidBody.position;
            //Debug.Log($"{gameObject.name}.UnitController.UpdateApparentVelocity() set lastposition to: ({lastPosition.x}, {lastPosition.y}, {lastPosition.z})");
        }

        public override void ProcessLevelUnload() {
            //Debug.Log($"UnitController.ProcessLevelUnload() {GetInstanceID()}");
            //Debug.Log($"{gameObject.name}.UnitController.ProcessLevelUnload()");

            if (gameObject == null || gameObject.activeSelf == false) {
                // this could be a mount unit that was already despawned via the player CancelMountEffects() calls
                return;
            }

            base.ProcessNamePlateLevelUnload();
            Despawn(0f, false, true);
        }

        /// <summary>
        /// This function is called only by entry into an aggro range collider
        /// </summary>
        /// <param name="aggroTarget"></param>
        public void ProximityAggro(CharacterUnit aggroTarget) {
            //Debug.Log($"{gameObject.name}.UnitController.ProximityAggro({aggroTarget.UnitController.gameObject.name})");

            if (characterCombat.GetInCombat() == true) {
                //Debug.Log($"{gameObject.name}.UnitController.ProximityAggro(): already in combat");
                // already fighting this target or another target
                // just aggro without out of combat aggro notification
                Aggro(aggroTarget);
            } else {
                //Debug.Log($"{gameObject.name}.UnitController.ProximityAggro(): not in combat yet");
                if (Aggro(aggroTarget) == true) {
                    // was out of combat and this unit was not already in the aggro table
                    unitEventController.NotifyOnAggroTarget();
                }
            }

        }

        public bool Aggro(CharacterUnit aggroTarget) {
            //Debug.Log($"{gameObject.name}.UnitController.Aggro({aggroTarget.DisplayName})");

            // at this level, we are just pulling both parties into combat.

            if (currentState is DeathState) {
                // can't be in combat when dead
                return false;
            }

            if (aggroTarget.UnitController.CharacterCombat == null) {
                //Debug.Log("no character combat on target");
                return false;
            }

            if (characterCombat == null) {
                //Debug.Log("combat is null, this is an inanimate unit?");
                return false;
            }

            if (characterCombat.AggroTable.AggroTableContains(aggroTarget)) {
                return true;
            }

            // moved liveness check into EnterCombat to centralize logic because there are multiple entry points to EnterCombat
            aggroTarget.UnitController.CharacterCombat.PullIntoCombat(this);

            return characterCombat.PullIntoCombat(aggroTarget.UnitController);

            //return false;
        }

        private void ApplyControlLock() {
            characterAbilityManager.TryToStopAnyAbility();
            unitActionManager.TryToStopAction();
        }

        public void FreezeCharacter() {
            //Debug.Log($"{gameObject.name}.UnitController.FreezeCharacter()");

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
            //Debug.Log($"{gameObject.name}.UnitController.UnFreezeCharacter()");

            frozen = false;
            FreezeRotation();
            if (unitAnimator != null) {
                UnitAnimator.Animator.enabled = true;
                UnitAnimator.Animator.Rebind();
            }
            if (unitMotor != null) {
                unitMotor.UnFreezeCharacter();
            }
        }

        public void StunCharacter() {
            //Debug.Log($"{gameObject.name}.UnitController.StunCharacter(): ");
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
                //Debug.Log($"{gameObject.name}.UnitController.StunCharacter(): characteranimator was null");
            }
            if (UnitMotor != null) {
                UnitMotor.FreezeCharacter();
            } else {
                //Debug.Log($"{gameObject.name}.UnitController.StunCharacter(): charactermotor was null");
            }
        }

        public void UnStunCharacter() {
            //Debug.Log($"{gameObject.name}.UnitController.UnStunCharacter(): ");
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
            //Debug.Log($"{gameObject.name}.UnitController.LevitateCharacter(): ");
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
            //Debug.Log($"{gameObject.name}.UnitController.UnLevitateCharacter(): ");
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
            //Debug.Log($"{gameObject.name}.UnitController.SetTarget({(newTarget == null ? "null" : newTarget.gameObject.name)})");

            if (unitControllerMode == UnitControllerMode.AI || unitControllerMode == UnitControllerMode.Pet) {
                if (currentState is DeathState || currentState is EvadeState) {
                    return;
                }
                if (Target == null) {
                    target = newTarget;
                }
                //Debug.Log("my target is " + MyTarget.ToString());

                // this next block is disabled for testing because when a player moves into a collider, the aggro call is already called
                // and we don't want it called twice because it causes an incorrect return value in the first call.
                /*
                CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                if (targetCharacterUnit != null) {
                    Aggro(targetCharacterUnit);
                }
                */
            } else {
                if (target != null) {
                    ClearTarget();
                }
                target = newTarget;
            }
            UnitEventController.NotifyOnSetTarget(target);
            target.OnInteractableResetSettings += HandleTargetResetSettings;
            target.OnInteractableDisable += HandleTargetDisable;
        }

        public void HandleTargetResetSettings() {
            ClearTarget();
        }

        public void HandleTargetDisable() {
            ClearTarget();
        }

        // receive messages from master and pass them on
        public void HandleClearTarget(Interactable oldTarget) {
            //Debug.Log($"{gameObject.name}.UnitController.HandleClearTarget({(oldTarget == null ? "null" : oldTarget.gameObject.name)})");

            ClearTarget();
        }

        public void ClearTarget() {
            //Debug.Log($"{gameObject.name}.UnitController.ClearTarget()");

            if (target != null) {
                target.OnInteractableResetSettings -= HandleTargetResetSettings;
                target.OnInteractableDisable -= HandleTargetDisable;
            }
            Interactable oldTarget = target;
            target = null;
            // FIX ME (reenable possibly?)
            if (UnitMotor != null) {
                UnitMotor.StopFollowingTarget();
            }
            UnitEventController.NotifyOnClearTarget(oldTarget);
        }

        private Vector3 GetHitBoxCenter() {
            //Debug.Log($"{gameObject.name}.UnitController.GetHitBoxCenter()");
            if (characterUnit == null) {
                //Debug.Log($"{gameObject.name}BaseController.GetHitBoxCenter(): characterUnit.BaseCharacter.MyCharacterUnit is null!");
                return myCollider.bounds.center;
            }
            Vector3 returnValue = myCollider.bounds.center + (transform.forward * (characterUnit.HitBoxSize / 2f));
            //Vector3 returnValue = transform.TransformPoint(myCollider.bounds.center) + (transform.forward * (characterUnit.HitBoxSize / 2f));
            //Debug.Log($"{gameObject.name}.UnitController.GetHitBoxCenter() Capsule Collider Center is:" + characterUnit.BaseCharacter.MyCharacterUnit.transform.TransformPoint(characterUnit.BaseCharacter.MyCharacterUnit.gameObject.GetComponent<CapsuleCollider>().center));
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
            //Debug.Log($"{gameObject.name}.UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + ")");
            if (newTarget == null) {
                return false;
            }
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int interactableMask = 1 << LayerMask.NameToLayer("Interactable");
            int triggerMask = 1 << LayerMask.NameToLayer("Triggers");
            int validMask = (playerMask | characterMask | interactableMask | triggerMask);

            //Collider[] hitColliders = Physics.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, Quaternion.identity, validMask);
            Collider[] hitColliders = new Collider[100];
             int hitCount = physicsScene.OverlapBox(GetHitBoxCenter(), GetHitBoxSize() / 2f, hitColliders, transform.rotation, validMask);
            //Debug.Log($"{gameObject.name}.UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "); center: " + hitBoxCenter.x + " " + hitBoxCenter.y + " " + hitBoxCenter.z + "; size: " + GetHitBoxSize() + "; navEnabled: " + agent.enabled);
            int i = 0;
            //Check when there is a new collider coming into contact with the box
            while (i < hitCount) {
                //Debug.Log($"{gameObject.name}.UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "); center: " + GetHitBoxCenter() + "; size: " + GetHitBoxSize() + "Hit : " + hitColliders[i].gameObject.name + "[" + i + "]");

                if (hitColliders[i].gameObject == newTarget.InteractableGameObject) {
                    //Debug.Log($"{gameObject.name}.UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "): Hit : " + hitColliders[i].gameObject.name + "[" + i + "] return true");
                    return true;
                }
                i++;
            }
            //Debug.Log($"{gameObject.name}.UnitController.IsTargetInHitBox(" + (newTarget == null ? "null" : newTarget.gameObject.name) + "): return false");
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
            if ((currentState is DeathState) == true || characterStats.IsReviving == true) {
                return;
            }
            UnitEventController.NotifyOnManualMovement();
        }

        public bool CanGetValidAttack(bool beginAttack = false) {
            //Debug.Log($"{gameObject.name}.UnitController.CanGetValidAttack({beginAttack})");

            if (CombatStrategy != null) {
                // attempt to get a valid ability from combat strategy before defaulting to random attacks
                AbilityProperties validCombatStrategyAbility = CombatStrategy.GetValidAbility(this);
                if (validCombatStrategyAbility != null) {
                    characterAbilityManager.BeginAbility(validCombatStrategyAbility);
                    //Debug.Log($"{gameObject.name}.UnitController.CanGetValidAttack({beginAttack}): using combat strategy ability: {validCombatStrategyAbility.ResourceName}");
                    return true;
                }
            } else {
                // get random attack if no strategy exists
                AbilityProperties validAttackAbility = characterCombat.GetValidAttackAbility();
                if (validAttackAbility != null) {
                    characterAbilityManager.BeginAbility(validAttackAbility);
                    //Debug.Log($"{gameObject.name}.UnitController.CanGetValidAttack({beginAttack}): using random attack ability: {validAttackAbility.ResourceName}");
                    return true;
                }
            }

            //Debug.Log($"{gameObject.name}.UnitController.CanGetValidAttack({beginAttack}): no valid attack found");
            return false;
        }

        public void FreezePositionXZ() {
            //Debug.Log($"{gameObject.name}.UnitController.FreezePositionXZ() position: ({rigidBody.position.x}, {rigidBody.position.y}, {rigidBody.position.z})");

            RigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            lastFrozenPosition = rigidBody.position;
        }

        public void FreezeAll() {
            RigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void FreezeRotation() {
            //Debug.Log($"{gameObject.name}.UnitController.FreezeRotation()");
            RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        /// <summary>
        /// if this unit is configured to use the agent, enable it
        /// </summary>
        public void EnableAgent() {
            //Debug.Log($"{gameObject.name}.UnitController.EnableAgent()");

            if (NavMeshAgent != null && useAgent == true && NavMeshAgent.enabled == false) {
                NavMeshAgent.enabled = true;
            }
            NavMeshAgent.updateRotation = true;
        }

        public void DisableAgent() {
            //Debug.Log($"{gameObject.name}.UnitController.DisableAgent()");

            if (NavMeshAgent != null) {
                NavMeshAgent.enabled = false;
            }
        }

        public void DeactivateMountedState() {
            unitMountManager.DeactivateMountedState();
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

        public override void ProcessPlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitController.ProcessPlayerUnitSpawn()");

            // players do not need to react to their own spawn, and previews should never react
            // now players do because they need their minimap to show up
            //if (unitControllerMode == UnitControllerMode.Player || unitControllerMode == UnitControllerMode.Preview) {
            if (unitControllerMode == UnitControllerMode.Preview) {
                return;
            }

            behaviorController.HandlePlayerUnitSpawn(sourceUnitController);
            base.ProcessPlayerUnitSpawn(sourceUnitController);
        }

        public void HandleMovementSpeedUpdate() {
            if (UnitMotor != null) {
                UnitMotor.MovementSpeed = MovementSpeed;
            }
        }

        public override bool IsBuilding() {
            return unitModelController.IsBuilding();
        }

        public void EnterWater(WaterBody water) {
            if (currentWater.Contains(water) == false) {
                currentWater.Add(water);
                if (!inWater && water.EnterWaterAudioProfile?.AudioClip != null) {
                    interactableEventController.NotifyOnPlayMovementSound(water.EnterWaterAudioProfile.AudioClip, false);
                }
                inWater = true;
            }
        }

        public void StartSwimming(bool isReplay) {
            swimming = true;
            if (isReplay == false) {
                StopMovementSound();
            }
        }

        public void StopSwimming() {
            swimming = false;
        }

        public void StartFlying(bool isReplay) {
            flying = true;
            if (isReplay == false) {
                StopMovementSound();
                unitEventController.NotifyOnStartFlying();
            }
        }

        public void StopFlying(bool isReplay) {
            flying = false;
            if (isReplay == false) {
                unitEventController.NotifyOnStopFlying();
            }
        }

        public void ExitWater(WaterBody water) {
            if (currentWater.Contains(water) == true) {
                currentWater.Remove(water);
                if (currentWater.Count == 0) {
                    inWater = false;
                }
            }
        }

        


        public override void OnSendObjectToPool() {
            //Debug.Log($"{gameObject.name}.UnitController.UnitEventController.OnSendObjectToPool()");
            // recevied a message from the object pooler
            // this object is about to be pooled.  Re-enable all monobehaviors in case it was in preview mode

            base.OnSendObjectToPool();

            MonoBehaviour[] monoBehaviours = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monoBehaviours) {
                monoBehaviour.enabled = true;
            }
        }

        public void ActivateStealth() {

            isStealth = true;
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && isOwner == false) {
                characterStats.ClearStatusEffectPrefabs();
                namePlateController.RemoveNamePlate();
                CleanupMiniMapIndicator();
            }
            unitMaterialController.ActivateStealth();
            unitEventController.NotifyOnEnterStealth();
            NotifyOnInteractableDisable();
        }

        public void DeactivateStealth() {
            //Debug.Log($"{gameObject.name}.UnitController.DeactivateStealth()");

            isStealth = false;
            unitMaterialController.DeactivateStealth();
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && isOwner == false) {
                namePlateController.AddNamePlate();
                characterStats.SpawnStatusEffectPrefabs();
                InstantiateMiniMapIndicator();
            }
            unitEventController.NotifyOnLeaveStealth();

            // to ensure the character gets agrod if close to enemies, the collider must be cycled
            DisableCollider();
            EnableCollider();
        }

        public override bool IsMouseOverBlocked() {
            return isStealth;
        }

        public void WriteMessageFeedMessage(string messageText) {
            //Debug.Log($"{gameObject.name}.UnitController.WriteMessageFeedMessage({messageText})");

            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                // in network client mode, all messages should come from the server.
                return;
            }
            unitEventController.NotifyOnWriteMessageFeedMessage(messageText);
        }

        protected override void ConfigureComponents() {
            base.ConfigureComponents();
            if (componentController != null) {
                componentController.SetUnitController(this);
            }
        }

        public override Vector3 GetNameplatePosition() {
            //Debug.Log($"{gameObject.name}.UnitController.GetNameplatePosition()");
            
            Vector3 returnValue = nameplateTransform.position + nameplateVector;
            //Debug.Log($"{gameObject.name}.UnitController.GetNameplatePosition() nameplateTransform.position: {nameplateTransform.position} nameplateVector: {nameplateVector} returnValue: {returnValue}");
            return returnValue;
            //return nameplateTransform.position + nameplateVector;
        }

        public void EnterInteractableRange(Interactable interactable) {
            //Debug.Log($"{gameObject.name}.UnitController.EnterInteractableRange({interactable.gameObject.name})");

            if ((unitControllerMode == UnitControllerMode.Player || unitControllerMode == UnitControllerMode.Mount) == false) {
                return;
            }

            if (inRangeInteractables.Contains(interactable) == false) {
                inRangeInteractables.Add(interactable);
            }
            unitEventController.NotifyOnEnterInteractableRange(interactable);

            if (unitMotor.InteractionTarget != null
                && unitMotor.InteractionTarget == interactable
                && unitMotor.InteractionTransform == null) {
                // we have entered the range of the interactable we were following, and it doesn't have a specific interaction point
                // stop following the target and interact with it
                unitMotor.StopFollowingTarget();
                unitMovementController.ChangeState(CharacterMovementState.Idle, false);
                if (unitControllerMode == UnitControllerMode.Mount) {
                    UnitController cachedRiderUnitController = riderUnitController;
                    cachedRiderUnitController.CancelMountEffects();
                    // if we don't manually trigger this here, the unit is considered out of range for interaction because the triggerEnter()
                    // happens on physics ticks
                    interactable.InteractableTriggerEnter(cachedRiderUnitController.Collider);
                    interactionManagerServer.InteractWithInteractable(cachedRiderUnitController, interactable);
                    return;
                } else {
                    interactionManagerServer.InteractWithInteractable(this, interactable);
                }

                return;
            }

        }

        #region MessagePassthroughs

        public void BeginDialog(string dialogName) {
            dialogController.BeginDialog(this, dialogName);
        }

        public void BeginChatMessage(string messageText) {
            dialogController.BeginChatMessage(messageText);
        }

        public void BeginPatrol(string patrolName) {
            patrolController.BeginPatrol(patrolName);
        }

        public void BeginAction(string actionName) {
            //Debug.Log($"{gameObject.name}.UnitController.BeginAction({actionName})");

            unitActionManager.BeginAction(actionName);
        }

        public void BeginAbility(string abilityName) {
            //Debug.Log($"{gameObject.name}.UnitController.BeginAbility({abilityName})");

            characterAbilityManager.BeginAbility(abilityName);
        }

        public void StopBackgroundMusic() {
            behaviorController.StopBackgroundMusic();
        }

        public void StartBackgroundMusic() {
            behaviorController.StartBackgroundMusic();
        }

        public void Knockback(float explosionForce, Vector3 explosionCenter, float upwardModifier) {
            if (unitMovementController != null) {
                unitMovementController.KnockBack();
            }
            unitMotor.Knockback(explosionForce, explosionCenter, upwardModifier);

        }

        public void Knockback(Vector3 direction) {
            if (unitMovementController != null) {
                unitMovementController.KnockBack();
            }
            unitMotor.Move(direction);
        }


        #endregion
    }

    public enum UnitControllerMode { Preview, Player, AI, Mount, Pet, Inanimate };

}