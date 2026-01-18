using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnNode : AutoConfiguredMonoBehaviour, IPrerequisiteOwner, ICharacterRequestor {

        [Header("Spawn GameObject")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private List<string> unitProfileNames = new List<string>();

        private List<UnitProfile> unitProfiles = new List<UnitProfile>();

        [Header("Unit Level and Toughness")]

        [SerializeField]
        private bool dynamicLevel = true;

        [SerializeField]
        private int unitLevel = 1;

        [Tooltip("The number of extra levels above the normal level for this mob")]
        [SerializeField]
        private int extraLevels = 0;

        [Tooltip("If a unit has no toughness set, this toughness will be used.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitToughness))]
        private string defaultToughness = string.Empty;

        [Header("Timers")]

        [Tooltip("Spawn time for regular mob spawns, such as when prerequisites are updated, or the spawner supports multiple units and they should not all spawn at once.")]
        [SerializeField]
        private int spawnTimer = 0;

        [Tooltip("An additional delay to add to the timer.  meant to allow an offset for multiple spawners of the same type.")]
        [SerializeField]
        private int spawnDelay = 0;

        [Tooltip("An extra delay from the time the Destroy GameObject call is made before it is actually destroyed.")]
        [SerializeField]
        private float despawnDelay = 0f;

        [Tooltip("A separate spawn timer for when mob despawns are detected to give players longer to move away being a mob attacks them again. -1 disables respawning of despawned units.")]
        [FormerlySerializedAs("despawnTimer")]
        [SerializeField]
        private int respawnTimer = 60;

        [Tooltip("On which event should the respawn timer be started.")]
        [SerializeField]
        private respawnCondition respawnOn = respawnCondition.Despawn;

        [Header("Options")]

        [Tooltip("The maximum number of units that can be active at once.  Once this limit is reached, spawns will be paused until a unit dies. Set to -1 to do infinite spawns.")]
        [SerializeField]
        private int maxUnits = 1;

        [Tooltip("Set to true to allow for unit spawn control panels to use this node.")]
        [SerializeField]
        private bool suppressAutoSpawn = false;

        [Tooltip("If true, ignore spawn timers and use trigger instead.")]
        [SerializeField]
        private bool triggerBased = false;

        [Tooltip("The number of times this object can be triggered.  0 is unlimited.")]
        [SerializeField]
        private int triggerLimit = 0;

        // keep track of the number of times this switch has been activated
        private int triggerCount = 0;


        //[SerializeField]
        //private bool areaBased = false;

        [Tooltip("In area mode, the number of mobs per square meter to spawn.")]
        [SerializeField]
        private float spawnDensity = 0.01f;

        [Tooltip("If true, units will spawn at the pivot of the Unit Spawn Node.")]
        [SerializeField]
        private bool pointBased = true;

        [Tooltip("If true, and there are units spawned, and the prerequisite conditions are no longer met, despawn them.")]
        [SerializeField]
        private bool forceDespawnUnits = false;

        [Tooltip("If true, the spawned unit will be parented to the current parent transform of the unit spawn node.")]
        [SerializeField]
        private bool parentUnit = false;

        [Header("Prerequisites")]

        [Tooltip("conditions must be met for spawner to spawn nodes")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        protected UnitToughness unitToughness = null;

        private Coroutine countDownRoutine = null;

        private Coroutine delayRoutine = null;

        protected bool eventSubscriptionsInitialized = false;
        protected bool serverEventSubscriptionsInitialized = false;

        protected bool disabled = false;

        // keep track of spawn requests so that they can be configured after spawning
        //private Dictionary<int, UnitSpawnNodeSpawnRequestData> spawnRequests = new Dictionary<int, UnitSpawnNodeSpawnRequestData>();

        private List<UnitController> spawnReferences = new List<UnitController>();

        // game manager references
        private PlayerManager playerManager = null;
        private SystemDataFactory systemDataFactory = null;
        private CharacterManager characterManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private LevelManager levelManager = null;
        private PlayerManagerServer playerManagerServer = null;

        // later on make this spawn mob as player walks into collider ;>
        //private BoxCollider boxCollider;
        public bool PrerequisitesMet(UnitController sourceUnitController) {
            if (prerequisiteConditions.Count > 0 && sourceUnitController == null) {
                // this is not a player triggered check, and there are prerequisites, so we cannot spawn
                return false;
            }
            foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }

        public string DisplayName {
            get {
                return gameObject.name;
            }
        }

        public List<string> UnitProfileNames { get => unitProfileNames; set => unitProfileNames = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.Configure()");

            base.Configure(systemGameManager);
            bool isCutscene = false;
            if (networkManagerServer.ServerModeActive == false) {
                isCutscene = levelManager.IsCutscene();
            }
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && isCutscene == false) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.Configure() setting not active");
                gameObject.SetActive(false);
                return;
            }
            SetupScriptableObjects();

            if (systemGameManager.GameMode == GameMode.Local) {
                CreateEventSubscriptions();
            }/* else if (networkManagerServer.ServerModeActive) {
                CreateServerEventSubscriptions();
            }
            */

            if (networkManagerServer.ServerModeActive || isCutscene == true || systemGameManager.GameMode == GameMode.Local) {
                // network client should never spawn
                CheckPrerequisites(null);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            characterManager = systemGameManager.CharacterManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManager = systemGameManager.LevelManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CreateEventSubscriptions()");

            if (eventSubscriptionsInitialized) {
                return;
            }
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): systemGameManager not found.  Is the GameManager in the scene?");
                return;
            }
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnLevelUnloadClient += HandleLevelUnloadClient;
            if (playerManager.PlayerUnitSpawned == true) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.CreateEventSubscriptions(): player unit already spawned.  Handling player unit spawn");
                ProcessPlayerUnitSpawn(playerManager.UnitController);
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }

            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            systemEventManager.OnLevelUnloadClient -= HandleLevelUnloadClient;

            eventSubscriptionsInitialized = false;
        }

        /*
        private void CreateServerEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CreateServerEventSubscriptions()");

            if (serverEventSubscriptionsInitialized) {
                return;
            }
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): systemGameManager not found.  Is the GameManager in the scene?");
                return;
            }
            systemEventManager.OnLevelUnloadServer += HandleLevelUnloadServer;
            serverEventSubscriptionsInitialized = true;
        }

        private void CleanupServerEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CleanupServerEventSubscriptions()");
            if (!serverEventSubscriptionsInitialized) {
                return;
            }

            systemEventManager.OnLevelUnloadServer -= HandleLevelUnloadServer;

            serverEventSubscriptionsInitialized = false;
        }
        */


        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandlePlayerUnitSpawn()");

            ProcessPlayerUnitSpawn(sourceUnitController);
        }

        /*
        public void HandleLevelUnloadServer(int sceneHandle, string sceneName) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleLevelUnload({sceneHandle}, {sceneName})");
            if (gameObject.scene.handle != sceneHandle) {
                return;
            }
            Cleanup();
        }
        */

        public void HandleLevelUnloadClient(int sceneHandle, string sceneName) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleLevelUnload({sceneHandle}, {sceneName})");
            Cleanup();
        }

        public void ProcessPlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.ProcessPlayerUnitSpawn({sourceUnitController.gameObject.name})");

            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, false);
                    }
                }
                if (PrerequisitesMet(sourceUnitController)) {
                    HandlePrerequisiteUpdates(sourceUnitController);
                }
            } else {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandlePlayerUnitSpawn(): no prerequisites found.");
                HandlePrerequisiteUpdates(sourceUnitController);
            }
            //HandlePrerequisiteUpdates();
        }


        public void Cleanup() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.Cleanup()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            //CleanupServerEventSubscriptions();
            StopAllCoroutines();
            countDownRoutine = null;
            delayRoutine = null;
            CleanupScriptableObjects();
            disabled = true;
        }

        public void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandlePrerequisiteUpdates()");
            CheckPrerequisites(sourceUnitController);
        }

        public void CheckPrerequisites(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CheckPrerequisites()");

            if (PrerequisitesMet(sourceUnitController) && !triggerBased) {
                SpawnWithDelay(sourceUnitController);
            }
            if (forceDespawnUnits && !PrerequisitesMet(sourceUnitController)) {
                StartCoroutine(DestroySpawnsAtEndOfFrame());
            }
        }

        private int GetMaxUnits() {
            //Debug.Log("UnitSpawnNode.GetMaxUnits()");
            if (pointBased || triggerBased) {
                return maxUnits;
            }

            // area based
            //Debug.Log("UnitSpawnNode.GetMaxUnits(): area based");
            float spawnAreaSize = transform.localScale.x * transform.localScale.z;
            int returnValue = Mathf.Clamp((int)(spawnAreaSize * spawnDensity), 0, 100);
            //Debug.Log("UnitSpawnNode.GetMaxUnits(): return: " + returnValue);
            return returnValue;
        }

        private Vector3 GetSpawnLocation() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.GetSpawnLocation()");

            if (pointBased || triggerBased) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.GetSpawnLocation(): returning: " + transform.position);
                return transform.position;
            }

            // area based
            Vector3 center = transform.position;
            float xSize = transform.localScale.x;
            float zSize = transform.localScale.z;
            float ySize = transform.localScale.y;
            //Debug.Log("UnitSpawnNode.GetSpawnLocation(). center: " + center + "; xsize: " + xSize + "; ySize: " + ySize + "; zsize: " + zSize);
            bool gotLocation = false;
            // circuit breaker to prevent infinite loop
            int counter = 0;
            float xLocation = 0f;
            float zLocation = 0f;
            float yLocation = 0f;
            Vector3 spawnLocation = Vector3.zero;
            while (gotLocation == false) {
                xLocation = UnityEngine.Random.Range(0, xSize) - (xSize / 2);
                //Debug.Log("xLocation: " + xLocation);
                zLocation = UnityEngine.Random.Range(0, zSize) - (zSize / 2);
                //Debug.Log("zLocation: " + zLocation);
                yLocation = center.y + (ySize / 2);
                //Debug.Log("yLocation: " + yLocation);
                Vector3 tempVector = new Vector3(transform.position.x + xLocation, transform.position.y + (ySize / 2), transform.position.z + zLocation);
                Vector3 attemptVector = tempVector;
                RaycastHit hit;
                if (PhysicsScene.Raycast(attemptVector, Vector3.down, out hit, 500f, playerManager.PlayerController.movementMask)) {
                    gotLocation = true;
                    spawnLocation = hit.point;
                    //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
                } else {
                    //Debug.Log("We did not hit anything walkable!");
                }
                counter++;
                if (counter == 200) {
                    Debug.LogWarning("HIT CIRCUIT BREAKER!");
                    break;
                }
            }
            //Debug.Log("Returning: " + spawnLocation);
            //return transform.TransformPoint(spawnLocation);
            return spawnLocation;
        }

        public void ManualSpawn(int unitLevel, int extraLevels, bool dynamicLevel, UnitProfile unitProfile, UnitToughness toughness, UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.ManualSpawn({unitLevel}, {extraLevels}, {dynamicLevel}, {unitProfile.ResourceName})");

            CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfile, toughness, sourceUnitController);
        }

        // allow for trigger from cutscene
        public void Spawn() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.Spawn()");

            Spawn(null);
        }

        public void Spawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.Spawn(): GetMaxUnits(): {GetMaxUnits()}");

            if (unitProfiles.Count == 0) {
                return;
            }
            if (CanTriggerSpawn(sourceUnitController)) {
                int spawnIndex = UnityEngine.Random.Range(0, unitProfiles.Count);
                if (unitProfiles[spawnIndex] != null) {
                    CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfiles[spawnIndex], unitToughness, sourceUnitController);
                }
            } else {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.Spawn(): cannot trigger spawn.  CanTriggerSpawn() is false.");
            }

        }

        public int GetUnspawnedPlayerLevel(int defaultLevel) {
            if (playerManagerServer.PlayerCharacterMonitors.Count > 0) {
                return playerManagerServer.PlayerCharacterMonitors.First().Value.characterSaveData.CharacterLevel;
            }
            return defaultLevel;
        }

        public void CommonSpawn(int unitLevel, int extraLevels, bool dynamicLevel, UnitProfile unitProfile, UnitToughness toughness, UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.CommonSpawn({unitLevel}, {extraLevels}, {dynamicLevel}, {unitProfile.ResourceName})");

            // prevent a coroutine that finished during a level load from spawning a character
            if (disabled == true) {
                return;
            }
            if (unitProfile == null ) {
                return;
            }
            /*
            if (playerManager.UnitController == null && systemGameManager.GameMode == GameMode.Local && levelManager.IsCutscene() == false) {
                return;
            }
            */

            int _unitLevel = unitLevel;
            if (sourceUnitController != null) {
                _unitLevel = (dynamicLevel ? sourceUnitController.CharacterStats.Level : unitLevel) + extraLevels;
            } else if (systemGameManager.GameMode == GameMode.Local) {
                _unitLevel = (dynamicLevel ? GetUnspawnedPlayerLevel(unitLevel) : unitLevel) + extraLevels;
            }
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = _unitLevel;
            characterConfigurationRequest.unitToughness = (toughness != null ? toughness : unitProfile.DefaultToughness);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.AI;
            CharacterRequestData characterRequestData = new CharacterRequestData(this,
                GameMode.Local, 
                characterConfigurationRequest
                );
            characterRequestData.characterId = characterManager.GetNewCharacterId(UnitControllerMode.AI);
            UnitController unitController = null;
            if (networkManagerServer.ServerModeActive == true) {
                characterRequestData.isServerOwned = true;
                characterRequestData.requestMode = GameMode.Network;
                unitController = characterManager.SpawnUnitPrefab(characterRequestData, null, transform.position, transform.forward, gameObject.scene);
            } else {
                unitController = characterManager.SpawnUnitPrefabLocal(characterRequestData, (parentUnit == false ? null : transform.parent), transform.position, transform.forward);
            }
            if (unitController != null) {
                spawnReferences.Add(unitController);
            }
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.ConfigureSpawnedCharacter()");

            //if (spawnRequests.ContainsKey(characterRequestData.spawnRequestId) == false) {
            //    return;
            //}
            //UnitToughness toughness = spawnRequests[characterRequestData.spawnRequestId].unitToughness;
            // clean up the unneeded request data
            //spawnRequests.Remove(characterRequestData.spawnRequestId);

            Vector3 newSpawnLocation = Vector3.zero;
            Vector3 newSpawnForward = Vector3.forward;

            // lookup persistent position, or use navmesh agent to get a valid position (in case this spawner was not placed on walkable terrain)
            if (unitController.PersistentObjectComponent.PersistObjectPosition == true) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.CommonSpawn(): persist ojbect position is true on {unitController.gameObject.name}");

                PersistentState persistentState = unitController.PersistentObjectComponent.GetPersistentState();
                if (persistentState != null) {
                    // since we will be using navMeshAgent.warp, do not attempt to move unit manually
                    unitController.PersistentObjectComponent.MoveOnStart = false;
                    newSpawnLocation = persistentState.Position;
                    newSpawnForward = persistentState.Forward;
                } else {
                    newSpawnLocation = GetSpawnLocation();
                    newSpawnForward = transform.forward;
                }
            } else {
                newSpawnLocation = GetSpawnLocation();
                newSpawnForward = transform.forward;
            }

            // now that we have a good final position and rotation, set it
            unitController.StartPosition = newSpawnLocation;
            unitController.NavMeshAgent.Warp(newSpawnLocation);
            unitController.transform.forward = newSpawnForward;


            //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
            //CharacterUnit characterUnit = unitController.CharacterUnit;

            if (respawnOn == respawnCondition.Despawn) {
                unitController.UnitEventController.OnDespawn += HandleDespawn;
            } else if (respawnOn == respawnCondition.Loot) {
                LootableCharacterComponent tmpLootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(unitController);
                if (tmpLootableCharacter != null) {
                    // there can only be one of these types of object on an interactable
                    // interesting note : there is no unsubscribe to this event.  Unit spawn nodes exist for the entire scene and are only destroyed at the same time as the interactables
                    // should we make an unsubscribe anyway even though it would never be called?
                    tmpLootableCharacter.OnLootComplete += HandleLootComplete;
                }

            } else if (respawnOn == respawnCondition.Death) {
                unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
            }
            
            /*
            // don't override an existing toughness
            if (unitController.BaseCharacter.UnitToughness == null && toughness != null) {
                //Debug.Log("UnitSpawnNode.Spawn(): setting toughness to null on gameObject: " + spawnReference.name);
                unitController.BaseCharacter.SetUnitToughness(toughness, true);
            }
            */
            //spawnReferences.Add(unitController);
        }

        public void PostInit(UnitController unitController) {
        }

        /// <summary>
        /// if the maximum unit count is not exceeded and the prerequisites are met, return true
        /// </summary>
        /// <returns></returns>
        private bool CanTriggerSpawn(UnitController sourceUnitController) {

            if ((spawnReferences.Count < GetMaxUnits() || GetMaxUnits() == -1) && PrerequisitesMet(sourceUnitController)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// spawn a unit if the trigger conditions are met, then start the countdown for the next spawn
        /// </summary>
        private void SpawnWithDelay(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.SpawnWithDelay()");

            if (suppressAutoSpawn) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.SpawnWithDelay(): suppressAutoSpawn is true, not spawning.");
                return;
            }
            if (unitProfiles.Count == 0) {
                return;
            }
            // this method is necessary to ensure that the main spawn count cycle is followed and the delay does not get directly added to the restart time
            if (CanTriggerSpawn(sourceUnitController)) {
                // if the countdown routine is not null, then there is already a regular spawn count or respawn count in progress
                if (countDownRoutine == null) {

                    // if the delay routine is not null, then there is already a spawn delay in progress so we do not want to start another one
                    if (delayRoutine == null) {
                        delayRoutine = StartCoroutine(StartSpawnDelayCountDown());
                    }

                    // now that we have spawned the mob (or at least started its additional delay timer), we will start the regular countdown
                    if (CanTriggerCountdown(sourceUnitController)) {
                        countDownRoutine = StartCoroutine(StartSpawnCountdown(spawnTimer));
                    }
                }
            } else {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.SpawnWithDelay(): cannot trigger spawn.  CanTriggerSpawn() is false.");
            }
        }

        public bool CanTriggerCountdown(UnitController sourceUnitController) {
            if (delayRoutine == null && CanTriggerSpawn(sourceUnitController)) {
                return true;
            }
            if (delayRoutine != null && GetMaxUnits() > (spawnReferences.Count + 1)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Perform a spawn delay after Spawn() is called and before the unit actually spawns
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartSpawnDelayCountDown() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.StartSpawnDelayCountDown()");

            float currentDelayTimer = spawnDelay;

            // add a one frame delay to allow cutscenes to start
            yield return null;

            while (currentDelayTimer > 0) {
                //Debug.Log("UnitSpawnNode.Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentDelayTimer -= 1;
            }
            // clearing the coroutine so the next round can start
            delayRoutine = null;
            if (disabled == false) {
                if (systemGameManager.GameMode == GameMode.Local && playerManager.UnitController != null) {
                    Spawn(playerManager.UnitController);
                } else {
                    Spawn(null);
                }
            }
        }

        /// <summary>
        /// wait for the end of the frame to destroy spawns to avoid null references in the middle of a chain of prerequisite checks
        /// </summary>
        /// <returns></returns>
        private IEnumerator DestroySpawnsAtEndOfFrame() {
            yield return new WaitForEndOfFrame();
            DestroySpawns();
        }

        public void DestroySpawns() {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.DestroySpawn(): Destroying spawns");
            List<UnitController> tmpSpawnReferences = new List<UnitController>(spawnReferences);
            foreach (UnitController unitController in tmpSpawnReferences) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.DestroySpawn(): Destroying spawn: " + unitController.gameObject.name + "; delay: " + despawnDelay);
                unitController.Despawn(despawnDelay, false, true);
                //ObjectPooler.Instance.ReturnObjectToPool(unitController.gameObject, despawnDelay);
            }
            spawnReferences.Clear();
        }

        /// <summary>
        /// perform a countdown before calling Spawn()
        /// </summary>
        /// <param name="countdownTime"></param>
        /// <returns></returns>
        private IEnumerator StartSpawnCountdown(int countdownTime) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.StartSpawnCountdown({countdownTime})");

            float currentTimer = countdownTime;

            // single frame delay to prevent spawning more units in a single frame than the stack size, which would cause a stack overflow
            yield return null;

            while (currentTimer > 0) {
                //Debug.Log("UnitSpawnNode.Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            //clearing the coroutine so the next timer will be allowed to start
            countDownRoutine = null;
            if (disabled == false) {
                if (systemGameManager.GameMode == GameMode.Local && playerManager.UnitController != null) {
                    SpawnWithDelay(playerManager.UnitController);
                } else {
                    SpawnWithDelay(null);
                }
            }
        }

        public void ProcessRespawn(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.ProcessRespawn()");
            if (spawnReferences.Contains(unitController)) {
                spawnReferences.Remove(unitController);
            }
            if (respawnTimer > -1) {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; starting despawn countdown");
                if (countDownRoutine == null) {
                    countDownRoutine = StartCoroutine(StartSpawnCountdown(respawnTimer));
                } else {
                    //Debug.Log("Countdown routine already in progress, not starting new countdown");
                }
            } else {
                //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; NOT STARTING DESPAWN COUNTDOWN");
            }
        }

        public void HandleDespawn(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleDespawn()");
            if (respawnOn != respawnCondition.Despawn) {
                return;
            }
            ProcessRespawn(unitController);
        }

        public void HandleLootComplete(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleLootComplete(): timer: " + respawnTimer);
            if (respawnOn != respawnCondition.Loot) {
                return;
            }
            ProcessRespawn(unitController);
        }

        public void HandleBeforeDie(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitSpawnNode.HandleDie(): timer: " + respawnTimer);
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
            if (respawnOn != respawnCondition.Death) {
                return;
            }
            // this works because unit spawn nodes do not spawn players.  If they ever do, this will need to reference stats->unit->gameobject instead
            ProcessRespawn(unitController);
        }

        public void OnTriggerEnter(Collider other) {
            if (!triggerBased) {
                return;
            }

            // only players can activate trigger based unit spawn nodes.  we don't want npcs wandering around patrolling to activate these
            if (playerManagerServer.ActivePlayerGameObjects.ContainsKey(other.gameObject) == false) {
                return;
            }

            if (triggerLimit > 0 && triggerCount >= triggerLimit) {
                // this has already been activated the number of allowed times
                return;
            }
            triggerCount++;

            // already in the middle of spawning.  do nothing
            if (countDownRoutine != null) {
                return;
            }

            // all check passed, safe to spawn
            Spawn(other.GetComponent<UnitController>());
        }

        private void OnDestroy() {
            Cleanup();
        }

        public void SetupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        //Debug.Log($"{gameObject.name}.SetupScriptableObjects(): setting up prerequisites");
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);

                        // add this so unit spawn nodes can have their prerequisites properly set on the first check
                        //tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
            }

            unitProfiles = new List<UnitProfile>();
            if (unitProfileNames != null) {
                foreach (string unitProfileName in unitProfileNames) {
                    if (unitProfileName != null && unitProfileName != string.Empty) {
                        UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                        if (unitProfile != null) {
                            unitProfiles.Add(unitProfile);
                        } else {
                            Debug.LogError(gameObject.name + ": Unit Profile: " + unitProfileName + " not found while initializing Unit Spawn Node.  Check Inspector!");
                        }
                    }
                }
            }

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = systemDataFactory.GetResource<UnitToughness>(defaultToughness);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError("Unit Toughness: " + defaultToughness + " not found while initializing Unit Profiles.  Check Inspector!");
                }
            }

        }

        public void CleanupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }
        }

    }

    /*
    public class UnitSpawnNodeSpawnRequestData {
        public UnitToughness unitToughness;
        public UnitSpawnNodeSpawnRequestData(UnitToughness unitToughness) {
            this.unitToughness = unitToughness;
        }
    }
    */

    public enum respawnCondition { Despawn, Loot, Death };

}