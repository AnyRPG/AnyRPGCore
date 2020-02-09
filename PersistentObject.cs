﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(UUID))]
    public class PersistentObject : MonoBehaviour {

        [SerializeField]
        private bool saveOnLevelUnload = false;

        private Vector3 storedPosition;

        private Vector3 storedForwardDirection;

        private string storedUUID;

        protected bool eventSubscriptionsInitialized = false;

        private void Awake() {
            OrchestratorStart();
        }

        // Start is called before the first frame update
        void Start() {
            LoadPersistentState();
        }

        public void LoadPersistentState() {
            //Debug.Log(gameObject.name + "PersistentObject.LoadPersistentState()");
            UUID uuid = GetComponent<UUID>();
            if (uuid != null) {
                if (LevelManager.MyInstance != null) {
                    SceneNode activeSceneNode =  LevelManager.MyInstance.GetActiveSceneNode();
                    if (activeSceneNode != null && activeSceneNode.MyPersistentObjects != null) {
                        if (activeSceneNode.MyPersistentObjects.ContainsKey(uuid.ID)) {
                            storedUUID = activeSceneNode.MyPersistentObjects[uuid.ID].UUID;
                            storedPosition = new Vector3(activeSceneNode.MyPersistentObjects[uuid.ID].LocationX, activeSceneNode.MyPersistentObjects[uuid.ID].LocationY, activeSceneNode.MyPersistentObjects[uuid.ID].LocationZ);
                            storedForwardDirection = new Vector3(activeSceneNode.MyPersistentObjects[uuid.ID].DirectionX, activeSceneNode.MyPersistentObjects[uuid.ID].DirectionY, activeSceneNode.MyPersistentObjects[uuid.ID].DirectionZ);
                            transform.position = storedPosition;
                            transform.forward = storedForwardDirection;
                        }
                    }
                }
            }
        }

        public void OrchestratorStart() {
            //GetComponentReferences();
            CreateEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + "CharacterAbilityManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandleLevelUnload() {
            Debug.Log(gameObject.name + "PersistentObject.HandleLevelUnload()");
            if (saveOnLevelUnload == true) {
                SaveProperties();
            }
        }

        private void OnEnable() {

        }

        private void OnDisable() {
            CleanupEventSubscriptions();
        }

        public void SaveProperties() {
            Debug.Log(gameObject.name + "PersistentObject.SaveProperties()");
            storedPosition = transform.position;
            storedForwardDirection = transform.forward;
            UUID uuid = GetComponent<UUID>();
            if (uuid != null) {
                storedUUID = uuid.ID;
            }

            // save this data to the scene node that is active
            if (LevelManager.MyInstance != null) {
                SceneNode currentSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
                if (currentSceneNode != null) {
                    currentSceneNode.MyPersistentObjects[storedUUID] = MakeSaveData();
                }
            }
        }

        public PersistentObjectSaveData MakeSaveData() {
            Debug.Log(gameObject.name + "PersistentObject.MakeSaveData()");
            PersistentObjectSaveData returnValue = new PersistentObjectSaveData();
            returnValue.UUID = storedUUID;
            returnValue.LocationX = storedPosition.x;
            returnValue.LocationY = storedPosition.y;
            returnValue.LocationZ = storedPosition.z;
            returnValue.DirectionX = storedForwardDirection.x;
            returnValue.DirectionY = storedForwardDirection.y;
            returnValue.DirectionZ = storedForwardDirection.z;

            return returnValue;
        }
    }
}

