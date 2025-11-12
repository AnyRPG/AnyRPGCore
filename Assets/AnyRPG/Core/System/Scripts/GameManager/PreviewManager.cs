using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace AnyRPG {
    public abstract class PreviewManager : ConfiguredMonoBehaviour, ICharacterRequestor {

        public event System.Action OnUnitCreated = delegate { };
        public event System.Action OnModelCreated = delegate { };

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

        // game manager references
        protected CharacterManager characterManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterManager = systemGameManager.CharacterManager;
        }

        public void HandleCloseWindow() {
            //Debug.Log("PreviewManager.HandleCloseWindow()");
            DespawnUnit();
        }

        public virtual void DespawnUnit() {
            //Debug.Log("PreviewManager.DespawnUnit()");

            if (unitController == null) {
                return;
            }

            unitController.UnitModelController.OnModelCreated -= HandleModelCreated;

            unitController.Despawn(0f, false, true);
            unitController = null;
        }

        public virtual UnitProfile GetCloneSource() {
            // override this in all child classes
            return null;
        }

        public virtual void SpawnUnit(CharacterConfigurationRequest characterConfigurationRequest) {
            //Debug.Log($"PreviewManager.SpawnUnit({characterConfigurationRequest.unitProfile.ResourceName})");

            unitProfile = characterConfigurationRequest.unitProfile;
            //Debug.Log("PreviewManager.SpawnUnit()");
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Preview;
            CharacterRequestData characterRequestData = new CharacterRequestData(
                this,
                GameMode.Local,
                characterConfigurationRequest
                );
            characterRequestData.characterId = characterManager.GetNewCharacterId(UnitControllerMode.Preview);
            systemGameManager.CharacterManager.SpawnUnitPrefabLocal(characterRequestData, transform, transform.position, transform.forward);
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
            //Debug.Log($"PreviewManager.ConfigureSpawnedCharacter({unitController.gameObject.name})");
            this.unitController = unitController;
            if (unitController.UnitModelController != null) {
                unitController.UnitModelController.SetAttachmentProfile(unitProfile.UnitPrefabProps.AttachmentProfile);
                unitController.UnitModelController.OnModelCreated += HandleModelCreated;
            }
            BroadcastUnitCreated();
        }

        public void PostInit(UnitController unitController) {
        }


        protected virtual void BroadcastUnitCreated() {
            //Debug.Log("PreviewManager.BroadcastUnitCreated()");
            OnUnitCreated();
        }

        protected virtual void HandleModelCreated() {
            //Debug.Log("PreviewManager.HandleModelCreated()");

            OnModelCreated();
        }

    }
}