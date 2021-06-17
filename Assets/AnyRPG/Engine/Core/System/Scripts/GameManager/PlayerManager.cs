using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PlayerManager : MonoBehaviour {

        #region Singleton
        private static PlayerManager instance;

        public static PlayerManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PlayerManager>();
                }

                return instance;
            }
        }
        #endregion

        [SerializeField]
        private int initialLevel = 1;

        [SerializeField]
        private float maxMovementSpeed = 20f;

        [SerializeField]
        private LayerMask defaultGroundMask;

        [SerializeField]
        private GameObject playerConnectionParent = null;

        [SerializeField]
        private GameObject playerConnectionPrefab = null;

        [SerializeField]
        private GameObject playerUnitParent = null;

        [SerializeField]
        private GameObject aiUnitParent = null;

        [SerializeField]
        private GameObject effectPrefabParent = null;

        private string currentPlayerName = string.Empty;

        [Tooltip("If true, the system will enable the nav mesh agent for character navigation if a nav mesh exists in the scene")]
        [SerializeField]
        private bool autoDetectNavMeshes = false;

        [SerializeField]
        private bool autoSpawnPlayerOnLevelLoad = false;

        /// <summary>
        /// The invisible gameobject that stores all the player scripts. A reference to an instantiated playerPrefab
        /// </summary>
        private GameObject playerConnectionObject = null;

        private PlayerUnitMovementController playerUnitMovementController = null;

        private PlayerController playerController = null;

        // The actual movable rendered unit in the game world that we will be moving around
        //private GameObject playerUnitObject = null;

        private bool playerUnitSpawned = false;

        private bool playerConnectionSpawned = false;

        // a reference to the 'main' character.  This should remain constant as long as the player is logged in
        private BaseCharacter character = null;

        // a reference to the active character.  This can change the player mind controls a unit to allow things like action bar ability updates
        private BaseCharacter activeCharacter = null;

        // a reference to the 'main' unit.  This should be the main character when spawned, and null when not spawned
        private UnitController unitController = null;

        // a reference to the active unit.  This could change in cases of both mind control and mounted states
        private UnitController activeUnitController = null;

        protected bool eventSubscriptionsInitialized = false;

        public BaseCharacter MyCharacter { get => character; set => character = value; }

        public GameObject PlayerConnectionObject { get => playerConnectionObject; set => playerConnectionObject = value; }
        //public GameObject PlayerUnitObject { get => playerUnitObject; set => playerUnitObject = value; }
        public float MaxMovementSpeed { get => maxMovementSpeed; set => maxMovementSpeed = value; }
        public bool PlayerUnitSpawned { get => playerUnitSpawned; }
        public bool PlayerConnectionSpawned { get => playerConnectionSpawned; }
        public int InitialLevel { get => initialLevel; set => initialLevel = value; }
        public GameObject AIUnitParent { get => aiUnitParent; set => aiUnitParent = value; }
        public GameObject EffectPrefabParent { get => effectPrefabParent; set => effectPrefabParent = value; }
        public GameObject PlayerUnitParent { get => playerUnitParent; set => playerUnitParent = value; }
        public LayerMask DefaultGroundMask { get => defaultGroundMask; set => defaultGroundMask = value; }
        public PlayerUnitMovementController PlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }
        public BaseCharacter ActiveCharacter { get => activeCharacter; set => activeCharacter = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }
        public UnitController ActiveUnitController { get => activeUnitController; }
        public PlayerController PlayerController { get => playerController; set => playerController = value; }

        private void Awake() {
            //Debug.Log("PlayerManager.Awake()");
            /*
            if (defaultIsNonUMAUnit == true) {
                currentPlayerUnitPrefab = defaultNonUMAPlayerUnitPrefab;
            } else {
                currentPlayerUnitPrefab = defaultUMAPlayerUnitPrefab;
            }
            */
        }

        public void PerformRequiredPropertyChecks() {
            if (aiUnitParent == null) {
                Debug.LogError("PlayerManager.Awake(): the ai unit parent is null.  Please set it in the inspector");
            }
            if (effectPrefabParent == null) {
                Debug.LogError("PlayerManager.Awake(): the effect prefab parent is null.  Please set it in the inspector");
            }
        }

        public void OrchestratorStart() {
            PerformRequiredPropertyChecks();
            SetupScriptableObjects();
            CreateEventSubscriptions();
        }

        public void SetupScriptableObjects() {


            //defaultCharacterCreatorUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(defaultCharacterCreatorUnitProfileName);
        }

        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            //CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            //SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.MyInstance.OnExitGame += ExitGameHandler;
            SystemEventManager.MyInstance.OnLevelChanged += PlayLevelUpEffects;
            SystemEventManager.StartListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                //SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
                SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
                SystemEventManager.MyInstance.OnExitGame -= ExitGameHandler;
                SystemEventManager.MyInstance.OnLevelChanged -= PlayLevelUpEffects;
                SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }
        /*
        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            Debug.Log("PlayerManager.HandleLevelUnload()");
            ProcessLevelUnload();
        }
        */

        public void ResetInitialLevel() {
            initialLevel = 1;
        }

        public void ExitGameHandler() {
            //Debug.Log("PlayerManager.ExitGameHandler()");
            //DespawnPlayerUnit();
            DespawnPlayerConnection();
            SaveManager.MyInstance.ClearSystemManagedCharacterData();
        }

        public void SetPlayerName(string newName) {
            //Debug.Log("PlayerManager.SetPlayerName()");
            if (newName != null && newName != string.Empty) {
                character.SetCharacterName(newName);
            }

            SystemEventManager.MyInstance.NotifyOnPlayerNameChanged();
            if (playerUnitSpawned) {
                UIManager.MyInstance.PlayerUnitFrameController.SetTarget(UnitController.NamePlateController);
            }
        }

        public void SetPlayerCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (newCharacterClass != null) {
                activeCharacter.ChangeCharacterClass(newCharacterClass);
            }
        }

        public void SetPlayerCharacterSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (newClassSpecialization != null) {
                character.ChangeClassSpecialization(newClassSpecialization);
            }
        }

        public void SetPlayerFaction(Faction newFaction) {
            //Debug.Log("PlayerManager.SetPlayerFaction(" + factionName + ")");
            if (newFaction != null) {
                character.JoinFaction(newFaction);
            }
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("PlayerManager.OnLevelLoad()");
            bool loadCharacter = true;
            SceneNode activeSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (activeSceneNode != null) {
                //Debug.Log("PlayerManager.OnLevelLoad(): we have a scene node");
                // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
                if ((activeSceneNode.AutoPlayCutscene != null && (activeSceneNode.AutoPlayCutscene.Viewed == false || activeSceneNode.AutoPlayCutscene.Repeatable == true))
                    || activeSceneNode.SuppressCharacterSpawn) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    loadCharacter = false;
                    CameraManager.MyInstance.DeactivateMainCamera();
                    //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(true);
                }
            } else {
                if (LevelManager.MyInstance.IsMainMenu()) {
                    loadCharacter = false;
                }
            }
            if (autoSpawnPlayerOnLevelLoad == true && loadCharacter) {
                //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(false);
                Vector3 spawnLocation = SpawnPlayerUnit();
                CameraManager.MyInstance.ActivateMainCamera(true);
                CameraManager.MyInstance.MainCameraController.SetTargetPositionRaw(spawnLocation, activeUnitController.transform.forward);
            }
        }

        public void PlayLevelUpEffects(int newLevel) {
            //Debug.Log("PlayerManager.PlayLevelUpEffect()");
            if (PlayerUnitSpawned == false || SystemConfigurationManager.Instance?.LevelUpEffect == null) {
                return;
            }
            // 0 to allow playing this effect for different reasons than levelup
            if (newLevel == 0 || newLevel != 1) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();

                SystemConfigurationManager.Instance.LevelUpEffect.Cast(SystemAbilityController.MyInstance, unitController, unitController, abilityEffectContext);
            }
        }

        public void PlayDeathEffect() {
            //Debug.Log("PlayerManager.PlayDeathEffect()");
            if (PlayerUnitSpawned == false || SystemConfigurationManager.Instance?.DeathEffect == null) {
                return;
            }
            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            SystemConfigurationManager.Instance.DeathEffect.Cast(SystemAbilityController.MyInstance, unitController, unitController, abilityEffectContext);
        }

        /*
        public void Initialize() {
            //Debug.Log("PlayerManager.Initialize()");
            SpawnPlayerConnection();
            SpawnPlayerUnit();
        }
        */

            /*
        public void ProcessLevelUnload() {
            DespawnPlayerUnit();
        }
        */

        public void DespawnPlayerUnit() {
            //Debug.Log("PlayerManager.DespawnPlayerUnit()");
            if (!playerUnitSpawned) {
                //Debug.Log("Player Unit is not spawned.  Nothing to despawn.  returning");
                return;
            }

            unitController.Despawn();
        }

        public void HandlePlayerDeath(string eventName, EventParamProperties eventParam) {
            //Debug.Log("PlayerManager.KillPlayer()");
            PlayDeathEffect();
        }

        public void RespawnPlayer() {
            //Debug.Log("PlayerManager.RespawnPlayer()");
            DespawnPlayerUnit();
            activeCharacter.CharacterStats.ReviveRaw();
            SpawnPlayerUnit();
        }

        public void RevivePlayerUnit() {
            //Debug.Log("PlayerManager.RevivePlayerUnit()");
            activeCharacter.CharacterStats.Revive();
        }

        public Vector3 SpawnPlayerUnit() {
            //Debug.Log("PlayerManager.SpawnPlayerUnit()");
            Vector3 spawnLocation = LevelManager.MyInstance.GetSpawnLocation();
            SpawnPlayerUnit(spawnLocation);
            return spawnLocation;
        }

        public void SpawnPlayerUnit(Vector3 spawnLocation) {
            //Debug.Log("PlayerManager.SpawnPlayerUnit(" + spawnLocation + ")");

            if (activeUnitController != null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Player Unit already exists");
                return;
            }

            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): playerConnectionObject is null, instantiating connection!");
                SpawnPlayerConnection();
            }
            if (activeCharacter.UnitProfile == null) {
                activeCharacter.SetUnitProfile(SystemConfigurationManager.Instance.DefaultPlayerUnitProfileName, true, -1, false);
            }

            // spawn the player unit and set references
            Vector3 spawnRotation = LevelManager.MyInstance.GetSpawnRotation();
            activeCharacter.UnitProfile.SpawnUnitPrefab(playerUnitParent.transform, spawnLocation, spawnRotation, UnitControllerMode.Player);
            if (activeUnitController == null) {
                Debug.LogError("PlayerManager.SpawnPlayerUnit(): No UnitController could be found, or player unit was not spawned properly");
                return;
            }

            if (activeUnitController.DynamicCharacterAvatar != null) {
                if (activeUnitController.ModelReady == false) {
                    SaveManager.MyInstance.LoadUMASettings(false);
                } else {
                    SaveManager.MyInstance.LoadUMASettings(false);
                }
            }

            if (LevelManager.MyInstance.NavMeshAvailable == true && autoDetectNavMeshes) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Enabling NavMeshAgent()");
                activeUnitController.EnableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = true;
                }
            } else {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Disabling NavMeshAgent()");
                activeUnitController.DisableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = false;
                }
            }

            // testing - move this to before the below calls so its initialized if a model is already ready
            activeUnitController.Init();

            if (activeUnitController.ModelReady == false) {
                // do UMA spawn stuff to wait for UMA to spawn
                SubscribeToModelReady();
            } else {
                // handle spawn immediately since this is a non UMA unit and waiting should not be necessary
                HandlePlayerUnitSpawn();
            }
            //activeUnitController.Init();
        }

        public void SetActiveUnitController(UnitController unitController) {
            //Debug.Log("PlayerManager.SetActiveUnitController(" + unitController.gameObject.name + ")");
            activeUnitController = unitController;

            // this should not be needed, baseCharacter should always point to the proper unit
            //activeCharacter.SetUnitController(activeUnitController);
        }

        public void SetUnitController(UnitController unitController) {
            //Debug.Log("PlayerManager.SetUnitController(" + unitController.gameObject.name + ")");
            this.unitController = unitController;
            activeUnitController = unitController;

            activeCharacter.SetUnitController(activeUnitController);

            if (unitController == null) {
                playerUnitSpawned = false;
                return;
            }

            if (unitController.CharacterUnit != null) {
                // erase the connection from the base character on the unit, back to its unit controller so it doesn't fire events
                unitController.CharacterUnit.BaseCharacter.SetUnitController(null);
                // connect the characterUnit back to the baseCharacter that the playerManager owns so we get logged in character settings, not the unit settings
                unitController.CharacterUnit.SetBaseCharacter(activeCharacter);
            } else {
                Debug.LogError("UnitProfile.SpawnUnitPrefab(): active unit controller had no characterUnit!");
            }
        }

        public void HandleModelReady() {
            //Debug.Log("PlayerManager.HandleModelReady()");
            UnsubscribeFromModelReady();

            HandlePlayerUnitSpawn();
        }

        private void HandlePlayerUnitSpawn() {
            //Debug.Log("PlayerManager.HandlePlayerUnitSpawn()");
            playerUnitSpawned = true;

            // inform any subscribers that we just spawned a player unit
            SystemEventManager.TriggerEvent("OnPlayerUnitSpawn", new EventParamProperties());

            playerController.SubscribeToUnitEvents();

            if (SystemConfigurationManager.Instance.UseThirdPartyMovementControl == false) {
                playerUnitMovementController.Init();
            } else {
                DisableMovementControllers();
            }
        }

        public void DisableMovementControllers() {
            playerUnitMovementController.enabled = false;
            playerUnitMovementController.MovementStateController.enabled = false;
        }

        public void EnableMovementControllers() {
            //Debug.Log("PlayerManager.EnableMovementControllers()");
            playerUnitMovementController.enabled = true;
            playerUnitMovementController.MovementStateController.enabled = true;
            playerUnitMovementController.Init();
        }

        public void SubscribeToModelReady() {
            //Debug.Log("PlayerManager.SubscribeToModelReady()");

            // try this earlier
            //SaveManager.MyInstance.LoadUMASettings(false);

            activeUnitController.OnModelReady += HandleModelReady;
        }

        public void UnsubscribeFromModelReady() {
            activeUnitController.OnModelReady -= HandleModelReady;
        }

        public void SpawnPlayerConnection() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");
            if (playerConnectionObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }
            playerConnectionObject = ObjectPooler.MyInstance.GetPooledObject(playerConnectionPrefab, playerConnectionParent.transform);
            character = playerConnectionObject.GetComponent<BaseCharacter>();
            activeCharacter = character;
            playerController = playerConnectionObject.GetComponent<PlayerController>();
            playerUnitMovementController = playerConnectionObject.GetComponent<PlayerUnitMovementController>();

            SystemEventManager.MyInstance.NotifyBeforePlayerConnectionSpawn();
            activeCharacter.Init();
            activeCharacter.Initialize(SystemConfigurationManager.Instance.DefaultPlayerName, initialLevel);
            playerConnectionSpawned = true;
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionSpawn();

            SubscribeToPlayerEvents();
        }

        public void DespawnPlayerConnection() {
            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is null.  exiting.");
                return;
            }
            UnsubscribeFromPlayerEvents();
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionDespawn();
            ObjectPooler.MyInstance.ReturnObjectToPool(playerConnectionObject);
            character = null;
            activeCharacter = null;
            playerUnitMovementController = null;
            playerConnectionSpawned = false;
        }

        public void SubscribeToPlayerEvents() {
            activeCharacter.CharacterStats.OnImmuneToEffect += HandleImmuneToEffect;
            activeCharacter.CharacterStats.OnDie += HandleDie;
            activeCharacter.CharacterStats.OnReviveBegin += HandleReviveBegin;
            activeCharacter.CharacterStats.OnReviveComplete += HandleReviveComplete;
            MyCharacter.CharacterStats.OnLevelChanged += HandleLevelChanged;
            activeCharacter.CharacterStats.OnGainXP += HandleGainXP;
            activeCharacter.CharacterStats.OnStatusEffectAdd += HandleStatusEffectAdd;
            activeCharacter.CharacterStats.OnRecoverResource += HandleRecoverResource;
            activeCharacter.CharacterStats.OnResourceAmountChanged += HandleResourceAmountChanged;
            activeCharacter.CharacterStats.OnCalculateRunSpeed += HandleCalculateRunSpeed;
            activeCharacter.CharacterCombat.OnKillEvent += HandleKillEvent;
            activeCharacter.CharacterCombat.OnEnterCombat += HandleEnterCombat;
            activeCharacter.CharacterCombat.OnDropCombat += HandleDropCombat;
            activeCharacter.CharacterCombat.OnUpdate += HandleCombatUpdate;
            activeCharacter.CharacterCombat.OnReceiveCombatMiss += HandleCombatMiss;
            activeCharacter.CharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            activeCharacter.CharacterAbilityManager.OnUnlearnAbilities += HandleUnlearnClassAbilities;
            activeCharacter.CharacterAbilityManager.OnLearnedCheckFail += HandleLearnedCheckFail;
            activeCharacter.CharacterAbilityManager.OnPowerResourceCheckFail += HandlePowerResourceCheckFail;
            activeCharacter.CharacterAbilityManager.OnCombatCheckFail += HandleCombatCheckFail;
            activeCharacter.CharacterAbilityManager.OnAnimatedAbilityCheckFail += HandleAnimatedAbilityCheckFail;
            activeCharacter.CharacterAbilityManager.OnPerformAbility += HandlePerformAbility;
            activeCharacter.CharacterAbilityManager.OnBeginAbilityCoolDown += HandleBeginAbilityCoolDown;
            activeCharacter.CharacterAbilityManager.OnTargetInAbilityRangeFail += HandleTargetInAbilityRangeFail;
            activeCharacter.CharacterFactionManager.OnReputationChange += HandleReputationChange;
            activeCharacter.CharacterAbilityManager.OnUnlearnAbility += HandleUnlearnAbility;
            activeCharacter.CharacterAbilityManager.OnLearnAbility += HandleLearnAbility;
            activeCharacter.CharacterAbilityManager.OnActivateTargetingMode += HandleActivateTargetingMode;
            activeCharacter.CharacterAbilityManager.OnCombatMessage += HandleCombatMessage;
        }

        public void UnsubscribeFromPlayerEvents() {
            activeCharacter.CharacterStats.OnImmuneToEffect -= HandleImmuneToEffect;
            activeCharacter.CharacterStats.OnDie -= HandleDie;
            activeCharacter.CharacterStats.OnReviveBegin -= HandleReviveBegin;
            activeCharacter.CharacterStats.OnReviveComplete -= HandleReviveComplete;
            activeCharacter.CharacterStats.OnLevelChanged -= HandleLevelChanged;
            activeCharacter.CharacterStats.OnGainXP -= HandleGainXP;
            activeCharacter.CharacterStats.OnStatusEffectAdd -= HandleStatusEffectAdd;
            activeCharacter.CharacterStats.OnRecoverResource -= HandleRecoverResource;
            activeCharacter.CharacterStats.OnResourceAmountChanged -= HandleResourceAmountChanged;
            activeCharacter.CharacterStats.OnCalculateRunSpeed -= HandleCalculateRunSpeed;
            activeCharacter.CharacterCombat.OnKillEvent -= HandleKillEvent;
            activeCharacter.CharacterCombat.OnEnterCombat -= HandleEnterCombat;
            activeCharacter.CharacterCombat.OnDropCombat -= HandleDropCombat;
            activeCharacter.CharacterCombat.OnUpdate -= HandleCombatUpdate;
            activeCharacter.CharacterCombat.OnReceiveCombatMiss -= HandleCombatMiss;
            activeCharacter.CharacterEquipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            activeCharacter.CharacterAbilityManager.OnUnlearnAbilities -= HandleUnlearnClassAbilities;
            activeCharacter.CharacterAbilityManager.OnLearnedCheckFail -= HandleLearnedCheckFail;
            activeCharacter.CharacterAbilityManager.OnPowerResourceCheckFail -= HandlePowerResourceCheckFail;
            activeCharacter.CharacterAbilityManager.OnCombatCheckFail -= HandleCombatCheckFail;
            activeCharacter.CharacterAbilityManager.OnAnimatedAbilityCheckFail -= HandleAnimatedAbilityCheckFail;
            activeCharacter.CharacterAbilityManager.OnPerformAbility -= HandlePerformAbility;
            activeCharacter.CharacterAbilityManager.OnBeginAbilityCoolDown -= HandleBeginAbilityCoolDown;
            activeCharacter.CharacterAbilityManager.OnTargetInAbilityRangeFail -= HandleTargetInAbilityRangeFail;
            activeCharacter.CharacterFactionManager.OnReputationChange -= HandleReputationChange;
            activeCharacter.CharacterAbilityManager.OnUnlearnAbility -= HandleUnlearnAbility;
            activeCharacter.CharacterAbilityManager.OnLearnAbility -= HandleLearnAbility;
            activeCharacter.CharacterAbilityManager.OnActivateTargetingMode -= HandleActivateTargetingMode;
            activeCharacter.CharacterAbilityManager.OnCombatMessage -= HandleCombatMessage;
        }

        public void HandleBeginAbilityCoolDown() {
            SystemEventManager.TriggerEvent("OnBeginAbilityCooldown", new EventParamProperties());
        }

        public void HandleCombatMessage(string messageText) {
            CombatLogUI.MyInstance.WriteCombatMessage(messageText);
        }

        public void HandleCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            CombatTextManager.MyInstance.SpawnCombatText(targetObject, 0, CombatTextType.miss, CombatMagnitude.normal, abilityEffectContext);
        }

        public void HandleActivateTargetingMode(BaseAbility baseAbility) {
            CastTargettingManager.MyInstance.EnableProjector(baseAbility);
        }

        public void HandleAnimatedAbilityCheckFail(AnimatedAbility animatedAbility) {
            if (PlayerUnitSpawned == true && CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteCombatMessage("Cannot use " + (animatedAbility.DisplayName == null ? "null" : animatedAbility.DisplayName) + ". Waiting for another ability to finish.");
            }
        }

        public void HandleLearnAbility(BaseAbility baseAbility) {
            SystemEventManager.MyInstance.NotifyOnAbilityListChanged(baseAbility);
            baseAbility.NotifyOnLearn();
        }

        public void HandleUnlearnAbility(bool updateActionBars) {
            if (updateActionBars) {
                UIManager.MyInstance.ActionBarManager.UpdateVisuals(true);
            }
        }

        public void HandleCombatUpdate() {
            activeCharacter.CharacterCombat.HandleAutoAttack();
        }

        public void HandleDropCombat() {
            if (CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteCombatMessage("Left combat");
            }
        }

        public void HandleEnterCombat(Interactable interactable) {
            if (CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteCombatMessage("Entered combat with " + interactable.DisplayName);
            }
        }

        public void HandleReputationChange() {
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void HandleTargetInAbilityRangeFail(BaseAbility baseAbility, Interactable target) {
            if (baseAbility != null && CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteCombatMessage(target.name + " is out of range of " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName));
            }
        }

        public void HandlePerformAbility(BaseAbility ability) {
            SystemEventManager.MyInstance.NotifyOnAbilityUsed(ability);
            ability.NotifyOnAbilityUsed();

        }

        public void HandleCombatCheckFail(BaseAbility ability) {
            CombatLogUI.MyInstance.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while out of combat");
        }

        public void HandlePowerResourceCheckFail(BaseAbility ability, IAbilityCaster abilityCaster) {
            CombatLogUI.MyInstance.WriteCombatMessage("Not enough " + ability.PowerResource.DisplayName + " to perform " + ability.DisplayName + " at a cost of " + ability.GetResourceCost(abilityCaster));
        }

        public void HandleLearnedCheckFail(BaseAbility ability) {
            CombatLogUI.MyInstance.WriteCombatMessage("You have not learned the ability " + ability.DisplayName + " yet");
        }

        public void HandleUnlearnClassAbilities() {
            // now perform a single action bar update
            UIManager.MyInstance.ActionBarManager.UpdateVisuals(true);
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            if (PlayerUnitSpawned) {
                if (slotIndex != -1) {
                    InventoryManager.MyInstance.AddItem(oldItem, slotIndex);
                } else if (oldItem != null) {
                    InventoryManager.MyInstance.AddItem(oldItem);
                }
            }
            SystemEventManager.MyInstance.NotifyOnEquipmentChanged(newItem, oldItem);
        }

        /// <summary>
        /// trigger events with new speed information, mostly for third party controllers to pick up the new values
        /// </summary>
        /// <param name="oldRunSpeed"></param>
        /// <param name="currentRunSpeed"></param>
        /// <param name="oldSprintSpeed"></param>
        /// <param name="currentSprintSpeed"></param>
        public void HandleCalculateRunSpeed(float oldRunSpeed, float currentRunSpeed, float oldSprintSpeed, float currentSprintSpeed) {
            if (currentRunSpeed != oldRunSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentRunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
            if (currentSprintSpeed != oldSprintSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
        }

        public void HandleKillEvent(BaseCharacter sourceCharacter, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log(gameObject.name + ": About to gain xp from kill with creditPercent: " + creditPercent);
            MyCharacter.CharacterStats.GainXP((int)(LevelEquations.GetXPAmountForKill(activeCharacter.CharacterStats.Level, sourceCharacter) * creditPercent));
        }


        public void HandleRecoverResource(PowerResource powerResource, int amount) {
            if (CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteSystemMessage("You gain " + amount + " " + powerResource.DisplayName);
            }
        }

        public void HandleResourceAmountChanged(PowerResource powerResource, int amount, int amount2) {
            UIManager.MyInstance.ActionBarManager.UpdateVisuals();
        }

        public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
            if (statusEffectNode != null && statusEffectNode.StatusEffect.ClassTrait == false && activeUnitController != null) {
                if (statusEffectNode.AbilityEffectContext.savedEffect == false) {
                    if (activeUnitController.CharacterUnit != null) {
                        CombatTextManager.MyInstance.SpawnCombatText(activeUnitController, statusEffectNode.StatusEffect, true);
                    }
                }
            }
        }

        public void HandleGainXP(int xp) {
            if (CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteSystemMessage("You gain " + xp + " experience");
            }
            if (activeUnitController != null) {
                if (CombatTextManager.MyInstance != null) {
                    CombatTextManager.MyInstance.SpawnCombatText(activeUnitController, xp, CombatTextType.gainXP, CombatMagnitude.normal, null);
                }
            }
            SystemEventManager.MyInstance.NotifyOnXPGained();
        }

        public void HandleLevelChanged(int newLevel) {
            SystemEventManager.MyInstance.NotifyOnLevelChanged(newLevel);
            MessageFeedManager.MyInstance.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", newLevel.ToString()));
        }

        public void HandleReviveComplete() {
            SystemEventManager.TriggerEvent("OnReviveComplete", new EventParamProperties());
            if (activeUnitController != null) {
                activeUnitController.UnitAnimator.SetCorrectOverrideController();
            }
        }

        public void HandleReviveBegin() {
            playerController.HandleReviveBegin();
        }

        public void HandleDie(CharacterStats characterStats) {
            playerController.HandleDie(characterStats);
            SystemEventManager.TriggerEvent("OnPlayerDeath", new EventParamProperties());
        }

        public void HandleImmuneToEffect(AbilityEffectContext abilityEffectContext) {
            CombatTextManager.MyInstance.SpawnCombatText(activeUnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
        }

    }

}