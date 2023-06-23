using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class PersistentObjectComponent : ConfiguredClass {

        [Header("Object Persistence")]

        [Tooltip("If true, the object position is saved based on selected settings. NOTE: at least one save option must be chosen (below or in patrol etc)")]
        [SerializeField]
        private bool persistObjectPosition = false;

        [Tooltip("If true, this object will save it's position when switching from one scene to another (including the main menu).  It will not save if the game is quit directly from the main menu.")]
        [SerializeField]
        private bool saveOnLevelUnload = false;

        [Tooltip("If true, this object will save it's position when the player saves the game.")]
        [SerializeField]
        private bool saveOnGameSave = false;

        // should this object be moved to its new position on start.  set to false for things with navmeshagent and let the unit spawn node warp them instead
        private bool moveOnStart = true;

        private Vector3 storedPosition;

        private Vector3 storedForwardDirection;

        private string storedUUID;

        protected bool eventSubscriptionsInitialized = false;

        private IPersistentObjectOwner persistentObjectOwner = null;

        // game manager references
        protected LevelManager levelManager = null;

        public bool MoveOnStart { get => moveOnStart; set => moveOnStart = value; }
        public bool PersistObjectPosition { get => persistObjectPosition; set => persistObjectPosition = value; }
        public bool SaveOnLevelUnload { get => saveOnLevelUnload; set => saveOnLevelUnload = value; }
        public bool SaveOnGameSave { get => saveOnGameSave; set => saveOnGameSave = value; }

        public PersistentObjectComponent() {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
        }

        public void Setup(IPersistentObjectOwner persistentObjectOwner, SystemGameManager systemGameManager) {
            //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.Setup() setting UUID {persistentObjectOwner.UUID.ID}");
            this.persistentObjectOwner = persistentObjectOwner;
            Configure(systemGameManager);
            CreateEventSubscriptions();
        }

        public void Cleanup() {
            CleanupEventSubscriptions();
        }

        public void Init() {
            //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.Init() UUID is {persistentObjectOwner.UUID.ID}");

            if (persistObjectPosition == false) {
                return;
            }
            if (moveOnStart == true) {
                LoadPersistentState();
            }
        }

        public PersistentState GetPersistentState() {
            //Debug.Log(persistentObjectOwner.gameObject.name + "PersistentObjectComponent.GetPersistentState()");
            if (persistentObjectOwner.UUID != null) {
                if (levelManager != null) {
                    SceneNode activeSceneNode = levelManager.GetActiveSceneNode();
                    if (activeSceneNode != null && activeSceneNode.PersistentObjects != null) {
                        PersistentObjectSaveData persistentObjectSaveData = activeSceneNode.GetPersistentObject(persistentObjectOwner.UUID.ID);
                        if (!persistentObjectSaveData.Equals(default(PersistentObjectSaveData))) {
                            storedUUID = persistentObjectSaveData.UUID;
                            storedPosition = new Vector3(persistentObjectSaveData.LocationX, persistentObjectSaveData.LocationY, persistentObjectSaveData.LocationZ);
                            storedForwardDirection = new Vector3(persistentObjectSaveData.DirectionX, persistentObjectSaveData.DirectionY, persistentObjectSaveData.DirectionZ);
                            PersistentState persistentState = new PersistentState();
                            persistentState.Position = storedPosition;
                            persistentState.Forward = storedForwardDirection;
                            return persistentState;
                        }
                    }
                }
            }
            return null;
        }

        public void LoadPersistentState() {
            //Debug.Log($"{gameObject.name}PersistentObject.LoadPersistentState()");
            PersistentState persistentState = GetPersistentState();
            if (persistentState != null) {
                //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.PersistentObject.LoadPersistentState() setting transform.position on UUID {persistentObjectOwner.UUID.ID}");
                persistentObjectOwner.transform.position = persistentState.Position;
                persistentObjectOwner.transform.forward = persistentState.Forward;
            }
        }

        public void CreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}CharacterAbilityManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnSaveGame", HandleSaveGame);
        }

        public void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnSaveGame", HandleSaveGame);
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            if (persistObjectPosition == false) {
                return;
            }
            if (saveOnLevelUnload == true) {
                SaveProperties();
            }
        }

        public void HandleSaveGame(string eventName, EventParamProperties eventParamProperties) {
            if (persistObjectPosition == false) {
                return;
            }
            if (saveOnGameSave == true) {
                SaveProperties();
            }
        }

        public void SaveProperties() {
            //Debug.Log($"{ persistentObjectOwner.gameObject.name}.PersistentObjectComponent.SaveProperties()");

            // since all units automatically have this component, give it a chance to not save based on configuration
            if (persistObjectPosition == false) {
                return;
            }

            //Debug.Log($"{gameObject.name}PersistentObject.SaveProperties()");
            storedPosition = persistentObjectOwner.transform.position;
            storedForwardDirection = persistentObjectOwner.transform.forward;
            if (persistentObjectOwner.UUID != null) {
                storedUUID = persistentObjectOwner.UUID.ID;
                //Debug.Log($"{ persistentObjectOwner.gameObject.name}.PersistentObjectComponent.SaveProperties() UUID: {storedUUID}");
            }

            // save this data to the scene node that is active
            if (levelManager != null) {
                SceneNode currentSceneNode = levelManager.GetActiveSceneNode();
                if (currentSceneNode != null) {
                    //currentSceneNode.PersistentObjects[storedUUID] = MakeSaveData();
                    currentSceneNode.SavePersistentObject(storedUUID, MakeSaveData());
                }
            }
        }

        public PersistentObjectSaveData MakeSaveData() {
            //Debug.Log(persistentObjectOwner.gameObject.name + ".PersistentObjectComponent.MakeSaveData()");
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

    public class PersistentState {
        public Vector3 Position;
        public Vector3 Forward;
    }
}

