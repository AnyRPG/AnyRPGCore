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
        protected LevelManagerClient levelManagerClient = null;
        protected SystemEventManager systemEventManager = null;

        public bool MoveOnStart { get => moveOnStart; set => moveOnStart = value; }
        public bool PersistObjectPosition { get => persistObjectPosition; set => persistObjectPosition = value; }
        public bool SaveOnLevelUnload { get => saveOnLevelUnload; set => saveOnLevelUnload = value; }
        public bool SaveOnGameSave { get => saveOnGameSave; set => saveOnGameSave = value; }

        public PersistentObjectComponent() {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManagerClient = systemGameManager.LevelManagerClient;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void Setup(IPersistentObjectOwner persistentObjectOwner, SystemGameManager systemGameManager) {
            //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.Setup() setting UUID {persistentObjectOwner.UUID.ID}");
            this.persistentObjectOwner = persistentObjectOwner;
            Configure(systemGameManager);
        }

        public void Init() {
            //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.Init() UUID is {persistentObjectOwner.UUID.ID}");

            if (persistObjectPosition == false) {
                return;
            }
            // no persistent state on server
            if (networkManagerServer.ServerModeActive == true) {
                return;
            }
            if (moveOnStart == true) {
                LoadPersistentState();
            }
        }

        public PersistentObjectSaveData GetPersistentObjectSaveData() {
            //Debug.Log(persistentObjectOwner.gameObject.name + "PersistentObjectComponent.GetPersistentState()");
            if (persistentObjectOwner.UUID != null) {
                if (levelManagerClient != null) {
                    SceneNode activeSceneNode = levelManagerClient.GetActiveSceneNode();
                    if (activeSceneNode != null && activeSceneNode.PersistentObjects != null) {
                        PersistentObjectSaveData persistentObjectSaveData = activeSceneNode.GetPersistentObjectSaveData(persistentObjectOwner.UUID.ID);
                        if (persistentObjectSaveData != null) {
                            storedUUID = persistentObjectSaveData.UUID;
                            storedPosition = new Vector3(persistentObjectSaveData.LocationX, persistentObjectSaveData.LocationY, persistentObjectSaveData.LocationZ);
                            storedForwardDirection = new Vector3(persistentObjectSaveData.DirectionX, persistentObjectSaveData.DirectionY, persistentObjectSaveData.DirectionZ);
                            PersistentState persistentState = new PersistentState();
                            persistentState.Position = storedPosition;
                            persistentState.Forward = storedForwardDirection;
                            return persistentObjectSaveData;
                        }
                    }
                }
            }
            return null;
        }

        public void LoadPersistentState() {
            Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObject.LoadPersistentState()");

            PersistentObjectSaveData persistentObjectSaveData = GetPersistentObjectSaveData();
            if (persistentObjectSaveData != null && persistObjectPosition == true) {
                //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.PersistentObject.LoadPersistentState() setting transform.position on UUID {persistentObjectOwner.UUID.ID}");
                persistentObjectOwner.transform.position = storedPosition;
                persistentObjectOwner.transform.forward = storedForwardDirection;
            }
            persistentObjectOwner.LoadPersistentObjectSaveData(persistentObjectSaveData);
        }

        public void ProcessBeforeUnloadScene() {
            //Debug.Log($"PersistentObjectComponent.HandleLevelUnload(sceneHandle: {sceneHandle}, sceneName: {sceneName}) hashcode: {GetHashCode()}");
            if (saveOnLevelUnload == true) {
                SaveProperties();
            }
        }

        public void ProcessSaveGame() {
            if (saveOnGameSave == true) {
                SaveProperties();
            }
        }

        public void SaveProperties() {
            //Debug.Log($"{ persistentObjectOwner.gameObject.name}.PersistentObjectComponent.SaveProperties()");

            // since all units automatically have this component, give it a chance to not save based on configuration
            if (systemGameManager.GameMode == GameMode.Network) {
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
            if (levelManagerClient != null) {
                SceneNode currentSceneNode = levelManagerClient.GetActiveSceneNode();
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

            persistentObjectOwner.PopulatePersistentObjectSaveData(returnValue);

            return returnValue;
        }
    }

    public class PersistentState {
        public Vector3 Position;
        public Vector3 Forward;
    }
}

