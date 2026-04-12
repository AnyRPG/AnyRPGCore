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

        private Quaternion storedRotation;

        private string storedUUID;

        protected bool eventSubscriptionsInitialized = false;

        private IPersistentObjectOwner persistentObjectOwner = null;

        // game manager references
        protected LevelManagerClient levelManagerClient = null;
        protected SystemEventManager systemEventManager = null;
        protected SaveManager saveManager = null;

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
            saveManager = systemGameManager.SaveManager;
        }

        public void Setup(IPersistentObjectOwner persistentObjectOwner, SystemGameManager systemGameManager) {
            //Debug.Log($"{(persistentObjectOwner as MonoBehaviour).gameObject.name}.Setup() setting UUID {persistentObjectOwner.UUID.ID}");
            this.persistentObjectOwner = persistentObjectOwner;
            Configure(systemGameManager);
        }

        public void Init() {
            Debug.Log($"{persistentObjectOwner.gameObject.name}.Init() UUID is {persistentObjectOwner.UUID.ID}");

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

            if (persistentObjectOwner.UUID == null) {
                return null;
            }
            SceneNode activeSceneNode = levelManagerClient.GetActiveSceneNode();
            if (activeSceneNode == null) {
                return null;
            }
            PersistentObjectSaveData persistentObjectSaveData = saveManager.GetEphemeralObject(persistentObjectOwner.UUID.ID, activeSceneNode);
            if (persistentObjectSaveData == null) {
                persistentObjectSaveData = saveManager.GetPersistentObject(persistentObjectOwner.UUID.ID, activeSceneNode);
            }
            if (persistentObjectSaveData != null) {
                storedUUID = persistentObjectSaveData.UUID;
                storedPosition = new Vector3(persistentObjectSaveData.LocationX, persistentObjectSaveData.LocationY, persistentObjectSaveData.LocationZ);
                storedRotation = new Quaternion(persistentObjectSaveData.RotationX, persistentObjectSaveData.RotationY, persistentObjectSaveData.RotationZ, persistentObjectSaveData.RotationW).normalized;
                //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.GetPersistentObjectSaveData() found persistent object save data for UUID {persistentObjectOwner.UUID.ID} with position {storedPosition} and rotation {storedRotation}");
                if (storedRotation.x == 0 && storedRotation.y == 0 && storedRotation.z == 0 && storedRotation.w == 0) {
                    Debug.LogWarning($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.GetPersistentObjectSaveData() found persistent object save data for UUID {persistentObjectOwner.UUID.ID} with invalid rotation {storedRotation}.  Setting to identity.");
                    storedRotation = Quaternion.identity;
                }
                return persistentObjectSaveData;
            }
            return null;
        }

        public void LoadPersistentState() {
            //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.LoadPersistentState()");

            PersistentObjectSaveData persistentObjectSaveData = GetPersistentObjectSaveData();
            if (persistentObjectSaveData == null) {
                return;
            }
            if (persistObjectPosition == true && moveOnStart == true) {
                Rigidbody rb = persistentObjectOwner.gameObject.GetComponent<Rigidbody>();
                if (rb != null) {
                    //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.LoadPersistentState() setting rigidbody position on UUID {persistentObjectOwner.UUID.ID} to {storedPosition} and rotation to {storedRotation}");
                    rb.position = storedPosition;
                    rb.rotation = storedRotation;
                } else {
                    //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.LoadPersistentState() setting transform.position on UUID {persistentObjectOwner.UUID.ID} to {storedPosition} and rotation to {storedRotation}");
                    persistentObjectOwner.transform.position = storedPosition;
                    //if (storedRotation != Quaternion.identity) {
                    persistentObjectOwner.transform.rotation = storedRotation;
                    //}
                }
            } else {
                //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.LoadPersistentState() NOT setting transform.position on UUID {persistentObjectOwner.UUID.ID} because persistObjectPosition is {persistObjectPosition} and moveOnStart is {moveOnStart}");
            }
            persistentObjectOwner.LoadPersistentObjectSaveData(persistentObjectSaveData);
        }

        public void ProcessBeforeUnloadScene(bool ephemeral) {
            //Debug.Log($"PersistentObjectComponent.HandleLevelUnload(sceneHandle: {sceneHandle}, sceneName: {sceneName}) hashcode: {GetHashCode()}");
            if (saveOnLevelUnload == true) {
                SaveProperties(ephemeral);
            }
        }

        public void ProcessSaveGame(bool ephemeral) {
            //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.ProcessSaveGame()");

            if (saveOnGameSave == true) {
                SaveProperties(ephemeral);
            }
        }

        public void SaveProperties(bool ephemeral) {
            //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.SaveProperties(ephemeral: {ephemeral})");

            // since all units automatically have this component, give it a chance to not save based on configuration
            if (systemGameManager.GameMode == GameMode.Network) {
                return;
            }

            //Debug.Log($"{gameObject.name}PersistentObject.SaveProperties()");
            storedPosition = persistentObjectOwner.transform.position;
            storedRotation = persistentObjectOwner.transform.rotation;
            if (persistentObjectOwner.UUID != null) {
                storedUUID = persistentObjectOwner.UUID.ID;
                //Debug.Log($"{persistentObjectOwner.gameObject.name}.PersistentObjectComponent.SaveProperties() UUID: {persistentObjectOwner.UUID.ID}");
            }

            // save this data to the scene node that is active
            if (levelManagerClient != null) {
                SceneNode currentSceneNode = levelManagerClient.GetActiveSceneNode();
                if (currentSceneNode != null) {
                    //currentSceneNode.PersistentObjects[storedUUID] = MakeSaveData();
                    if (ephemeral == true) {
                        saveManager.SaveEphemeralObject(storedUUID, MakeSaveData(), currentSceneNode);
                    } else {
                        saveManager.SavePersistentObject(storedUUID, MakeSaveData(), currentSceneNode);
                    }
                }
            }
        }

        public PersistentObjectSaveData MakeSaveData() {
            //Debug.Log($"{ persistentObjectOwner.gameObject.name}.PersistentObjectComponent.MakeSaveData() storedUUID: {storedUUID}");

            PersistentObjectSaveData returnValue = new PersistentObjectSaveData();
            returnValue.UUID = storedUUID;
            returnValue.LocationX = storedPosition.x;
            returnValue.LocationY = storedPosition.y;
            returnValue.LocationZ = storedPosition.z;
            returnValue.RotationX = storedRotation.x;
            returnValue.RotationY = storedRotation.y;
            returnValue.RotationZ = storedRotation.z;
            returnValue.RotationW = storedRotation.w;

            persistentObjectOwner.PopulatePersistentObjectSaveData(returnValue);

            return returnValue;
        }
    }

    public class PersistentState {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}

