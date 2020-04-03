using AnyRPG;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorManager : MonoBehaviour {

        #region Singleton
        private static CharacterCreatorManager instance;

        public static CharacterCreatorManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CharacterCreatorManager>();
                }

                return instance;
            }
        }

        #endregion

        // the gameObject we will spawn to focus the camera on.  This allows us to make modifications without saving them to the actual character unit until we are happy
        [SerializeField]
        private GameObject previewUnit;

        [SerializeField]
        private Vector3 previewSpawnLocation;

        // the source we are going to clone from 
        private GameObject cloneSource;

        //private bool targetInitialized = false;

        public GameObject MyPreviewUnit { get => previewUnit; set => previewUnit = value; }

        public void Start() {
            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
            //PlayerManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            /*
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                HandlePlayerUnitSpawn();
            }
            */
        }

        public void HandleOpenWindow(UnitProfile unitProfile) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            /*
            // determine which preview prefab is the correct one to clone
            if (forceUMAUnit) {
                // clone UMA prefab directly
                PlayerManager.MyInstance.MyCurrentPlayerUnitPrefab = PlayerManager.MyInstance.MyDefaultUMAPlayerUnitPrefab;
            }

            // determine if the character is currently an UMA unit.  If it is not, then spawn the playermanager default UMA
            if (PlayerManager.MyInstance.MyCurrentPlayerUnitPrefab == PlayerManager.MyInstance.MyDefaultUMAPlayerUnitPrefab) {
                //Debug.Log("CharacterCreatorManager.HandleOpenWindow(): the current player unit prefab is the UMA prefab, cloning UMA prefab");
                cloneSource = PlayerManager.MyInstance.MyDefaultUMAPlayerUnitPrefab;
            } else {
                //Debug.Log("CharacterCreatorManager.HandleOpenWindow(): the current player unit prefab is NOT the UMA prefab, cloning default prefab");
                cloneSource = PlayerManager.MyInstance.MyDefaultNonUMAPlayerUnitPrefab;
            }
            */
            cloneSource = unitProfile.MyUnitPrefab;

            if (cloneSource == null) {
                //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");
            }

            //Debug.Log("CharacterCreatorManager.HandleOpenWindow(): spawning preview unit");
            previewUnit = Instantiate(cloneSource, transform.position, Quaternion.identity, transform);
            UIManager.MyInstance.SetLayerRecursive(previewUnit, 12);

            // disable any components on the cloned unit that may give us trouble since this unit cannot move
            if (previewUnit.GetComponent<PlayerUnitMovementController>() != null) {
                previewUnit.GetComponent<PlayerUnitMovementController>().enabled = false;
            }
            if (previewUnit.GetComponent<Interactable>() != null) {
                previewUnit.GetComponent<Interactable>().enabled = false;
            }
            if (previewUnit.GetComponent<AnyRPGCharacterController>() != null) {
                previewUnit.GetComponent<AnyRPGCharacterController>().enabled = false;
            }
            if (previewUnit.GetComponent<Rigidbody>() != null) {
                previewUnit.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                previewUnit.GetComponent<Rigidbody>().isKinematic = true;
                previewUnit.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                previewUnit.GetComponent<Rigidbody>().useGravity = false;
            }

            AIEquipmentManager aIEquipmentManager = previewUnit.AddComponent<AIEquipmentManager>();

        }

        public void HandleCloseWindow() {
            //Debug.Log("CharacterCreatorManager.HandleCloseWindow();");
            if (previewUnit != null) {
                Destroy(previewUnit);
            }
        }

        public IEnumerator WaitForCamera() {
            //Debug.Log("CharacterCreatorManager.WaitForCamera();");

            while (CharacterPanel.MyInstance.MyPreviewCameraController == null) {
                yield return null;
            }
            //Debug.Log("WaitForCamera(): got camera");

            CharacterPanel.MyInstance.MyPreviewCameraController.InitializeCamera(previewUnit.transform);
            //targetInitialized = true;

        }


    }
}