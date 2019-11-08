using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnNode : MonoBehaviour {

        [SerializeField]
        private List<GameObject> spawnPrefabs = new List<GameObject>();

        [SerializeField]
        private bool dynamicLevel = true;

        [SerializeField]
        private int unitLevel;

        // levels above the normal level for this mob
        [SerializeField]
        private int extraLevels;

        // spawn time for regular mob spawns
        [SerializeField]
        private int spawnTimer;

        // an additional delay to add to the timer.  meant to allow an offset for multiple spawners of the same type
        [SerializeField]
        private int spawnDelay;

        private int defaultSpawnDelay = 0;

        // a separate spawn timer for when mob despawns are detected to give players longer to move away being a mob attacks them again, -1 disables respawning of despawned units
        [SerializeField]
        private int DespawnTimer;

        // set to -1 to do infinite spawns ;>
        [SerializeField]
        private int maxUnits;

        // ignore spawn timers and use trigger instead
        [SerializeField]
        private bool triggerBased = false;

        [SerializeField]
        private bool areaBased = false;

        // in area mode, the number of mobs per square meter to spawn
        [SerializeField]
        private float spawnDensity = 0.01f;

        [SerializeField]
        private bool pointBased = true;

        private Coroutine countDownRoutine = null;

        private Coroutine delayRoutine = null;

        protected bool eventReferencesInitialized = false;
        protected bool startHasRun = false;

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

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Start()");
            //boxCollider = GetComponent<BoxCollider>();
            startHasRun = true;
            if (!triggerBased) {
                SpawnWithDelay();
            }
            CreateEventReferences();
        }

        private void CreateEventReferences() {
            //Debug.Log("UnitSpawnNode.CreateEventReferences()");
            if (eventReferencesInitialized || !startHasRun) {
                return;
            }
            SystemEventManager.MyInstance.OnPrerequisiteUpdated += CheckPrerequisites;
            eventReferencesInitialized = true;
        }

        private void CleanupEventReferences() {
            //Debug.Log("UnitSpawnNode.CleanupEventReferences()");
            if (!eventReferencesInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPrerequisiteUpdated -= CheckPrerequisites;
            }
            eventReferencesInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
            CleanupEventReferences();
            if (countDownRoutine != null) {
                StopCoroutine(countDownRoutine);
                countDownRoutine = null;
            }
            if (delayRoutine != null) {
                StopCoroutine(delayRoutine);
                delayRoutine = null;
            }
        }
        public void CheckPrerequisites() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.CheckPrerequisites()");
            if (MyPrerequisitesMet && !triggerBased) {
                SpawnWithDelay();
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

        private void Spawn() {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.Spawn(): GetMaxUnits(): " + GetMaxUnits());
            int spawnIndex = UnityEngine.Random.Range(0, spawnPrefabs.Count - 1);
            if ((spawnReferences.Count < GetMaxUnits() || GetMaxUnits() == -1) && MyPrerequisitesMet) {
                //GetSpawnLocation();
                //Debug.Log("UnitSpawnNode.Spawn(): Spawning " + spawnPrefabs[spawnIndex].name);
                //GameObject spawnReference = Instantiate(spawnPrefabs[spawnIndex], GetSpawnLocation(),  Quaternion.identity);
                GameObject spawnReference = Instantiate(spawnPrefabs[spawnIndex]);
                //Debug.Log("UnitSpawnNode.Spawn(): gameObject spawned at: " + spawnReference.transform.position);
                Vector3 newSpawnLocation = GetSpawnLocation();
                //Debug.Log("UnitSpawnNode.Spawn(): newSpawnLocation: " + newSpawnLocation);
                NavMeshAgent navMeshAgent = spawnReference.GetComponent<NavMeshAgent>();
                AIController aIController = spawnReference.GetComponent<AIController>();
                aIController.MyStartPosition = newSpawnLocation;
                //Debug.Log("UnitSpawnNode.Spawn(): navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; isOnOffMeshLink: " + navMeshAgent.isOnOffMeshLink + "; pathpending: " + navMeshAgent.pathPending + "; warping now!");
                //spawnReference.transform.position = newSpawnLocation;
                navMeshAgent.Warp(newSpawnLocation);
                //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
                CharacterUnit _characterUnit = spawnReference.GetComponent<CharacterUnit>();
                if (_characterUnit != null) {
                    _characterUnit.OnDespawn += HandleDespawn;
                }
                int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : unitLevel) + extraLevels;
                _characterUnit.MyCharacter.MyCharacterStats.SetLevel(_unitLevel);
                spawnReferences.Add(spawnReference);
            }
        }

        private void SpawnWithDelay() {
            //Debug.Log(gameObject.name + "UnitSpawnNode.SpawnWithDelay()");
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
                currentDelayTimer -= 1;
                yield return new WaitForSeconds(1);
            }
            // clearing the coroutine so the next round can start
            delayRoutine = null;
            Spawn();
        }

        private void DestroySpawns() {
            //Debug.Log("UnitSpawnNode.DestroySpawn(): Destroying spawns");
            foreach (GameObject _gameObject in spawnReferences) {
                Destroy(_gameObject);
            }
            spawnReferences.Clear();
        }

        private IEnumerator StartSpawnCountdown(int countdownTime) {
            //Debug.Log(gameObject.name + ".UnitSpawnNode.StartSpawnCountdown(" + countdownTime + ")");
            float currentTimer = countdownTime;
            while (currentTimer > 0) {
                //Debug.Log("UnitSpawnNode.Spawn Timer: " + currentTimer);
                currentTimer -= 1;
                yield return new WaitForSeconds(1);
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

    }

}