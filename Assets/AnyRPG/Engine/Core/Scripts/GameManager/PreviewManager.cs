using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace AnyRPG {
    public abstract class PreviewManager : MonoBehaviour {

        protected UnitController unitController;

        [SerializeField]
        protected Vector3 previewSpawnLocation;

        /*
        [Tooltip("The name of the layer to set the preview unit to")]
        [SerializeField]
        protected string layerName;

        protected int previewLayer;
        */

        // the source we are going to clone from 
        protected UnitProfile cloneSource;

        public UnitController PreviewUnitController { get => unitController; set => unitController = value; }
        //public int PreviewLayer { get => previewLayer; set => previewLayer = value; }

        protected void Awake() {
            //previewLayer = LayerMask.NameToLayer(layerName);
        }

        protected void Start() {
            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public void HandleCloseWindow() {
            //Debug.Log("PreviewManager.HandleCloseWindow()");
            if (unitController != null) {
                Destroy(unitController.gameObject);
                unitController = null;
            }
        }

        public virtual UnitProfile GetCloneSource() {
            // override this in all child classes
            return null;
        }

        public void OpenWindowCommon() {
            //Debug.Log("PreviewManager.OpenWindowCommon()");

            unitController = cloneSource.SpawnUnitPrefab(transform, transform.position, transform.forward, UnitControllerMode.Preview);
            if (unitController != null) {
                if (unitController.CharacterUnit.BaseCharacter != null) {
                    unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.AttachmentProfile = cloneSource.UnitPrefabProps.AttachmentProfile;
                }

            }
        }

    }
}