using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnNode : MonoBehaviour, IPrerequisiteOwner {

        [SerializeField]
        private List<string> unitProfileNames = new List<string>();

        private List<UnitProfile> unitProfiles = new List<UnitProfile>();

        [SerializeField]
        private List<GameObject> spawnPrefabs = new List<GameObject>();

        [SerializeField]
        private bool dynamicLevel = true;

        [SerializeField]
        private int unitLevel = 1;

        // levels above the normal level for this mob
        [SerializeField]
        private int extraLevels = 0;

        // spawn time for regular mob spawns
        [SerializeField]
        private int spawnTimer = 0;

        // an additional delay to add to the timer.  meant to allow an offset for multiple spawners of the same type
        [SerializeField]
        private int spawnDelay = 0;

        //private int defaultSpawnDelay = 0;

        // a separate spawn timer for when mob despawns are detected to give players longer to move away being a mob attacks them again, -1 disables respawning of despawned units
        [SerializeField]
        private int DespawnTimer = 0;

        // set to -1 to do infinite spawns ;>
        [SerializeField]
        private int maxUnits = 1;

        // to allow for unit spawn control panels to use this node
        [SerializeField]
        private bool suppressAutoSpawn = false;

        // ignore spawn timers and use trigger instead
        [SerializeField]
        private bool triggerBased = false;

        //[SerializeField]
        //private bool areaBased = false;

        // in area mode, the number of mobs per square meter to spawn
        [SerializeField]
        private float spawnDensity = 0.01f;

        [SerializeField]
        private bool pointBased = true;

        // if there are units spawned, and the prerequisite conditions are no longer met, despawn them
        [SerializeField]
        private bool forceDespawnUnits = false;

        [SerializeField]
        private float despawnDelay = 0f;

        [SerializeField]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness = null;


        private Coroutine countDownRoutine = null;

        private Coroutine delayRoutine = null;

        protected bool eventSubscriptionsInitialized = false;

        /*
        private float currentTimer = 0f;
        private float currentDelayTimer = 0f;
        */

        private List<GameObject> spawnReferences = new List<GameObject>();

        // conditions must be met for spawner to spawn nodes
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();


        // later on make this spawn mob as player walks into collider ;>
        //private BoxCollider boxCollider;
        public bool MyPrerequisitesMet {
            get {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
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
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.CreateEventSubscriptions(): player unit already spawned.  Handling player unit spawn");
                HandlePlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            }
            
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn() {
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
            Debug.Log(gameObject.name + ".UnitSpawnNode.HandlePrerequisiteUpdates()");
            CheckPrerequisites();
        }

        public void CheckPrerequisites() {
            Debug.Log(gameObject.name + ".UnitSpawnNode.CheckPrerequisites()");
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
                if (Physics.Raycast(attemptVector, Vector3.down, out hit, 500f, (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).movementMask)) {
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
            if ((spawnReferences.Count < GetMaxUnits() || GetMaxUnits() == -1) && MyPrerequisitesMet) {
                int spawnIndex = UnityEngine.Random.Range(0, unitProfiles.Count);
                if (unitProfiles[spawnIndex].MyUnitPrefab != null) {
                    CommonSpawn(unitLevel, extraLevels, dynamicLevel, unitProfiles[spawnIndex].MyUnitPrefab, unitToughness);
                }
            }
        }

        public void CommonSpawn(int unitLevel, int extraLevels, bool dynamicLevel, GameObject spawnPrefab, UnitToughness toughness = null) {
            //GetSpawnLocation();
            //Debug.Log(gameObject.name + "UnitSpawnNode.Spawn(): Spawning index: " + spawnIndex + "; :" + spawnPrefabs[spawnIndex].name);
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
            if (_characterUnit != null) {
                _characterUnit.OnDespawn += HandleDespawn;
            }
            // don't override an existing toughness
            if (_characterUnit.MyCharacter.MyCharacterStats.MyToughness == null) {
                //Debug.Log("UnitSpawnNode.Spawn(): setting toughness to null on gameObject: " + spawnReference.name);
                _characterUnit.MyCharacter.MyCharacterStats.MyToughness = toughness;
            }
            int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : unitLevel) + extraLevels;
            _characterUnit.MyCharacter.MyCharacterStats.SetLevel(_unitLevel);
            _characterUnit.MyCharacter.MyCharacterStats.TrySpawnDead();
            spawnReferences.Add(spawnReference);
        }

        private void SpawnWithDelay() {
            //Debug.Log(gameObject.name + "UnitSpawnNode.SpawnWithDelay()");

            if (suppressAutoSpawn) {
                return;
            }
            if (unitProfiles.Count == 0) {
                return;
            }
            // this method is necessary to ensure that the main spawn count cycle is followed and the delay does not get directly added to the restart time
            if ((spawnReferences.Count < GetMaxUnits() || GetMaxUnits() == -1) && MyPrerequisitesMet) {
                if (countDownRoutine == null) {
                    countDownRoutine = StartCoroutine(StartSpawnCountdown(spawnTimer));
                    // AVOID MULTIPLE SPAWNS DUE TO TRIGGER BY PREREQUISITES ON A NODE WITH A ZERO DELAY TIME -- MOVED INSIDE OUTER ROUTINE CHECK
                    if (delayRoutine == null) {
                        delayRoutine = StartCoroutine(StartSpawnDelayCountDown());
                    }
                }
            }
        }

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
            Debug.Log("UnitSpawnNode.DestroySpawn(): Destroying spawns");
            foreach (GameObject _gameObject in spawnReferences) {
                Destroy(_gameObject, despawnDelay);
            }
            spawnReferences.Clear();
        }

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

        public void HandleDespawn(GameObject _gameObject) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer);
            if (spawnReferences.Contains(_gameObject)) {
                spawnReferences.Remove(_gameObject);
            }
            if (DespawnTimer > -1) {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; starting despawn countdown");
                if (countDownRoutine == null) {
                    countDownRoutine = StartCoroutine(StartSpawnCountdown(DespawnTimer));
                } else {
                    //Debug.Log("Countdown routine already in progress, not starting new countdown");
                }
            } else {
                //Debug.Log(gameObject.name + ".UnitSpawnNode.HandleDespawn(): timer: " + DespawnTimer + "; NOT STARTING DESPAWN COUNTDOWN");
            }
        }

        public void OnTriggerEnter(Collider other) {
            if (!triggerBased) {
                return;
            }
            CharacterUnit _characterUnit = other.gameObject.GetComponent<CharacterUnit>();
            if (_characterUnit != null && _characterUnit == PlayerManager.MyInstance.MyCharacter.MyCharacterUnit) {
                Spawn();
            }
        }

        public void SetupScriptableObjects() {
            if (SystemGameManager.MyInstance == null) {
                Debug.LogError("System Game Manager Not Found In The Scene.");
                return;
            }
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(this);
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

}