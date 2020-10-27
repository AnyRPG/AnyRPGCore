using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class PersistentObjectComponent {

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

        public bool MoveOnStart { get => moveOnStart; set => moveOnStart = value; }
        public bool PersistObjectPosition { get => persistObjectPosition; set => persistObjectPosition = value; }

        public PersistentObjectComponent() {
        }

        public void Initialize(IPersistentObjectOwner persistentObjectOwner) {
            this.persistentObjectOwner = persistentObjectOwner;
            OrchestratorStart();
        }


        // Start is called before the first frame update
        public void Start() {
            if (moveOnStart == true) {
                LoadPersistentState();
            }
        }

        public PersistentState GetPersistentState() {
            //Debug.Log(gameObject.name + "PersistentObject.GetPersistentState()");
            if (persistentObjectOwner.UUID != null) {
                if (LevelManager.MyInstance != null) {
                    SceneNode activeSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
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
            //Debug.Log(gameObject.name + "PersistentObject.LoadPersistentState()");
            PersistentState persistentState = GetPersistentState();
            if (persistentState != null) {
                persistentObjectOwner.transform.position = persistentState.Position;
                persistentObjectOwner.transform.forward = persistentState.Forward;
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
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("PersistentObjectComponent.CreateEventSubscriptions: Could not find SystemEventManager.  Is GameManager in the scene?");
            } else {
                SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
                SystemEventManager.StartListening("OnSaveGame", HandleSaveGame);
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
                SystemEventManager.StopListening("OnSaveGame", HandleSaveGame);
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            CleanupEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            if (saveOnLevelUnload == true) {
                SaveProperties();
            }
        }

        public void HandleSaveGame(string eventName, EventParamProperties eventParamProperties) {
            if (saveOnGameSave == true) {
                SaveProperties();
            }
        }

        public void SaveProperties() {

            // since all units automatically have this component, give it a chance to not save based on configuration
            if (persistObjectPosition == false) {
                return;
            }

            //Debug.Log(gameObject.name + "PersistentObject.SaveProperties()");
            storedPosition = persistentObjectOwner.transform.position;
            storedForwardDirection = persistentObjectOwner.transform.forward;
            if (persistentObjectOwner.UUID != null) {
                storedUUID = persistentObjectOwner.UUID.ID;
            }

            // save this data to the scene node that is active
            if (LevelManager.MyInstance != null) {
                SceneNode currentSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
                if (currentSceneNode != null) {
                    //currentSceneNode.PersistentObjects[storedUUID] = MakeSaveData();
                    currentSceneNode.SavePersistentObject(storedUUID, MakeSaveData());
                }
            }
        }

        public PersistentObjectSaveData MakeSaveData() {
            //Debug.Log(gameObject.name + "PersistentObject.MakeSaveData()");
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

