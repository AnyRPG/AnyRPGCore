using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class PlayerManager : ConfiguredMonoBehaviour, ICharacterRequestor {

        [SerializeField]
        private float maxMovementSpeed = 20f;

        [SerializeField]
        private LayerMask defaultGroundMask;

        [SerializeField]
        private GameObject playerConnectionParent = null;

        [SerializeField]
        private GameObject playerConnectionPrefab = null;

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

        private bool playerUnitSpawned = false;

        private bool playerConnectionSpawned = false;

        // a reference to the 'main' unit.  This should be the main character when spawned, and null when not spawned
        private UnitController unitController = null;

        // a reference to the active unit.  This could change in cases of both mind control and mounted states
        private UnitController activeUnitController = null;

        // track if subscription to target ready should happen
        // only used when loading new level or respawning
        private bool subscribeToTargetReady = false;

        private Coroutine waitForPlayerReadyCoroutine = null;

        //private SpawnPlayerRequest spawnPlayerRequest = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected SaveManager saveManager = null;
        protected UIManager uIManager = null;
        protected LevelManager levelManager = null;
        protected CameraManager cameraManager = null;
        protected SystemAbilityController systemAbilityController = null;
        protected ClassChangeManagerClient classChangeManager = null;

        protected MessageLogClient messageLogClient = null;
        protected CastTargettingManager castTargettingManager = null;
        protected CombatTextManager combatTextManager = null;
        protected ActionBarManager actionBarManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected ObjectPooler objectPooler = null;
        protected ControlsManager controlsManager = null;
        protected NetworkManagerClient networkManagerClient = null;
        protected CharacterManager characterManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected PlayerManagerServer playerManagerServer = null;
        protected SystemAchievementManager systemAchievementManager = null;

        public GameObject PlayerConnectionObject { get => playerConnectionObject; set => playerConnectionObject = value; }
        public float MaxMovementSpeed { get => maxMovementSpeed; set => maxMovementSpeed = value; }
        public bool PlayerUnitSpawned { get => playerUnitSpawned; }
        public bool PlayerConnectionSpawned { get => playerConnectionSpawned; }
        public LayerMask DefaultGroundMask { get => defaultGroundMask; set => defaultGroundMask = value; }
        public PlayerUnitMovementController PlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }
        public UnitController ActiveUnitController { get => activeUnitController; }
        public PlayerController PlayerController { get => playerController; set => playerController = value; }
        //public PlayerCharacterSaveData PlayerCharacterSaveData { get => playerCharacterSaveData; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            uIManager = systemGameManager.UIManager;
            combatTextManager = uIManager.CombatTextManager;
            actionBarManager = uIManager.ActionBarManager;
            messageFeedManager = uIManager.MessageFeedManager;
            levelManager = systemGameManager.LevelManager;
            cameraManager = systemGameManager.CameraManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
            messageLogClient = systemGameManager.MessageLogClient;
            castTargettingManager = systemGameManager.CastTargettingManager;
            objectPooler = systemGameManager.ObjectPooler;
            controlsManager = systemGameManager.ControlsManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            characterManager = systemGameManager.CharacterManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            systemAchievementManager = systemGameManager.SystemAchievementManager;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnLevelUnloadClient += HandleLevelUnload;
            systemEventManager.OnLevelLoad += HandleLevelLoad;
            systemEventManager.OnLevelChanged += PlayLevelUpEffects;
            systemEventManager.OnPlayerDeath += HandlePlayerDeath;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnLevelUnloadClient -= HandleLevelUnload;
            systemEventManager.OnLevelLoad -= HandleLevelLoad;
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            systemEventManager.OnPlayerDeath -= HandlePlayerDeath;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        /*
        /// <summary>
        /// called when network client is stopped on the player unit
        /// </summary>
        public void ProcessStopClient() {
            //Debug.Log("PlayerManager.ProcessStopClient()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            playerManagerServer.DespawnPlayerUnit(networkManagerClient.AccountId);
        }
        */

        public void ProcessExitToMainMenu() {
            //Debug.Log("PlayerManager.ProcessExitToMainMenu()");

            if (unitController != null) {
                // we need to check here because the exit to main menu could have come from a network disconnection
                // that occured before the player unit was spawned
                playerManagerServer.DespawnPlayerUnit(networkManagerClient.AccountId);
            }
            DespawnPlayerConnection();
            saveManager.ClearSharedData();
        }

        public void HandleLevelLoad() {
            //Debug.Log("PlayerManager.HandleLevelLoad()");

            SceneNode activeSceneNode = levelManager.GetActiveSceneNode();
            
            if (activeSceneNode == null) {
                if (levelManager.IsMainMenu()) {
                    return;
                }
            }

            if (autoSpawnPlayerOnLevelLoad == false) {
                return;
            }

            //Debug.Log($"PlayerManager.OnLevelLoad(): scene node {(activeSceneNode == null ? "null" : activeSceneNode.ResourceName)}");
            // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
            if (activeSceneNode != null) {
                if ((activeSceneNode.AutoPlayCutscene != null && (activeSceneNode.AutoPlayCutscene.Viewed == false || activeSceneNode.AutoPlayCutscene.Repeatable == true))
                    || activeSceneNode.SuppressCharacterSpawn) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    return;
                }
            }

            // server does not spawn players
            if (systemGameManager.GameMode == GameMode.Network  && networkManagerServer.ServerModeActive == true) {
                return;
            }

            // only remove request if game type is network.  In local mode we need to save the spawn location
            SpawnPlayerRequest spawnSettings = playerManagerServer.GetSpawnPlayerRequest(networkManagerClient.AccountId, SceneManager.GetActiveScene().name, systemGameManager.GameMode == GameMode.Local);
            if (spawnSettings.overrideSpawnLocation == false && systemGameManager.GameMode == GameMode.Network) {
                // it is a network game, and we were loading the default location,
                // so randomize the spawn position a bit so players don't all spawn in the same place
                spawnSettings.spawnLocation = new Vector3(spawnSettings.spawnLocation.x + spawnSettings.xOffset, spawnSettings.spawnLocation.y, spawnSettings.spawnLocation.z + spawnSettings.zOffset);
            }

            cameraManager.MainCameraController.SetTargetPositionRaw(spawnSettings.spawnLocation, spawnSettings.spawnForwardDirection);

            RequestSpawnPlayerUnit();
        }

        public void PlayLevelUpEffects(UnitController sourceUnitController, int newLevel) {
            //Debug.Log("PlayerManager.PlayLevelUpEffect()");
            if (systemConfigurationManager.LevelUpEffect == null) {
                return;
            }
            // 0 to allow playing this effect for different reasons than levelup
            if (newLevel == 0 || newLevel != 1) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();

                systemConfigurationManager.LevelUpEffect.AbilityEffectProperties.Cast(systemAbilityController, sourceUnitController, sourceUnitController, abilityEffectContext);
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

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            //DespawnPlayerUnit();
            if (playerController != null) {
                playerController.ProcessLevelUnload();
            }
        }

        public void HandlePlayerDeath() {
            //Debug.Log("PlayerManager.KillPlayer()");
            PlayDeathEffect();
        }

        public void RequestRespawnPlayer() {
            //Debug.Log("PlayerManager.RespawnPlayer()");
            if (systemGameManager.GameMode == GameMode.Network) {
                //Debug.Log("PlayerManager.RequestRespawnPlayer(): Lobby Game Mode, requesting server to respawn player unit");
                networkManagerClient.RequestRespawnPlayerUnit();
                return;
            }

            playerManagerServer.RespawnPlayerUnit(0);
        }

        public void RequestRevivePlayer() {
            //Debug.Log("PlayerManager.RequestRevivePlayer()");

            if (systemGameManager.GameMode == GameMode.Network) {
                //Debug.Log("PlayerManager.RequestRespawnPlayer(): Lobby Game Mode, requesting server to respawn player unit");
                networkManagerClient.RequestRevivePlayerUnit();
                return;
            }

            playerManagerServer.RevivePlayerUnit(0);
        }

        public void SubscribeToTargetReady() {
            //Debug.Log($"PlayerManager.SubscribeToTargetReady()");

            activeUnitController.OnCameraTargetReady += HandleTargetReady;
            subscribeToTargetReady = false;
        }

        public void UnsubscribeFromTargetReady() {
            if (activeUnitController != null) {
                activeUnitController.OnCameraTargetReady -= HandleTargetReady;
            }
        }

        public void HandleTargetReady() {
            //Debug.Log($"PlayerManager.HandleTargetReady()");

            waitForPlayerReadyCoroutine = StartCoroutine(WaitForPlayerReady());
        }

        private IEnumerator WaitForPlayerReady() {
            //Debug.Log("PlayerManager.WaitForPlayerReady()");
            //private IEnumerator WaitForCamera(int frameNumber) {
            yield return null;
            //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): about to render " + namePlateController.Interactable.GetInstanceID() + "; initial frame: " + frameNumber + "; current frame: " + lastWaitFrame);
            //if (lastWaitFrame != frameNumber) {
            if (activeUnitController.IsBuilding() == true) {
                //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): a new wait was started. initial frame: " + frameNumber +  "; current wait: " + lastWaitFrame);
            } else {
                //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): rendering");
                waitForPlayerReadyCoroutine = null;
                UnsubscribeFromTargetReady();
                cameraManager.ShowPlayers();
            }
        }

        public void RequestSpawnPlayerUnit() {
            //Debug.Log("PlayerManager.SpawnPlayerUnit()");

            cameraManager.HidePlayers();
            subscribeToTargetReady = true;
            RequestSpawnPlayerUnit(networkManagerClient.AccountId);
        }

        public void RequestSpawnPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManager.RequestSpawnPlayerUnit({accountId})");

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.RequestSpawnPlayerUnit(SceneManager.GetActiveScene().name);
            } else {
                playerManagerServer.RequestSpawnPlayerUnit(accountId, SceneManager.GetActiveScene().name);
            }
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
            //Debug.Log($"PlayerManager.ConfigureSpawnedCharacter({unitController.gameObject.name})");

            //if (OwnPlayer(unitController, characterRequestData) == true) {
                //SetUnitController(unitController);
            //}

            if (levelManager.NavMeshAvailable == true && autoDetectNavMeshes) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Enabling NavMeshAgent()");
                unitController.EnableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = true;
                }
            } else {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Disabling NavMeshAgent()");
                unitController.DisableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = false;
                }
            }

            //unitController.UnitModelController.SetInitialSavedAppearance(playerCharacterSaveData.SaveData);
            if (subscribeToTargetReady) {
                SubscribeToTargetReady();
            }
        }

        public void PostInit(UnitController unitController) {
            //Debug.Log($"PlayerManager.PostInit({unitController.gameObject.name})");

            if (unitController.UnitModelController.ModelCreated == false) {
                // do UMA spawn stuff to wait for UMA to spawn
                SubscribeToModelReady();
            } else {
                // handle spawn immediately since this is a non UMA unit and waiting should not be necessary
                HandlePlayerUnitSpawn();
            }

            /*
            if (systemGameManager.GameMode == GameMode.Local) {
                // load player data from saveManager

                systemAchievementManager.CreateEventSubscriptions();
            }
            */

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
            //Debug.Log($"PlayerManager.SetUnitController({(unitController == null ? "null" : unitController.gameObject.name)})");

            this.unitController = unitController;
            activeUnitController = unitController;

            if (unitController == null) {
                playerManagerServer.RemoveActivePlayer(networkManagerClient.AccountId);
                playerUnitSpawned = false;
                return;
            }
            SubscribeToPlayerEvents();
            unitController.CharacterUnit.SetCharacterStatsCapabilities();
            //playerManagerServer.AddActivePlayer(0, unitController);
        }

        public void HandleModelReady() {
            //Debug.Log("PlayerManager.HandleModelReady()");
            SubscribeToModelEvents();
            UnsubscribeFromModelReady();

            HandlePlayerUnitSpawn();
        }

        public void SubscribeToModelEvents() {
            //Debug.Log("PlayerManager.SubscribeToModelEvents()");
            unitController.UnitEventController.OnStatusEffectAdd += HandleStatusEffectAdd;
        }

        public void UnsubscribeFromModelEvents() {
            //Debug.Log("PlayerManager.SubscribeToModelEvents()");
            unitController.UnitEventController.OnStatusEffectAdd -= HandleStatusEffectAdd;
        }

        private void HandlePlayerUnitSpawn() {
            //Debug.Log("PlayerManager.HandlePlayerUnitSpawn()");
            playerUnitSpawned = true;

            // inform any subscribers that we just spawned a player unit
            systemEventManager.NotifyOnPlayerUnitSpawn(unitController);

            playerController.SubscribeToUnitEvents();

            //if (systemConfigurationManager.UseThirdPartyMovementControl == false) {
                playerUnitMovementController.Init();
            //} else {
            //    DisableMovementControllers();
            //}
            
            if (unitController.CharacterStats.IsAlive == false) {
                // when a dead player spawns, we need to lock controller and allow popup to respawn
                DeathActions();
            }
        }

        public void DisableMovementControllers() {
            //Debug.Log("PlayerManager.DisableMovementControllers()");

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

            //activeUnitController.UnitModelController.OnModelUpdated += HandleModelReady;
            activeUnitController.UnitModelController.OnModelCreated += HandleModelReady;
        }

        public void UnsubscribeFromModelReady() {
            //Debug.Log("PlayerManager.UnsubscribeFromModelReady()");

            //activeUnitController.UnitModelController.OnModelUpdated -= HandleModelReady;
            activeUnitController.UnitModelController.OnModelCreated -= HandleModelReady;
        }

        public void SpawnPlayerConnection(CharacterSaveData characterSaveData) {
            //Debug.Log($"PlayerManager.SpawnPlayerConnection({playerCharacterSaveData.SaveData})");

            // this is only called in local mode so we can safely pass zero for account id
            playerManagerServer.AddPlayerMonitor(0, characterSaveData);

            SpawnPlayerConnectionObject();
        }

        public void SpawnPlayerConnection() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");

            SpawnPlayerConnectionObject();
        }

        public void SpawnPlayerConnectionObject() {
            //Debug.Log("PlayerManager.SpawnPlayerConnectionObject()");

            if (playerConnectionObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }

            playerConnectionObject = objectPooler.GetPooledObject(playerConnectionPrefab, playerConnectionParent.transform);
            playerController = playerConnectionObject.GetComponent<PlayerController>();
            playerController.Configure(systemGameManager);
            playerUnitMovementController = playerConnectionObject.GetComponent<PlayerUnitMovementController>();
            playerUnitMovementController.Configure(systemGameManager);

            SystemEventManager.TriggerEvent("OnBeforePlayerConnectionSpawn", new EventParamProperties());
            playerConnectionSpawned = true;
            SystemEventManager.TriggerEvent("OnPlayerConnectionSpawn", new EventParamProperties());

            // this goes here so action bars can get abilities on them when the player is initialized
            //SubscribeToPlayerEvents();
        }

        public void DespawnPlayerConnection() {
            //Debug.Log("PlayerManager.DespawnPlayerConnection()");

            // this only runs on the client, so is safe to call here
            playerManagerServer.StopMonitoringPlayerUnit(networkManagerClient.AccountId);

            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is null.  exiting.");
                return;
            }
            SystemEventManager.TriggerEvent("OnPlayerConnectionDespawn", new EventParamProperties());
            objectPooler.ReturnObjectToPool(playerConnectionObject);
            playerConnectionObject = null;
            playerUnitMovementController = null;
            playerConnectionSpawned = false;
        }

        public void SubscribeToPlayerEvents() {
            //Debug.Log("PlayerManager.SubscribeToPlayerEvents()");

            unitController.UnitEventController.OnImmuneToEffect += HandleImmuneToEffect;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
            //unitController.UnitEventController.OnAfterDie += HandleAfterDie;
            unitController.UnitEventController.OnReviveBegin += HandleReviveBegin;
            unitController.UnitEventController.OnReviveComplete += HandleReviveComplete;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnGainXP += HandleGainXP;
            unitController.UnitEventController.OnRecoverResource += HandleRecoverResource;
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.UnitEventController.OnEnterCombat += HandleEnterCombat;
            unitController.UnitEventController.OnDropCombat += HandleDropCombat;
            //unitController.UnitEventController.OnCombatUpdate += HandleCombatUpdate;
            unitController.UnitEventController.OnReceiveCombatMiss += HandleCombatMiss;
            unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;
            //unitController.UnitEventController.OnUnlearnAbilities += HandleUnlearnClassAbilities;
            unitController.UnitEventController.OnLearnedCheckFail += HandleLearnedCheckFail;
            unitController.UnitEventController.OnPowerResourceCheckFail += HandlePowerResourceCheckFail;
            unitController.UnitEventController.OnCombatCheckFail += HandleCombatCheckFail;
            unitController.UnitEventController.OnStealthCheckFail += HandleStealthCheckFail;
            unitController.UnitEventController.OnAbilityActionCheckFail += HandleAbilityActionCheckFail;
            //unitController.UnitEventController.OnTargetInAbilityRangeFail += HandleTargetInAbilityRangeFail;
            unitController.UnitEventController.OnReputationChange += HandleReputationChange;
            //unitController.UnitEventController.OnUnlearnAbility += HandleUnlearnAbility;
            unitController.UnitEventController.OnLearnAbility += HandleLearnAbility;
            unitController.UnitEventController.OnActivateTargetingMode += HandleActivateTargetingMode;
            unitController.UnitEventController.OnCombatMessage += HandleCombatMessage;
            unitController.UnitEventController.OnEnterInteractableRange += HandleEnterInteractableRange;
            unitController.UnitEventController.OnExitInteractableRange += HandleExitInteractableRange;
            unitController.UnitEventController.OnAcceptQuest += HandleAcceptQuest;
            unitController.UnitEventController.OnAbandonQuest += HandleRemoveQuest;
            unitController.UnitEventController.OnTurnInQuest += HandleRemoveQuest;
            unitController.UnitEventController.OnMarkQuestComplete += HandleMarkQuestComplete;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated += HandleQuestObjectiveStatusUpdated;
            unitController.UnitEventController.OnLearnSkill += HandleLearnSkill;
            unitController.UnitEventController.OnUnLearnSkill += HandleUnLearnSkill;
            unitController.UnitEventController.OnStartInteractWithOption += HandleStartInteractWithOption;
            unitController.UnitEventController.OnSetCraftAbility += HandleSetCraftAbility;
            unitController.UnitEventController.OnCraftItem += HandleCraftItem;
            unitController.UnitEventController.OnFactionChange += HandleFactionChange;
            unitController.UnitEventController.OnReceiveCombatTextEvent += HandleReceiveCombatTextEvent;
            unitController.UnitEventController.OnTakeDamage += HandleTakeDamage;
            unitController.UnitEventController.OnDespawn += HandleDespawn;
            unitController.UnitEventController.OnCurrencyChange += HandleCurrencyChange;
            unitController.UnitEventController.OnSetGamepadActionButton += HandleSetGamepadActionButton;
            unitController.UnitEventController.OnSetMouseActionButton += HandleSetMouseActionButton;
            unitController.UnitEventController.OnUnsetMouseActionButton += HandleUnsetMouseActionButton;
            unitController.UnitEventController.OnUnsetGamepadActionButton += HandleUnsetGamepadActionButton;
            unitController.UnitEventController.OnNameChange += HandleNameChange;
            unitController.UnitEventController.OnRemoveActivePet += HandleRemoveActivePet;
            unitController.UnitEventController.OnMarkAchievementComplete += HandleMarkAchievementComplete;
            unitController.UnitEventController.OnWriteMessageFeedMessage += HandleWriteMessageFeedMessage;
            unitController.UnitEventController.OnItemCountChanged += HandleItemCountChanged;
            unitController.CharacterInventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            unitController.CharacterInventoryManager.OnAddBankBagNode += HandleAddBankBagNode;
            unitController.CharacterInventoryManager.OnAddInventorySlot += HandleAddInventorySlot;
            unitController.CharacterInventoryManager.OnAddBankSlot += HandleAddBankSlot;
            unitController.CharacterInventoryManager.OnRemoveInventorySlot += HandleRemoveInventorySlot;
            unitController.CharacterInventoryManager.OnRemoveBankSlot += HandleRemoveBankSlot;
            unitController.UnitEventController.OnAddBag += HandleAddBag;
            unitController.UnitEventController.OnNameChangeFail += HandleNameChangeFail;
            unitController.UnitEventController.OnClassChange += HandleClassChange;
            unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChange;
            unitController.UnitEventController.OnSetGuildId += HandleSetGuildId;
        }

        public void UnsubscribeFromPlayerEvents() {
            //Debug.Log("PlayerManager.UnsubscribeFromPlayerEvents()");

            unitController.UnitEventController.OnImmuneToEffect -= HandleImmuneToEffect;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
            //unitController.UnitEventController.OnAfterDie -= HandleAfterDie;
            unitController.UnitEventController.OnReviveBegin -= HandleReviveBegin;
            unitController.UnitEventController.OnReviveComplete -= HandleReviveComplete;
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnGainXP -= HandleGainXP;
            unitController.UnitEventController.OnRecoverResource -= HandleRecoverResource;
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.UnitEventController.OnEnterCombat -= HandleEnterCombat;
            unitController.UnitEventController.OnDropCombat -= HandleDropCombat;
            //unitController.UnitEventController.OnCombatUpdate -= HandleCombatUpdate;
            unitController.UnitEventController.OnReceiveCombatMiss -= HandleCombatMiss;
            unitController.UnitEventController.OnAddEquipment -= HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment -= HandleRemoveEquipment;
            //unitController.UnitEventController.OnUnlearnAbilities -= HandleUnlearnClassAbilities;
            unitController.UnitEventController.OnLearnedCheckFail -= HandleLearnedCheckFail;
            unitController.UnitEventController.OnPowerResourceCheckFail -= HandlePowerResourceCheckFail;
            unitController.UnitEventController.OnCombatCheckFail -= HandleCombatCheckFail;
            unitController.UnitEventController.OnStealthCheckFail -= HandleStealthCheckFail;
            unitController.UnitEventController.OnAbilityActionCheckFail -= HandleAbilityActionCheckFail;
            //unitController.UnitEventController.OnTargetInAbilityRangeFail -= HandleTargetInAbilityRangeFail;
            unitController.UnitEventController.OnReputationChange -= HandleReputationChange;
            //unitController.UnitEventController.OnUnlearnAbility -= HandleUnlearnAbility;
            unitController.UnitEventController.OnLearnAbility -= HandleLearnAbility;
            unitController.UnitEventController.OnActivateTargetingMode -= HandleActivateTargetingMode;
            unitController.UnitEventController.OnCombatMessage -= HandleCombatMessage;
            unitController.UnitEventController.OnEnterInteractableRange -= HandleEnterInteractableRange;
            unitController.UnitEventController.OnExitInteractableRange -= HandleExitInteractableRange;
            unitController.UnitEventController.OnAcceptQuest -= HandleAcceptQuest;
            unitController.UnitEventController.OnAbandonQuest -= HandleRemoveQuest;
            unitController.UnitEventController.OnTurnInQuest -= HandleRemoveQuest;
            unitController.UnitEventController.OnMarkQuestComplete -= HandleMarkQuestComplete;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated -= HandleQuestObjectiveStatusUpdated;
            unitController.UnitEventController.OnLearnSkill -= HandleLearnSkill;
            unitController.UnitEventController.OnUnLearnSkill -= HandleUnLearnSkill;
            unitController.UnitEventController.OnStartInteractWithOption -= HandleStartInteractWithOption;
            unitController.UnitEventController.OnSetCraftAbility -= HandleSetCraftAbility;
            unitController.UnitEventController.OnCraftItem -= HandleCraftItem;
            unitController.UnitEventController.OnFactionChange -= HandleFactionChange;
            unitController.UnitEventController.OnReceiveCombatTextEvent -= HandleReceiveCombatTextEvent;
            unitController.UnitEventController.OnTakeDamage -= HandleTakeDamage;
            unitController.UnitEventController.OnDespawn -= HandleDespawn;
            unitController.UnitEventController.OnCurrencyChange -= HandleCurrencyChange;
            unitController.UnitEventController.OnSetGamepadActionButton += HandleSetGamepadActionButton;
            unitController.UnitEventController.OnSetMouseActionButton += HandleSetMouseActionButton;
            unitController.UnitEventController.OnUnsetMouseActionButton += HandleUnsetMouseActionButton;
            unitController.UnitEventController.OnUnsetGamepadActionButton += HandleUnsetGamepadActionButton;
            unitController.UnitEventController.OnNameChange -= HandleNameChange;
            unitController.UnitEventController.OnRemoveActivePet -= HandleRemoveActivePet;
            unitController.UnitEventController.OnMarkAchievementComplete -= HandleMarkAchievementComplete;
            unitController.UnitEventController.OnWriteMessageFeedMessage -= HandleWriteMessageFeedMessage;
            unitController.UnitEventController.OnItemCountChanged -= HandleItemCountChanged;
            unitController.CharacterInventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            unitController.CharacterInventoryManager.OnAddBankBagNode -= HandleAddBankBagNode;
            unitController.CharacterInventoryManager.OnAddInventorySlot -= HandleAddInventorySlot;
            unitController.CharacterInventoryManager.OnAddBankSlot -= HandleAddBankSlot;
            unitController.CharacterInventoryManager.OnRemoveInventorySlot -= HandleRemoveInventorySlot;
            unitController.CharacterInventoryManager.OnRemoveBankSlot -= HandleRemoveBankSlot;
            unitController.UnitEventController.OnAddBag -= HandleAddBag;
            unitController.UnitEventController.OnNameChangeFail -= HandleNameChangeFail;
            unitController.UnitEventController.OnClassChange -= HandleClassChange;
            unitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChange;
            unitController.UnitEventController.OnSetGuildId -= HandleSetGuildId;
        }

        private void HandleSetGuildId(int guildId, string guildName) {
            systemEventManager.NotifyOnSetGuildId(guildId);
        }

        private void HandleNameChangeFail() {
            systemEventManager.NotifyOnNameChangeFail();
        }

        private void HandleItemCountChanged(UnitController controller, Item item) {
            systemEventManager.NotifyOnItemCountChanged(unitController, item);
        }

        private void HandleWriteMessageFeedMessage(string messageText) {
            //Debug.Log($"PlayerManager.HandleWriteMessageFeedMessage({messageText})");

            messageFeedManager.WriteMessage(messageText);
        }

        public void HandleMarkAchievementComplete(UnitController targetUnitController, Achievement achievement) {
            PlayLevelUpEffects(targetUnitController, 0);
        }

        public void HandleRemoveActivePet(UnitProfile unitProfile) {
            systemEventManager.NotifyOnRemoveActivePet(unitProfile);
        }

        public void HandleNameChange(string newName) {
            systemEventManager.NotifyOnNameChange(newName);
            uIManager.PlayerUnitFramePanel.SetTarget(unitController);
        }

        public void HandleUnsetGamepadActionButton(int buttonIndex) {
            systemEventManager.NotifyOnUnsetGamepadActionButton(buttonIndex);
        }

        public void HandleUnsetMouseActionButton(int buttonIndex) {
            systemEventManager.NotifyOnUnsetMouseActionButton(buttonIndex);
        }

        public void HandleSetMouseActionButton(IUseable useable, int buttonIndex) {
            systemEventManager.NotifyOnSetMouseActionButton(useable, buttonIndex);
        }

        public void HandleSetGamepadActionButton(IUseable useable, int buttonIndex) {
            systemEventManager.NotifyOnSetGamepadActionButton(useable, buttonIndex);
        }

        public void HandleCurrencyChange(string currencyResourceName, int amount) {
            //Debug.Log("PlayerManager.HandleCurrencyChange()");
            systemEventManager.NotifyOnCurrencyChange();
        }

        public void HandleDespawn(UnitController controller) {
            UnsubscribeFromPlayerEvents();
            UnsubscribeFromModelEvents();
            systemEventManager.NotifyOnPlayerUnitDespawn(controller);
            SetUnitController(null);
        }

        public void HandleTakeDamage(IAbilityCaster sourceCaster, UnitController targetUnitController, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, AbilityEffectContext abilityEffectContext) {

            combatTextManager.SpawnCombatText(targetUnitController, amount, combatTextType, combatMagnitude, abilityEffectContext);
            systemEventManager.NotifyOnTakeDamage(sourceCaster, unitController, amount, abilityName);
        }

        public void HandleReceiveCombatTextEvent(UnitController targetUnitController, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(targetUnitController, amount, combatTextType, combatMagnitude, abilityEffectContext);
        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            systemEventManager.NotifyOnFactionChange();
            systemEventManager.NotifyOnReputationChange(unitController);
            messageFeedManager.WriteMessage($"Changed faction to {newFaction.DisplayName}");
        }

        public void HandleAddBag(InstantiatedBag bag, BagNode node) {
            systemEventManager.NotifyOnAddBag();
        }


        public void HandleCraftItem() {
            systemEventManager.NotifyOnCraftItem();
        }

        public void HandleSetCraftAbility(CraftAbilityProperties abilityProperties) {
            systemEventManager.NotifyOnSetCraftAbility(unitController, abilityProperties);
        }

        public void HandleStartInteractWithOption(UnitController sourceUnitController, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"PlayerManager.HandleStartInteractWithOption({sourceUnitController.gameObject.name}, {interactableOptionComponent.Interactable.gameObject.name}, {componentIndex}, {choiceIndex})");

            interactableOptionComponent.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
        }

        public void HandleLearnSkill(UnitController sourceUnitController, Skill skill) {
            //Debug.Log($"PlayerManager.HandleLearnSkill({sourceUnitController.gameObject.name}, {skill.ResourceName})");

            systemEventManager.NotifyOnLearnSkill(unitController, skill);
        }

        public void HandleUnLearnSkill(UnitController sourceUnitController, Skill skill) {
            systemEventManager.NotifyOnUnLearnSkill(unitController, skill);
        }

        public void HandleQuestObjectiveStatusUpdated(UnitController sourceUnitController, Quest quest) {
            systemEventManager.NotifyOnQuestObjectiveStatusUpdated(sourceUnitController, quest);
        }

        public void HandleMarkQuestComplete(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnMarkQuestComplete(sourceUnitController, questBase);
        }

        public void HandleRemoveQuest(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnRemoveQuest(sourceUnitController, questBase);
        }

        public void HandleAcceptQuest(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnAcceptQuest(sourceUnitController, questBase);
        }

        public void HandleEnterInteractableRange(UnitController controller, Interactable interactable) {
            playerController.AddInteractable(interactable);
        }

        public void HandleExitInteractableRange(UnitController controller, Interactable interactable) {
            playerController.RemoveInteractable(interactable);
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            systemEventManager.NotifyOnAddInventoryBagNode(bagNode);
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            systemEventManager.NotifyOnAddBankBagNode(bagNode);
        }

        public void HandleAddInventorySlot(InventorySlot inventorySlot) {
            //Debug.Log("PlayerManager.HandleAddInventorySlot()");

            systemEventManager.NotifyOnAddInventorySlot(inventorySlot);
        }

        public void HandleAddBankSlot(InventorySlot inventorySlot) {
            //Debug.Log("PlayerManager.HandleAddBankSlot()");

            systemEventManager.NotifyOnAddBankSlot(inventorySlot);
        }

        public void HandleRemoveInventorySlot(InventorySlot inventorySlot) {
            systemEventManager.NotifyOnRemoveInventorySlot(inventorySlot);
        }

        public void HandleRemoveBankSlot(InventorySlot inventorySlot) {
            systemEventManager.NotifyOnRemoveBankSlot(inventorySlot);
        }

        public void HandleCombatMessage(string messageText) {
            messageLogClient.WriteCombatMessage(messageText);
        }

        public void HandleCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(targetObject, 0, CombatTextType.miss, CombatMagnitude.normal, abilityEffectContext);
        }

        public void HandleActivateTargetingMode(AbilityProperties baseAbility) {
            castTargettingManager.EnableProjector(baseAbility);
        }

        public void HandleAbilityActionCheckFail(AbilityProperties baseAbility) {
            if (PlayerUnitSpawned == true && messageLogClient != null) {
                messageLogClient.WriteCombatMessage($"Cannot use {(baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName)}. Waiting for another ability to finish.");
            }
        }

        public void HandleLearnAbility(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"PlayerManager.HandleLearnAbility({baseAbility.ResourceName})");

            systemEventManager.NotifyOnAbilityListChanged(sourceUnitController, baseAbility);
            // this is ok to have here for now because prerequisites are only used on the client
            baseAbility.NotifyOnLearn(sourceUnitController);
        }

        /*
        public void HandleCombatUpdate() {
            //Debug.Log("PlayerManager.HandleCombatUpdate()");

            activeUnitController.CharacterCombat.HandleAutoAttack();
        }
        */

        public void HandleDropCombat() {
            //Debug.Log("PlayerManager.HandleDropCombat()");

            if (messageLogClient != null) {
                messageLogClient.WriteCombatMessage("Left combat");
            }
        }

        public void HandleEnterCombat(Interactable interactable) {
            if (messageLogClient != null) {
                messageLogClient.WriteCombatMessage("Entered combat with " + interactable.DisplayName);
            }
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            //Debug.Log("PlayerManager.HandleReputationChange");

            systemEventManager.NotifyOnReputationChange(sourceUnitController);
        }

        public void HandleTargetInAbilityRangeFail(AbilityProperties baseAbility, Interactable target) {
            if (baseAbility != null && messageLogClient != null) {
                messageLogClient.WriteCombatMessage(target.name + " is out of range of " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName));
            }
        }

        public void HandleCombatCheckFail(AbilityProperties ability) {
            messageLogClient.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while out of combat");
        }

        public void HandleStealthCheckFail(AbilityProperties ability) {
            messageLogClient.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while while stealthed");
        }

        public void HandlePowerResourceCheckFail(AbilityProperties ability, IAbilityCaster abilityCaster) {
            messageLogClient.WriteCombatMessage("Not enough " + ability.PowerResource.DisplayName + " to perform " + ability.DisplayName + " at a cost of " + ability.GetResourceCost(abilityCaster));
        }

        public void HandleLearnedCheckFail(AbilityProperties ability) {
            messageLogClient.WriteCombatMessage("You have not learned the ability " + ability.DisplayName + " yet");
        }

        private void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            systemEventManager.NotifyOnAddEquipment(profile, equipment);
        }

        private void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            systemEventManager.NotifyOnRemoveEquipment(profile, equipment);
        }

        public void HandleRecoverResource(PowerResource powerResource, int amount, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            if (messageLogClient != null) {
                messageLogClient.WriteCombatMessage($"You gain {amount} {powerResource.DisplayName}");
            }
            combatTextManager.SpawnCombatText(activeUnitController, amount, CombatTextType.gainResource, combatMagnitude, abilityEffectContext);
        }

        public void HandleResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            actionBarManager.UpdateVisuals();
        }

        public void HandleStatusEffectAdd(UnitController sourceUnitController, StatusEffectNode statusEffectNode) {
            //Debug.Log("PlayerManager.HandleStatusEffectAdd()");

            if (statusEffectNode == null) {
                return;
            }

            if (statusEffectNode.StatusEffect.ClassTrait == false && activeUnitController != null) {
                if (statusEffectNode.AbilityEffectContext.savedEffect == false) {
                    if (activeUnitController.CharacterUnit != null) {
                        combatTextManager.SpawnCombatText(activeUnitController, statusEffectNode.StatusEffect, true);
                    }
                }
            }

        }

        public void HandleGainXP(UnitController unitController, int gainedXP, int currentXP) {
            if (messageLogClient != null) {
                messageLogClient.WriteSystemMessage($"You gain {gainedXP} experience");
            }
            if (activeUnitController != null) {
                combatTextManager.SpawnCombatText(activeUnitController, gainedXP, CombatTextType.gainXP, CombatMagnitude.normal, null);
            }
            SystemEventManager.TriggerEvent("OnXPGained", new EventParamProperties());
        }

        public void HandleLevelChanged(int newLevel) {
            systemEventManager.NotifyOnLevelChanged(unitController, newLevel);
            messageFeedManager.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", newLevel.ToString()));
        }

        public void HandleReviveComplete(UnitController sourceUnitController) {
            SystemEventManager.TriggerEvent("OnReviveComplete", new EventParamProperties());
            if (activeUnitController != null) {
                activeUnitController.UnitAnimator.SetCorrectOverrideController();
            }
        }

        public void HandleReviveBegin(float reviveTime) {
            playerController.HandleReviveBegin(reviveTime);
        }

        public void HandleBeforeDie(UnitController deadUnitController) {
            DeathActions();
            systemEventManager.NotifyOnPlayerDeath();
        }

        private void DeathActions() {
            playerController.HandleDie();
            uIManager.PlayerDeathHandler(unitController);
        }

        public void HandleAfterDie(CharacterStats deadCharacterStats) {
        }

        public void HandleImmuneToEffect(AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(activeUnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            systemEventManager.NotifyOnClassChange(sourceUnitController, newCharacterClass, oldCharacterClass);
            messageFeedManager.WriteMessage("Changed class to " + newCharacterClass.DisplayName);
        }

        public void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            systemEventManager.NotifyOnSpecializationChange(sourceUnitController, newSpecialization, oldSpecialization);
            if (newSpecialization != null) {
                messageFeedManager.WriteMessage("Changed specialization to " + newSpecialization.DisplayName);
            }
        }


        public void RequestSpawnPet(UnitProfile unitProfile) {
            if (systemGameManager.GameMode == GameMode.Local) {
                playerManagerServer.SpawnPet(unitController, unitProfile);
            } else {
                networkManagerClient.RequestSpawnPet(unitProfile);
            }
        }

        public void RequestDespawnPet(UnitProfile unitProfile) {
            if (systemGameManager.GameMode == GameMode.Local) {
                playerManagerServer.DespawnPet(unitController, unitProfile);
            } else {
                networkManagerClient.RequestDespawnPet(unitProfile);
            }
        }

        /*
        // this spawn request is only used for the camera positioning.
        // the spawn request that controls the player position is handed by the playerManagerServer
        public void AddSpawnRequest(int accountId, SpawnPlayerRequest spawnPlayerRequest) {
            this.spawnPlayerRequest = spawnPlayerRequest;
        }
        */
    }

}