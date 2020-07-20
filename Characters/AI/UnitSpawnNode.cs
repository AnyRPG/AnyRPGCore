using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnNode : MonoBehaviour, IPrerequisiteOwner {

        [Header("Spawn GameObject")]

        [SerializeField]
        private List<string> unitProfileNames = new List<string>();

        private List<UnitProfile> unitProfiles = new List<UnitProfile>();

        [SerializeField]
        private List<GameObject> spawnPrefabs = new List<GameObject>();

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
        private respawnCondition respawnOn;

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

        private List<GameObject> spawnReferences = new List<GameObject>();


        // later on make this spawn mob as player walks into collider ;>
        //private BoxCollider boxCollider;
        public bool MyPrerequisitesMet {
            get {
                // disabled next bit because it interferes with spawning in cutscenes
                /*
                if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
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

        private void Awake() {
            SetupScriptableObjects();
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
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): SystemEventManager not found.  Is the GameManager in the scene?");
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
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

            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            }

            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
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


        public void OnDisable() {
            //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
            CleanupEventSubscriptions();
            if (countDownRoutine != null) {
                StopCoroutine(countDownRoutine);
                countDownRoutine = null;
            }
            if (delayRoutine != null) {
                StopCoroutine(delayRoutine);
                delayRoutine = null;
            }
            CleanupScriptableObjects();
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
                DestroySpawns();
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
                if (Physics.Raycast(attemptVector, Vector3.down, out hit, 500f, (PlayerManager.MyInstance.MyCharacter.CharacterController as PlayerController).movementMask)) {
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

        public void ManualSpawn(int unitLevel, int extraLevels, bool dynamicLevel, GameObject spawnPrefab, UnitToughness toughness) {
            CommonSpawn(unitLevel, extraLevels, dynamicLevel, spawnPrefab, toughness);
        }

        public void Spawn() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Spawn(): GetMaxUnits(): " + GetMaxUnits());
            if (unitProfiles.Count == 0) {
                return;
            }
            if (CanTriggerSpawn()) {
                int spawnIndex = UnityEngine.Random.Range(0, unitProfiles.Count);
                if (unitProfiles[spawnIndex].UnitPrefab != null) {
                    CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfiles[spawnIndex].UnitPrefab, unitToughness);
                }
            }
        }

        public void CommonSpawn(int unitLevel, int extraLevels, bool dynamicLevel, GameObject spawnPrefab, UnitToughness toughness = null) {
            //Debug.Log(gameObject.name + "UnitSpawnNode.CommonSpawn()");
            if (spawnPrefab == null || PlayerManager.MyInstance.MyCharacter == null) {
                return;
            }
            //GameObject spawnReference = Instantiate(spawnPrefabs[spawnIndex], GetSpawnLocation(),  Quaternion.identity);
            GameObject spawnReference = Instantiate(spawnPrefab);
            spawnReference.name = spawnReference.name + SystemGameManager.MyInstance.GetSpawnCount();
            //Debug.Log("UnitSpawnNode.Spawn(): gameObject spawned at: " + spawnReference.transform.position);
            Vector3 newSpawnLocation = GetSpawnLocation();
            //Debug.Log("UnitSpawnNode.Spawn(): newSpawnLocation: " + newSpawnLocation);
            NavMeshAgent navMeshAgent = spawnReference.GetComponent<NavMeshAgent>();
            AIController aIController = spawnReference.GetComponent<AIController>();
            aIController.MyStartPosition = newSpawnLocation;
            //Debug.Log("UnitSpawnNode.Spawn(): navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; isOnOffMeshLink: " + navMeshAgent.isOnOffMeshLink + "; pathpending: " + navMeshAgent.pathPending + "; warping now!");
            //spawnReference.transform.position = newSpawnLocation;
            navMeshAgent.Warp(newSpawnLocation);
            spawnReference.transform.forward = transform.forward;
            //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
            CharacterUnit _characterUnit = spawnReference.GetComponent<CharacterUnit>();
            if (respawnOn == respawnCondition.Despawn) {
                if (_characterUnit != null) {
                    _characterUnit.OnDespawn += HandleDespawn;
                }
            } else if (respawnOn == respawnCondition.Loot) {
                LootableCharacter _lootableCharacter = spawnReference.GetComponent<LootableCharacter>();
                if (_lootableCharacter != null) {
                    _lootableCharacter.OnLootComplete += HandleLootComplete;
                }
            } else if (respawnOn == respawnCondition.Death) {
                if (_characterUnit != null && _characterUnit.MyCharacter != null && _characterUnit.MyCharacter.CharacterStats != null) {
                    _characterUnit.MyCharacter.CharacterStats.OnDie += HandleDie;
                }
            }
            // don't override an existing toughness
            if (_characterUnit.MyCharacter.UnitToughness == null) {
                //Debug.Log("UnitSpawnNode.Spawn(): setting toughness to null on gameObject: " + spawnReference.name);
                _characterUnit.MyCharacter.SetUnitToughness(toughness);
            }
            int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.CharacterStats.Level : unitLevel) + extraLevels;
            _characterUnit.MyCharacter.CharacterStats.SetLevel(_unitLevel);
            _characterUnit.MyCharacter.CharacterStats.TrySpawnDead();
            spawnReferences.Add(spawnReference);
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
                    countDownRoutine = StartCoroutine(StartSpawnCountdown(spawnTimer));
                }
            }
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
            Spawn();
        }

        private void DestroySpawns() {
            //Debug.Log("UnitSpawnNode.DestroySpawn(): Destroying spawns");
            foreach (GameObject _gameObject in spawnReferences) {
                Destroy(_gameObject, despawnDelay);
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
            while (currentTimer > 0) {
                //Debug.Log("UnitSpawnNode.Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            //clearing the coroutine so the next timer will be allowed to start
            countDownRoutine = null;
            SpawnWithDelay();
        }

        public void ProcessRespawn(GameObject _gameObject) {
            if (spawnReferences.Contains(_gameObject)) {
                spawnReferences.Remove(_gameObject);
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

        public void HandleDespawn(GameObject _gameObject) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn()");
            if (respawnOn != respawnCondition.Despawn) {
                return;
            }
        }

        public void HandleLootComplete(GameObject _gameObject) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleLootComplete(): timer: " + respawnTimer);
            if (respawnOn != respawnCondition.Loot) {
                return;
            }
            ProcessRespawn(_gameObject);
        }

        public void HandleDie(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDie(): timer: " + respawnTimer);
            if (respawnOn != respawnCondition.Death) {
                return;
            }
            // this works because unit spawn nodes do not spawn players.  If they ever do, this will need to reference stats->unit->gameobject instead
            ProcessRespawn(characterStats.gameObject);
        }

        public void OnTriggerEnter(Collider other) {
            if (!triggerBased) {
                return;
            }
            if (countDownRoutine != null) {
                return;
            }
            CharacterUnit _characterUnit = other.gameObject.GetComponent<CharacterUnit>();
            if (_characterUnit != null && _characterUnit == PlayerManager.MyInstance.MyCharacter.CharacterUnit) {
                Spawn();
            }
        }

        public void SetupScriptableObjects() {
            if (SystemGameManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": System Game Manager Not Found In The Scene.");
                return;
            }
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        //Debug.Log(gameObject.name + ".SetupScriptableObjects(): setting up prerequisites");
                        tmpPrerequisiteConditions.SetupScriptableObjects(this);

                        // add this so unit spawn nodes can have their prerequisites properly set on the first check
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
            }

            unitProfiles = new List<UnitProfile>();
            if (unitProfileNames != null) {
                foreach (string unitProfileName in unitProfileNames) {
                    if (unitProfileName != null && unitProfileName != string.Empty) {
                        UnitProfile unitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                        if (unitProfile != null) {
                            unitProfiles.Add(unitProfile);
                        }
                    }
                }
            }

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(defaultToughness);
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
                        tmpPrerequisiteConditions.CleanupScriptableObjects();
                    }
                }
            }
        }

    }

    public enum respawnCondition { Despawn, Loot, Death };

}