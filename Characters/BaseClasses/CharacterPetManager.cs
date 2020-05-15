using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class CharacterPetManager : MonoBehaviour {

        [SerializeField]
        protected List<string> unitProfileNames = new List<string>();

        [SerializeField]
        protected List<UnitProfile> unitProfiles = new List<UnitProfile>();

        protected Dictionary<UnitProfile, GameObject> activeUnitProfiles = new Dictionary<UnitProfile, GameObject>();

        protected BaseCharacter baseCharacter;

        protected bool eventSubscriptionsInitialized = false;

        public BaseCharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public List<string> UnitProfileNames { get => unitProfileNames; set => unitProfileNames = value; }
        public List<UnitProfile> MyUnitProfiles { get => unitProfiles; set => unitProfiles = value; }
        public Dictionary<UnitProfile, GameObject> MyActiveUnitProfiles { get => activeUnitProfiles; set => activeUnitProfiles = value; }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + "CharacterPetManager.Start()");
            //CreateEventSubscriptions();
        }

        public void OrchestratorStart() {
            //Debug.Log(gameObject.name + "CharacterPetManager.OrchestratorStart()");

            GetComponentReferences();
            CreateEventSubscriptions();
            SetupScriptableObjects();
        }

        public void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + "CharacterPetManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
            CleanupCoroutines();
        }


        public virtual void CleanupCoroutines() {
            //Debug.Log(gameObject.name + ".CharacterPetManager.CleanupCoroutines()");
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".CharacterPetManager.SetupScriptableObjects()");

            if (unitProfileNames != null && unitProfileNames.Count > 0) {
                foreach (string unitProfileName in unitProfileNames) {
                    UnitProfile unitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                    if (unitProfile != null) {
                        unitProfiles.Add(unitProfile);
                    } else {
                        Debug.LogError(gameObject.name + ".CharacterPetManager.SetupScriptableObjects: COULD NOT FIND unitProfile: " + unitProfileName + " WHILE INITIALIZING");
                    }
                }
            }

        }

        public virtual void AddPet(UnitProfile unitProfile) {
            if (unitProfile != null && unitProfiles != null && unitProfiles.Contains(unitProfile) == false) {
                unitProfiles.Add(unitProfile);
            }
        }

        public virtual void AddPet(string unitProfileName) {
            UnitProfile unitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
            if (unitProfile != null) {
                AddPet(unitProfile);
            } else {
                Debug.Log(gameObject.name + ".CharacterPetManager.AddPet: COULD NOT FIND unitProfile: " + unitProfileName + " WHILE LOADING");
            }
        }

        public virtual void DespawnPet(UnitProfile unitProfile) {
            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                Destroy(activeUnitProfiles[unitProfile]);
            }
            activeUnitProfiles.Remove(unitProfile);
        }

        public virtual void SpawnPet(UnitProfile unitProfile) {
            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                // can't add the same dictionary key twice
                return;
            }
            Vector3 spawnLocation = Vector3.zero;
            Transform prefabParent = null;
            spawnLocation = baseCharacter.CharacterUnit.transform.position;
            prefabParent = baseCharacter.CharacterUnit.transform;
            //Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
            Vector3 finalSpawnLocation = spawnLocation;
            Vector3 usedForwardDirection = baseCharacter.CharacterUnit.transform.forward;
            GameObject prefabObject = Instantiate(unitProfile.MyUnitPrefab, finalSpawnLocation, Quaternion.LookRotation(usedForwardDirection), prefabParent);
            activeUnitProfiles.Add(unitProfile, prefabObject);
            HandlePetSpawn(prefabObject);
        }

        public virtual void HandlePetSpawn(GameObject go) {
            //Debug.Log(gameObject.name + ".CharacterPetManager.HandlePetSpawn()");
            go.transform.parent = null;
            //Vector3 newSpawnLocation = GetSpawnLocation();
            Vector3 newSpawnLocation = baseCharacter.CharacterUnit.transform.position;
            //Debug.Log("UnitSpawnNode.Spawn(): newSpawnLocation: " + newSpawnLocation);
            NavMeshAgent navMeshAgent = go.GetComponent<NavMeshAgent>();
            AIController aIController = go.GetComponent<AIController>();
            aIController.MyStartPosition = newSpawnLocation;
            //Debug.Log("UnitSpawnNode.Spawn(): navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; isOnOffMeshLink: " + navMeshAgent.isOnOffMeshLink + "; pathpending: " + navMeshAgent.pathPending + "; warping now!");
            //spawnReference.transform.position = newSpawnLocation;
            navMeshAgent.Warp(newSpawnLocation);
            //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
            CharacterUnit _characterUnit = go.GetComponent<CharacterUnit>();
            /*
            if (_characterUnit != null) {
                _characterUnit.OnDespawn += HandleDespawn;
            }
            */
            //int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : unitLevel) + extraLevels;
            int _unitLevel = baseCharacter.CharacterStats.Level;
            //Debug.Log(gameObject.name + ".CharacterPetManager.HandlePetSpawn(): level: " + _unitLevel);
            _characterUnit.MyCharacter.CharacterStats.SetLevel(_unitLevel);
            (_characterUnit.MyCharacter.CharacterStats as AIStats).ApplyControlEffects(baseCharacter.CharacterAbilityManager);
        }

    }

}