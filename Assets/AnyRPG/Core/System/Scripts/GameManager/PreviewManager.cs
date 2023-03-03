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
        protected UnitProfile unitProfile = null;

        public UnitController PreviewUnitController { get => unitController; set => unitController = value; }
        public UnitProfile UnitProfile { get => unitProfile; }

        //public int PreviewLayer { get => previewLayer; set => previewLayer = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public void HandleCloseWindow() {
            //Debug.Log("PreviewManager.HandleCloseWindow()");
            DespawnUnit();
        }

        public virtual void DespawnUnit() {
            Debug.Log("PreviewManager.DespawnUnit()");

            if (unitController == null) {
                return;
            }

            unitController.Despawn();
            unitController = null;
        }

        public virtual UnitProfile GetCloneSource() {
            // override this in all child classes
            return null;
        }

        protected virtual void SpawnUnit() {
            Debug.Log("PreviewManager.SpawnUnit()");

            unitController = unitProfile.SpawnUnitPrefab(transform, transform.position, transform.forward, UnitControllerMode.Preview);
            if (unitController != null) {
                if (unitController.UnitModelController != null) {
                    unitController.UnitModelController.SetAttachmentProfile(unitProfile.UnitPrefabProps.AttachmentProfile);
                }
                BroadcastTargetCreated();
                unitController.Init();
            }
        }

        protected virtual void BroadcastTargetCreated() {
            //Debug.Log("PreviewManager.BroadcastTargetCreated()");
            OnTargetCreated();
        }

    }
}