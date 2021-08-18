using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace AnyRPG {
    public abstract class PreviewManager : ConfiguredMonoBehaviour {

        public event System.Action OnTargetCreated = delegate { };

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

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public void HandleCloseWindow() {
            //Debug.Log("PreviewManager.HandleCloseWindow()");
            if (unitController != null) {
                unitController.Despawn();
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
                if (unitController.UnitModelController != null) {
                    unitController.UnitModelController.SetAttachmentProfile(cloneSource.UnitPrefabProps.AttachmentProfile);
                }
                OnTargetCreated();
                unitController.Init();
            }
        }

    }
}