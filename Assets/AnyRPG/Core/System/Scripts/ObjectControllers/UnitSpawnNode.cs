using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnNode : AutoConfiguredMonoBehaviour, IPrerequisiteOwner {

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

        [Tooltip("spawn time for regular mob spawns, such as when prerequisites are updated, or the spawner supports multiple units and they should not all spawn at once")]
        [SerializeField]
        private int spawnTimer = 0;

        [Tooltip("an additional delay to add to the timer.  meant to allow an offset for multiple spawners of the same type")]
        [SerializeField]
        private int spawnDelay = 0;

        [Tooltip("An extra delay from the time the Destroy GameObject call is made before it is actually destroyed.")]
        [SerializeField]
        private float despawnDelay = 0f;

        [Tooltip("a separate spawn timer for when mob despawns are detected to give players longer to move away being a mob attacks them again, -1 disables respawning of despawned units")]
        [FormerlySerializedAs("despawnTimer")]
        [SerializeField]
        private int respawnTimer = 60;

        [Tooltip("On which event should the respawn timer be started.")]
        [SerializeField]
        private respawnCondition respawnOn = respawnCondition.Despawn;

        [Header("Options")]

        [Tooltip("The maximum number of units that can be active at once.  Once this limit is reached, spawns will be paused until a unit dies. set to -1 to do infinite spawns")]
        [SerializeField]
        private int maxUnits = 1;

        [Tooltip("to allow for unit spawn control panels to use this node")]
        [SerializeField]
        private bool suppressAutoSpawn = false;

        [Tooltip("ignore spawn timers and use trigger instead")]
        [SerializeField]
        private bool triggerBased = false;

        [Tooltip("The number of times this object can be triggered.  0 is unlimited")]
        [SerializeField]
        private int triggerLimit = 0;

        // keep track of the number of times this switch has been activated
        private int triggerCount = 0;


        //[SerializeField]
        //private bool areaBased = false;

        [Tooltip("in area mode, the number of mobs per square meter to spawn")]
        [SerializeField]
        private float spawnDensity = 0.01f;

        [Tooltip("Spawn at the pivot of the UnitSpawnNode")]
        [SerializeField]
        private bool pointBased = true;

        [Tooltip("if there are units spawned, and the prerequisite conditions are no longer met, despawn them")]
        [SerializeField]
        private bool forceDespawnUnits = false;

        [Header("Prerequisites")]

        [Tooltip("conditions must be met for spawner to spawn nodes")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        protected UnitToughness unitToughness = null;

        private Coroutine countDownRoutine = null;

        private Coroutine delayRoutine = null;

        protected bool eventSubscriptionsInitialized = false;

        protected bool disabled = false;

        private List<UnitController> spawnReferences = new List<UnitController>();

        // game manager references
        private PlayerManager playerManager = null;
        private SystemDataFactory systemDataFactory = null;

        // later on make this spawn mob as player walks into collider ;>
        //private BoxCollider boxCollider;
        public bool MyPrerequisitesMet {
            get {
                // disabled next bit because it interferes with spawning in cutscenes
                /*
                if (playerManager.MyPlayerUnitSpawned == false) {
                    //Debug.Log(gameObject.name + ".MyPrerequisitesMet: returning false because player isn't spawned");
                    return false;
                }
                */
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    //Debug.Log(gameObject.name + ".MyPrerequisitesMet: checking prerequisite");
                    if (!prerequisiteCondition.IsMet()) {
                        //Debug.Log(gameObject.name + ".MyPrerequisitesMet: returning false");
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                //Debug.Log(gameObject.name + ".MyPrerequisitesMet: returning true");
                return true;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Configure()");
            base.Configure(systemGameManager);

            SetupScriptableObjects();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Start()");
            //boxCollider = GetComponent<BoxCollider>();
            if (!triggerBased) {
                SpawnWithDelay();
            }
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): systemGameManager not found.  Is the GameManager in the scene?");
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnLeveOnLevelUnloadlUnload", HandleLevelUnload);
            if (playerManager.PlayerUnitSpawned == true) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): player unit already spawned.  Handling player unit spawn");
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }

            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);

            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            Debug.Log(gameObject.name + ".UnitSpawnNode.HandleLevelUnload()");
            Cleanup();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandlePlayerUnitSpawn()");
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
                if (MyPrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                }
            } else {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.HandlePlayerUnitSpawn(): no prerequisites found.");
                HandlePrerequisiteUpdates();
            }
            //HandlePrerequisiteUpdates();
        }


        public void Cleanup() {
            Debug.Log(gameObject.name + ".UnitSpawnNode.Cleanup()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            StopAllCoroutines();
            countDownRoutine = null;
            delayRoutine = null;
            CleanupScriptableObjects();
            disabled = true;
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandlePrerequisiteUpdates()");
            CheckPrerequisites();
        }

        public void CheckPrerequisites() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.CheckPrerequisites()");
            if (MyPrerequisitesMet && !triggerBased) {
                SpawnWithDelay();
            }
            if (forceDespawnUnits && !MyPrerequisitesMet) {
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
            //Debug.Log(gameObject.name + ".UnitSpawnNode.GetSpawnLocation()");

            if (pointBased || triggerBased) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.GetSpawnLocation(): returning: " + transform.position);
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
                if (Physics.Raycast(attemptVector, Vector3.down, out hit, 500f, playerManager.PlayerController.movementMask)) {
                    gotLocation = true;
                    spawnLocation = hit.point;
                    //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
                } else {
                    //Debug.Log("We did not hit anything walkable!");
                }
                counter++;
                if (counter == 200) {
                    Debug.Log("HIT CIRCUIT BREAKER!");
                    break;
                }
            }
            //Debug.Log("Returning: " + spawnLocation);
            //return transform.TransformPoint(spawnLocation);
            return spawnLocation;
        }

        public void ManualSpawn(int unitLevel, int extraLevels, bool dynamicLevel, UnitProfile unitProfile, UnitToughness toughness) {
            CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfile, toughness);
        }

        public void Spawn() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Spawn(): GetMaxUnits(): " + GetMaxUnits());
            if (unitProfiles.Count == 0) {
                return;
            }
            if (CanTriggerSpawn()) {
                int spawnIndex = UnityEngine.Random.Range(0, unitProfiles.Count);
                if (unitProfiles[spawnIndex] != null) {
                    CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfiles[spawnIndex], unitToughness);
                }
            }
        }

        public void CommonSpawn(int unitLevel, int extraLevels, bool dynamicLevel, UnitProfile unitProfile, UnitToughness toughness = null) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.CommonSpawn()");

            // prevent a coroutine that finished during a level load from spawning a character
            if (disabled == true) {
                return;
            }
            if (unitProfile == null || playerManager.MyCharacter == null) {
                return;
            }

            int _unitLevel = (dynamicLevel ? playerManager.MyCharacter.CharacterStats.Level : unitLevel) + extraLevels;
            UnitController unitController = unitProfile.SpawnUnitPrefab(null, transform.position, transform.forward, UnitControllerMode.AI, _unitLevel);

            if (unitController == null) {
                // something went wrong.  None of the code below will work, so might as well return
                return;
            }

            Vector3 newSpawnLocation = Vector3.zero;
            Vector3 newSpawnForward = Vector3.forward;

            // lookup persistent position, or use navmesh agent to get a valid position (in case this spawner was not placed on walkable terrain)
            if (unitController.PersistentObjectComponent.PersistObjectPosition == true) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.CommonSpawn(): persist ojbect position is true");
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
            unitController.MyStartPosition = newSpawnLocation;
            unitController.NavMeshAgent.Warp(newSpawnLocation);
            unitController.transform.forward = newSpawnForward;


            //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
            CharacterUnit _characterUnit = null;
            CharacterUnit tmpCharacterUnit = unitController.CharacterUnit;
            if (tmpCharacterUnit == null) {
                Debug.LogError("Interactable had no characterUnit");
                return;
            }
            _characterUnit = tmpCharacterUnit;

            if (respawnOn == respawnCondition.Despawn) {
                _characterUnit.OnDespawn += HandleDespawn;
            } else if (respawnOn == respawnCondition.Loot) {
                LootableCharacterComponent tmpLootableCharacter = LootableCharacterComponent.GetLootableCharacterComponent(unitController);
                if (tmpLootableCharacter != null) {
                    // there can only be one of these types of object on an interactable
                    // interesting note : there is no unsubscribe to this event.  Unit spawn nodes exist for the entire scene and are only destroyed at the same time as the interactables
                    // should we make an unsubscribe anyway even though it would never be called?
                    tmpLootableCharacter.OnLootComplete += HandleLootComplete;
                }

            } else if (respawnOn == respawnCondition.Death) {
                if (_characterUnit.BaseCharacter != null && _characterUnit.BaseCharacter.CharacterStats != null) {
                    _characterUnit.BaseCharacter.CharacterStats.OnDie += HandleDie;
                }
            }
            // don't override an existing toughness
            if (_characterUnit.BaseCharacter.UnitToughness == null && toughness != null) {
                //Debug.Log("UnitSpawnNode.Spawn(): setting toughness to null on gameObject: " + spawnReference.name);
                _characterUnit.BaseCharacter.SetUnitToughness(toughness, true);
            }
            unitController.Init();
            spawnReferences.Add(unitController);
        }

        /// <summary>
        /// if the maximum unit count is not exceeded and the prerequisites are met, return true
        /// </summary>
        /// <returns></returns>
        private bool CanTriggerSpawn() {
            if ((spawnReferences.Count < GetMaxUnits() || GetMaxUnits() == -1) && MyPrerequisitesMet) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// spawn a unit if the trigger conditions are met, then start the countdown for the next spawn
        /// </summary>
        private void SpawnWithDelay() {
            //Debug.Log(gameObject.name + "UnitSpawnNode.SpawnWithDelay()");

            if (suppressAutoSpawn) {
                return;
            }
            if (unitProfiles.Count == 0) {
                return;
            }
            // this method is necessary to ensure that the main spawn count cycle is followed and the delay does not get directly added to the restart time
            if (CanTriggerSpawn()) {
                // if the countdown routine is not null, then there is already a regular spawn count or respawn count in progress
                if (countDownRoutine == null) {

                    // if the delay routine is not null, then there is already a spawn delay in progress so we do not want to start another one
                    if (delayRoutine == null) {
                        delayRoutine = StartCoroutine(StartSpawnDelayCountDown());
                    }

                    // now that we have spawned the mob (or at least started its additional delay timer), we will start the regular countdown
                    if (CanTriggerCountdown()) {
                        countDownRoutine = StartCoroutine(StartSpawnCountdown(spawnTimer));
                    }
                }
            }
        }

        public bool CanTriggerCountdown() {
            if (delayRoutine == null && CanTriggerSpawn()) {
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
            //Debug.Log(gameObject.name + "UnitSpawnNode.StartSpawnDelayCountDown()");
            float currentDelayTimer = spawnDelay;
            while (currentDelayTimer > 0) {
                //Debug.Log("UnitSpawnNode.Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentDelayTimer -= 1;
            }
            // clearing the coroutine so the next round can start
            delayRoutine = null;
            if (disabled == false) {
                Spawn();
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

        private void DestroySpawns() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.DestroySpawn(): Destroying spawns");
            foreach (UnitController unitController in spawnReferences) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.DestroySpawn(): Destroying spawn: " + unitController.gameObject.name + "; delay: " + despawnDelay);
                unitController.Despawn(despawnDelay);
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
            //Debug.Log(gameObject.name + ".UnitSpawnNode.StartSpawnCountdown(" + countdownTime + ")");
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
                SpawnWithDelay();
            }
        }

        public void ProcessRespawn(UnitController unitController) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.ProcessRespawn()");
            if (spawnReferences.Contains(unitController)) {
                spawnReferences.Remove(unitController);
            }
            if (respawnTimer > -1) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; starting despawn countdown");
                if (countDownRoutine == null) {
                    countDownRoutine = StartCoroutine(StartSpawnCountdown(respawnTimer));
                } else {
                    //Debug.Log("Countdown routine already in progress, not starting new countdown");
                }
            } else {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; NOT STARTING DESPAWN COUNTDOWN");
            }
        }

        public void HandleDespawn(UnitController unitController) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn()");
            if (respawnOn != respawnCondition.Despawn) {
                return;
            }
            ProcessRespawn(unitController);
        }

        public void HandleLootComplete(UnitController unitController) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleLootComplete(): timer: " + respawnTimer);
            if (respawnOn != respawnCondition.Loot) {
                return;
            }
            ProcessRespawn(unitController);
        }

        public void HandleDie(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDie(): timer: " + respawnTimer);
            if (respawnOn != respawnCondition.Death) {
                return;
            }
            // this works because unit spawn nodes do not spawn players.  If they ever do, this will need to reference stats->unit->gameobject instead
            ProcessRespawn(characterStats.BaseCharacter.UnitController);
        }

        public void OnTriggerEnter(Collider other) {
            if (!triggerBased) {
                return;
            }

            // only players can activate trigger based unit spawn nodes.  we don't want npcs wandering around patrolling to activate these
            if (playerManager.PlayerUnitSpawned == false || other.gameObject != playerManager.ActiveUnitController.gameObject) {
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
            Spawn();
        }

        public void SetupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        //Debug.Log(gameObject.name + ".SetupScriptableObjects(): setting up prerequisites");
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);

                        // add this so unit spawn nodes can have their prerequisites properly set on the first check
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
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
                            Debug.LogError(gameObject.name + ": Unit Profile: " + unitProfileName + " not found while initializing Unit Unit Spawn Node.  Check Inspector!");
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

    public enum respawnCondition { Despawn, Loot, Death };

}