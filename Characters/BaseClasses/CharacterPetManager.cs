using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class CharacterPetManager {

        private List<UnitProfile> unitProfiles = new List<UnitProfile>();

        protected Dictionary<UnitProfile, GameObject> activeUnitProfiles = new Dictionary<UnitProfile, GameObject>();

        protected BaseCharacter baseCharacter;

        protected bool eventSubscriptionsInitialized = false;

        public BaseCharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public List<UnitProfile> MyUnitProfiles { get => unitProfiles; set => unitProfiles = value; }
        public Dictionary<UnitProfile, GameObject> MyActiveUnitProfiles { get => activeUnitProfiles; set => activeUnitProfiles = value; }

        public CharacterPetManager(BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public virtual void AddPet(UnitProfile unitProfile) {
            // need more logic in here about whether this class or spec is allowed to capture this type of pet
            if (unitProfile != null && unitProfiles != null && unitProfiles.Contains(unitProfile) == false && unitProfile.IsPet == true) {
                unitProfiles.Add(unitProfile);
            }
        }

        public virtual void AddPet(string unitProfileName) {
            UnitProfile unitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
            if (unitProfile != null) {
                AddPet(unitProfile);
            } else {
                Debug.Log("CharacterPetManager.AddPet: COULD NOT FIND unitProfile: " + unitProfileName + " WHILE LOADING");
            }
        }

        public virtual void DespawnPet(UnitProfile unitProfile) {
            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                UnityEngine.Object.Destroy(activeUnitProfiles[unitProfile]);
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
            GameObject prefabObject = UnityEngine.Object.Instantiate(unitProfile.UnitPrefab, finalSpawnLocation, Quaternion.LookRotation(usedForwardDirection), prefabParent);
            activeUnitProfiles.Add(unitProfile, prefabObject);
            HandlePetSpawn(prefabObject);
        }

        public virtual void HandlePetSpawn(GameObject go) {
            //Debug.Log(gameObject.name + ".CharacterPetManager.HandlePetSpawn()");
            go.transform.parent = null;
            //Vector3 newSpawnLocation = GetSpawnLocation();
            Vector3 newSpawnLocation = baseCharacter.CharacterUnit.transform.position;
            //Debug.Log("UnitSpawnNode.Spawn(): newSpawnLocation: " + newSpawnLocation);

            // remove behavior and patrol interactable so pets don't patrol once you capture them
            BehaviorInteractable behaviorInteractable = go.GetComponent<BehaviorInteractable>();
            if (behaviorInteractable != null) {
                UnityEngine.Object.Destroy(behaviorInteractable);
            }

            UnitController unitController = go.GetComponent<UnitController>();
            unitController.SetUnitControllerMode(UnitControllerMode.Pet);
            unitController.MyStartPosition = newSpawnLocation;
            unitController.NavMeshAgent.Warp(newSpawnLocation);
            /*
            if (_characterUnit != null) {
                _characterUnit.OnDespawn += HandleDespawn;
            }
            */
            //int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : unitLevel) + extraLevels;
            int _unitLevel = baseCharacter.CharacterStats.Level;
            //Debug.Log(gameObject.name + ".CharacterPetManager.HandlePetSpawn(): level: " + _unitLevel);
            unitController.BaseCharacter.CharacterStats.SetLevel(_unitLevel);
            unitController.BaseCharacter.CharacterStats.ApplyControlEffects(baseCharacter);
        }

    }

}