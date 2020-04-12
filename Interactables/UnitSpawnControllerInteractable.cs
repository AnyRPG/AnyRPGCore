using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyUnitSpawnControllerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyUnitSpawnControllerInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyUnitSpawnControllerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyUnitSpawnControllerNamePlateImage : base.MyNamePlateImage); }

        [SerializeField]
        private List<string> unitProfileNames = new List<string>();

        [SerializeField]
        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        [SerializeField]
        private GameObject spawnPrefab = null;

        [SerializeField]
        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        private GameObject spawnReference = null;

        private Collider boxCollider = null;

        protected override void Awake() {
            //Debug.Log("Portal.Awake()");
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log("Portal.Start()")
            base.Start();
            boxCollider = GetComponent<Collider>();
            if (boxCollider == null) {
                //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.Start(): collider is null");
            }
            Spawn();
        }

        private void Spawn() {
            //Debug.Log("Portal.Spawn(): Spawning " + spawnPrefab.name);
            if (spawnPrefab != null) {
                spawnReference = Instantiate(spawnPrefab, gameObject.transform);
            }
            boxCollider.enabled = true;
            //interactable.InitializeMaterials();
            MiniMapStatusUpdateHandler(this);
        }

        private void DestroySpawn() {
            //Debug.Log("Portal.DestroySpawn(): Destroying " + spawnPrefab.name);
            if (spawnReference != null) {
                Destroy(spawnReference);
                spawnReference = null;
            }
            boxCollider.enabled = false;
            MiniMapStatusUpdateHandler(this);
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (SystemWindowManager.MyInstance != null && SystemWindowManager.MyInstance.unitSpawnWindow != null && SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents != null) {
                (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction -= HandleConfirmAction;
                (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).MyUnitProfileList = unitProfileList;
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).MyUnitSpawnNodeList = unitSpawnNodeList;
            SystemWindowManager.MyInstance.unitSpawnWindow.OpenWindow();
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction += HandleConfirmAction;
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            SystemWindowManager.MyInstance.unitSpawnWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.SetMiniMapText(" + text + ")");
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            unitProfileList = new List<UnitProfile>();
            if (unitProfileNames != null) {
                foreach (string unitProfileName in unitProfileNames) {
                    UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                    if (tmpUnitProfile != null) {
                        unitProfileList.Add(tmpUnitProfile);
                    } else {
                        Debug.LogError(gameObject.name + "UnitSpawnControllerInteractable.SetupScriptableObjects(): COULD NOT FIND UNIT PROFILE: " + unitProfileName + " while initializing " + gameObject.name);
                    }
                }
            }
        }
    }
}