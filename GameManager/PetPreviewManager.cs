using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PetPreviewManager : MonoBehaviour {

        #region Singleton
        private static PetPreviewManager instance;

        public static PetPreviewManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PetPreviewManager>();
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

        [SerializeField]
        private string layerName = "PetPreview";

        private int previewLayer;

        // the source we are going to clone from 
        private GameObject cloneSource;

        //private bool targetInitialized = false;



        public GameObject MyPreviewUnit { get => previewUnit; set => previewUnit = value; }
        public int PreviewLayer { get => previewLayer; set => previewLayer = value; }

        private void Awake() {
            previewLayer = LayerMask.NameToLayer(layerName);
        }

        public void Start() {
            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public void HandleOpenWindow() {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

           cloneSource = PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton.MyUnitProfile.UnitPrefab;

            if (cloneSource == null) {
                //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");
            }

            //Debug.Log("CharacterCreatorManager.HandleOpenWindow(): spawning preview unit");
            previewUnit = Instantiate(cloneSource, transform.position, Quaternion.identity, transform);
            UIManager.MyInstance.SetLayerRecursive(previewUnit, previewLayer);

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
            if (previewUnit.GetComponent<AIController>() != null) {
                previewUnit.GetComponent<AIController>().enabled = false;
            }
            if (previewUnit.GetComponent<NavMeshAgent>() != null) {
                previewUnit.GetComponent<NavMeshAgent>().enabled = false;
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

    }
}