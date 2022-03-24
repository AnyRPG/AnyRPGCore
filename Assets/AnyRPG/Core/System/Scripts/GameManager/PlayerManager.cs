using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PlayerManager : ConfiguredMonoBehaviour {

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

        // track if subscription to target ready should happen
        // only used when loading new level or respawning
        private bool subscribeToTargetReady = false;

        private Coroutine waitForPlayerReadyCoroutine = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemEventManager systemEventManager = null;
        protected UIManager uIManager = null;
        protected LevelManager levelManager = null;
        protected CameraManager cameraManager = null;
        protected SystemAbilityController systemAbilityController = null;
        protected LogManager logManager = null;
        protected CastTargettingManager castTargettingManager = null;
        protected CombatTextManager combatTextManager = null;
        protected InventoryManager inventoryManager = null;
        protected ActionBarManager actionBarManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected ObjectPooler objectPooler = null;
        protected ControlsManager controlsManager = null;

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

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            saveManager = systemGameManager.SaveManager;
            systemEventManager = systemGameManager.SystemEventManager;
            uIManager = systemGameManager.UIManager;
            combatTextManager = uIManager.CombatTextManager;
            actionBarManager = uIManager.ActionBarManager;
            messageFeedManager = uIManager.MessageFeedManager;
            levelManager = systemGameManager.LevelManager;
            cameraManager = systemGameManager.CameraManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
            logManager = systemGameManager.LogManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
            inventoryManager = systemGameManager.InventoryManager;
            objectPooler = systemGameManager.ObjectPooler;
            controlsManager = systemGameManager.ControlsManager;

            PerformRequiredPropertyChecks();
            CreateEventSubscriptions();
        }

        public void PerformRequiredPropertyChecks() {
            if (aiUnitParent == null) {
                Debug.LogError("PlayerManager.Awake(): the ai unit parent is null.  Please set it in the inspector");
            }
            if (effectPrefabParent == null) {
                Debug.LogError("PlayerManager.Awake(): the effect prefab parent is null.  Please set it in the inspector");
            }
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            systemEventManager.OnLevelChanged += PlayLevelUpEffects;
            SystemEventManager.StartListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void ResetInitialLevel() {
            initialLevel = 1;
        }

        public void ProcessExitToMainMenu() {
            //Debug.Log("PlayerManager.ProcessExitToMainMenu()");
            DespawnPlayerUnit();
            DespawnPlayerConnection();
            saveManager.ClearSystemManagedCharacterData();
        }

        public void SetPlayerName(string newName) {
            //Debug.Log("PlayerManager.SetPlayerName()");
            if (newName != null && newName != string.Empty) {
                character.SetCharacterName(newName);
            }

            SystemEventManager.TriggerEvent("OnPlayerNameChanged", new EventParamProperties());
            if (playerUnitSpawned) {
                uIManager.PlayerUnitFrameController.SetTarget(UnitController.NamePlateController);
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
            SceneNode activeSceneNode = levelManager.GetActiveSceneNode();
            if (activeSceneNode != null) {
                //Debug.Log("PlayerManager.OnLevelLoad(): we have a scene node");
                // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
                if ((activeSceneNode.AutoPlayCutscene != null && (activeSceneNode.AutoPlayCutscene.Viewed == false || activeSceneNode.AutoPlayCutscene.Repeatable == true))
                    || activeSceneNode.SuppressCharacterSpawn) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    loadCharacter = false;
                    cameraManager.DeactivateMainCamera();
                    //cameraManager.MyCharacterCreatorCamera.gameObject.SetActive(true);
                }
            } else {
                if (levelManager.IsMainMenu()) {
                    loadCharacter = false;
                }
            }
            if (autoSpawnPlayerOnLevelLoad == true && loadCharacter) {
                //cameraManager.MyCharacterCreatorCamera.gameObject.SetActive(false);
                Vector3 spawnLocation = SpawnPlayerUnit();
                cameraManager.ActivateMainCamera(true);
                cameraManager.MainCameraController.SetTargetPositionRaw(spawnLocation, activeUnitController.transform.forward);
            }
        }

        public void PlayLevelUpEffects(int newLevel) {
            //Debug.Log("PlayerManager.PlayLevelUpEffect()");
            if (PlayerUnitSpawned == false || systemConfigurationManager.LevelUpEffect == null) {
                return;
            }
            // 0 to allow playing this effect for different reasons than levelup
            if (newLevel == 0 || newLevel != 1) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();

                systemConfigurationManager.LevelUpEffect.AbilityEffectProperties.Cast(systemAbilityController, unitController, unitController, abilityEffectContext);
            }
        }

        public void PlayDeathEffect() {
            //Debug.Log("PlayerManager.PlayDeathEffect()");
            if (PlayerUnitSpawned == false || systemConfigurationManager.DeathEffect == null) {
                return;
            }
            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            systemConfigurationManager.DeathEffect.AbilityEffectProperties.Cast(systemAbilityController, unitController, unitController, abilityEffectContext);
        }

        /*
        public void Initialize() {
            //Debug.Log("PlayerManager.Initialize()");
            SpawnPlayerConnection();
            SpawnPlayerUnit();
        }
        */

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //DespawnPlayerUnit();
            if (playerController != null) {
                playerController.ProcessLevelUnload();
            }
        }

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
            SpawnPlayerUnit();

            if (activeCharacter.CharacterStats.IsAlive == false) {
                activeCharacter.CharacterStats.ReviveComplete();
            }
        }

        public void RevivePlayerUnit() {
            //Debug.Log("PlayerManager.RevivePlayerUnit()");
            activeCharacter.CharacterStats.Revive();
        }

        public void SubscribeToTargetReady() {
            activeUnitController.OnCameraTargetReady += HandleTargetReady;
            subscribeToTargetReady = false;
        }

        public void UnsubscribeFromTargetReady() {
            if (activeUnitController != null) {
                activeUnitController.OnCameraTargetReady -= HandleTargetReady;
            }
        }

        public void HandleTargetReady() {
            //Debug.Log(gameObject.name + ".UnitFrameController.HandleTargetReady()");

            waitForPlayerReadyCoroutine = StartCoroutine(WaitForPlayerReady());
        }

        private IEnumerator WaitForPlayerReady() {
            //Debug.Log("PlayerManager.WaitForPlayerReady()");
            //private IEnumerator WaitForCamera(int frameNumber) {
            yield return null;
            //Debug.Log(gameObject.name + ".UnitFrameController.WaitForCamera(): about to render " + namePlateController.Interactable.GetInstanceID() + "; initial frame: " + frameNumber + "; current frame: " + lastWaitFrame);
            //if (lastWaitFrame != frameNumber) {
            if (activeUnitController.IsBuilding() == true) {
                //Debug.Log(gameObject.name + ".UnitFrameController.WaitForCamera(): a new wait was started. initial frame: " + frameNumber +  "; current wait: " + lastWaitFrame);
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.WaitForCamera(): rendering");
                waitForPlayerReadyCoroutine = null;
                UnsubscribeFromTargetReady();
                cameraManager.ShowPlayers();
            }
        }

        public Vector3 SpawnPlayerUnit() {
            //Debug.Log("PlayerManager.SpawnPlayerUnit()");
            cameraManager.HidePlayers();
            subscribeToTargetReady = true;
            Vector3 spawnLocation = levelManager.GetSpawnLocation();
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
                activeCharacter.SetUnitProfile(systemConfigurationManager.DefaultPlayerUnitProfileName, true, -1, false);
            }

            // spawn the player unit and set references
            Vector3 spawnRotation = levelManager.GetSpawnRotation();
            activeCharacter.UnitProfile.SpawnUnitPrefab(playerUnitParent.transform, spawnLocation, spawnRotation, UnitControllerMode.Player);
            if (activeUnitController == null) {
                Debug.LogError("PlayerManager.SpawnPlayerUnit(): No UnitController could be found, or player unit was not spawned properly");
                return;
            }

            if (levelManager.NavMeshAvailable == true && autoDetectNavMeshes) {
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
            activeUnitController.UnitModelController.SetInitialSavedAppearance();
            if (subscribeToTargetReady) {
                SubscribeToTargetReady();
            }
            activeUnitController.Init();

            // saved appearance settings should only be run after the above Init() call or there is no reference to the avatar to load the settings onto
            /*
            if (activeUnitController.UnitModelController != null) {
                activeUnitController.UnitModelController.LoadSavedAppearanceSettings();
            }
            */

            if (activeUnitController?.UnitModelController?.ModelReady == false) {
                // do UMA spawn stuff to wait for UMA to spawn
                SubscribeToModelReady();
            } else {
                // handle spawn immediately since this is a non UMA unit and waiting should not be necessary
                HandlePlayerUnitSpawn();
            }
            //activeUnitController.Init();
            if (PlayerPrefs.HasKey("ShowNewPlayerHints") == false) {
                if (controlsManager.GamePadModeActive == true) {
                    uIManager.gamepadHintWindow.OpenWindow();
                } else {
                    uIManager.keyboardHintWindow.OpenWindow();
                }
            }
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
                unitController.CharacterUnit.SetCharacterStatsCapabilities();
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

            if (systemConfigurationManager.UseThirdPartyMovementControl == false) {
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
            //saveManager.LoadUMASettings(false);

            activeUnitController.UnitModelController.OnModelReady += HandleModelReady;
        }

        public void UnsubscribeFromModelReady() {
            activeUnitController.UnitModelController.OnModelReady -= HandleModelReady;
        }

        public void SpawnPlayerConnection() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");
            if (playerConnectionObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }
            playerConnectionObject = objectPooler.GetPooledObject(playerConnectionPrefab, playerConnectionParent.transform);
            character = playerConnectionObject.GetComponent<BaseCharacter>();
            character.Configure(systemGameManager);
            activeCharacter = character;
            playerController = playerConnectionObject.GetComponent<PlayerController>();
            playerController.Configure(systemGameManager);
            playerUnitMovementController = playerConnectionObject.GetComponent<PlayerUnitMovementController>();
            playerUnitMovementController.Configure(systemGameManager);

            SystemEventManager.TriggerEvent("OnBeforePlayerConnectionSpawn", new EventParamProperties());
            activeCharacter.Init();
            SubscribeToPlayerInventoryEvents();
            activeCharacter.Initialize(systemConfigurationManager.DefaultPlayerName, initialLevel);
            playerConnectionSpawned = true;
            SystemEventManager.TriggerEvent("OnPlayerConnectionSpawn", new EventParamProperties());

            SubscribeToPlayerEvents();
        }

        public void DespawnPlayerConnection() {
            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is null.  exiting.");
                return;
            }
            UnsubscribeFromPlayerInventoryEvents();
            UnsubscribeFromPlayerEvents();
            SystemEventManager.TriggerEvent("OnPlayerConnectionDespawn", new EventParamProperties());
            objectPooler.ReturnObjectToPool(playerConnectionObject);
            playerConnectionObject = null;
            character = null;
            activeCharacter = null;
            playerUnitMovementController = null;
            playerConnectionSpawned = false;
        }

        public void SubscribeToPlayerInventoryEvents() {
            activeCharacter.CharacterInventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            activeCharacter.CharacterInventoryManager.OnAddBankBagNode += HandleAddBankBagNode;
            activeCharacter.CharacterInventoryManager.OnAddInventorySlot += HandleAddInventorySlot;
            activeCharacter.CharacterInventoryManager.OnAddBankSlot += HandleAddBankSlot;
            activeCharacter.CharacterInventoryManager.OnRemoveInventorySlot += HandleRemoveInventorySlot;
            activeCharacter.CharacterInventoryManager.OnRemoveBankSlot += HandleRemoveBankSlot;
        }

        public void UnsubscribeFromPlayerInventoryEvents() {
            activeCharacter.CharacterInventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            activeCharacter.CharacterInventoryManager.OnAddBankBagNode -= HandleAddBankBagNode;
            activeCharacter.CharacterInventoryManager.OnAddInventorySlot -= HandleAddInventorySlot;
            activeCharacter.CharacterInventoryManager.OnAddBankSlot -= HandleAddBankSlot;
            activeCharacter.CharacterInventoryManager.OnRemoveInventorySlot -= HandleRemoveInventorySlot;
            activeCharacter.CharacterInventoryManager.OnRemoveBankSlot -= HandleRemoveBankSlot;
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
            activeCharacter.CharacterAbilityManager.OnMessageFeedMessage += HandleMessageFeedMessage;
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
            activeCharacter.CharacterAbilityManager.OnMessageFeedMessage -= HandleMessageFeedMessage;
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            inventoryManager.AddInventoryBagNode(bagNode);
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            inventoryManager.AddBankBagNode(bagNode);
        }

        public void HandleAddInventorySlot(InventorySlot inventorySlot) {
            inventoryManager.AddInventorySlot(inventorySlot);
        }

        public void HandleAddBankSlot(InventorySlot inventorySlot) {
            inventoryManager.AddBankSlot(inventorySlot);
        }

        public void HandleRemoveInventorySlot(InventorySlot inventorySlot) {
            inventoryManager.RemoveInventorySlot(inventorySlot);
        }

        public void HandleRemoveBankSlot(InventorySlot inventorySlot) {
            inventoryManager.RemoveBankSlot(inventorySlot);
        }

        public void HandleBeginAbilityCoolDown() {
            SystemEventManager.TriggerEvent("OnBeginAbilityCooldown", new EventParamProperties());
        }

        public void HandleCombatMessage(string messageText) {
            logManager.WriteCombatMessage(messageText);
        }

        public void HandleMessageFeedMessage(string messageText) {
            messageFeedManager.WriteMessage(messageText);
        }

        public void HandleCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(targetObject, 0, CombatTextType.miss, CombatMagnitude.normal, abilityEffectContext);
        }

        public void HandleActivateTargetingMode(BaseAbilityProperties baseAbility) {
            castTargettingManager.EnableProjector(baseAbility);
        }

        public void HandleAnimatedAbilityCheckFail(AnimatedAbilityProperties animatedAbility) {
            if (PlayerUnitSpawned == true && logManager != null) {
                logManager.WriteCombatMessage("Cannot use " + (animatedAbility.DisplayName == null ? "null" : animatedAbility.DisplayName) + ". Waiting for another ability to finish.");
            }
        }

        public void HandleLearnAbility(BaseAbilityProperties baseAbility) {
            systemEventManager.NotifyOnAbilityListChanged(baseAbility);
            baseAbility.NotifyOnLearn();
        }

        public void HandleUnlearnAbility(bool updateActionBars) {
            if (updateActionBars) {
                actionBarManager.RemoveStaleActions();
            }
        }

        public void HandleCombatUpdate() {
            activeCharacter.CharacterCombat.HandleAutoAttack();
        }

        public void HandleDropCombat() {
            Debug.Log("PlayerManager.HandleDropCombat()");

            if (logManager != null) {
                logManager.WriteCombatMessage("Left combat");
            }
        }

        public void HandleEnterCombat(Interactable interactable) {
            if (logManager != null) {
                logManager.WriteCombatMessage("Entered combat with " + interactable.DisplayName);
            }
        }

        public void HandleReputationChange() {
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void HandleTargetInAbilityRangeFail(BaseAbilityProperties baseAbility, Interactable target) {
            if (baseAbility != null && logManager != null) {
                logManager.WriteCombatMessage(target.name + " is out of range of " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName));
            }
        }

        public void HandlePerformAbility(BaseAbilityProperties ability) {
            systemEventManager.NotifyOnAbilityUsed(ability);
            ability.NotifyOnAbilityUsed();

        }

        public void HandleCombatCheckFail(BaseAbilityProperties ability) {
            logManager.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while out of combat");
        }

        public void HandlePowerResourceCheckFail(BaseAbilityProperties ability, IAbilityCaster abilityCaster) {
            logManager.WriteCombatMessage("Not enough " + ability.PowerResource.DisplayName + " to perform " + ability.DisplayName + " at a cost of " + ability.GetResourceCost(abilityCaster));
        }

        public void HandleLearnedCheckFail(BaseAbilityProperties ability) {
            logManager.WriteCombatMessage("You have not learned the ability " + ability.DisplayName + " yet");
        }

        public void HandleUnlearnClassAbilities() {
            // now perform a single action bar update
            actionBarManager.RemoveStaleActions();
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            if (PlayerUnitSpawned) {
                if (slotIndex != -1) {
                    MyCharacter.CharacterInventoryManager.AddInventoryItem(oldItem, slotIndex);
                } else if (oldItem != null) {
                    MyCharacter.CharacterInventoryManager.AddItem(oldItem, false);
                }
            }
            systemEventManager.NotifyOnEquipmentChanged(newItem, oldItem);
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
            MyCharacter.CharacterStats.GainXP((int)(LevelEquations.GetXPAmountForKill(activeCharacter.CharacterStats.Level, sourceCharacter, systemConfigurationManager) * creditPercent));
        }


        public void HandleRecoverResource(PowerResource powerResource, int amount) {
            if (logManager != null) {
                logManager.WriteCombatMessage("You gain " + amount + " " + powerResource.DisplayName);
            }
        }

        public void HandleResourceAmountChanged(PowerResource powerResource, int amount, int amount2) {
            actionBarManager.UpdateVisuals();
        }

        public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
            if (statusEffectNode != null && statusEffectNode.StatusEffect.ClassTrait == false && activeUnitController != null) {
                if (statusEffectNode.AbilityEffectContext.savedEffect == false) {
                    if (activeUnitController.CharacterUnit != null) {
                        combatTextManager.SpawnCombatText(activeUnitController, statusEffectNode.StatusEffect, true);
                    }
                }
            }
        }

        public void HandleGainXP(int xp) {
            if (logManager != null) {
                logManager.WriteSystemMessage("You gain " + xp + " experience");
            }
            if (activeUnitController != null) {
                if (combatTextManager != null) {
                    combatTextManager.SpawnCombatText(activeUnitController, xp, CombatTextType.gainXP, CombatMagnitude.normal, null);
                }
            }
            SystemEventManager.TriggerEvent("OnXPGained", new EventParamProperties());
        }

        public void HandleLevelChanged(int newLevel) {
            systemEventManager.NotifyOnLevelChanged(newLevel);
            messageFeedManager.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", newLevel.ToString()));
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
            combatTextManager.SpawnCombatText(activeUnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
        }

    }

}