using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private NameChangeConfig nameChangeConfig = null;

        public override Sprite Icon { get => nameChangeConfig.Icon; }
        public override Sprite NamePlateImage { get => nameChangeConfig.NamePlateImage; }

        [SerializeField]
        private GameObject spawnPrefab = null;

        private GameObject spawnReference = null;

        private BoxCollider boxCollider = null;

        private bool windowEventSubscriptionsInitialized = false;

        public NameChangeInteractable(Interactable interactable, NameChangeConfig interactableOptionConfig) : base(interactable) {
            this.nameChangeConfig = interactableOptionConfig;
        }

        protected override void Start() {
            //Debug.Log("NameChangeInteractable.Start()");
            base.Start();
            boxCollider = GetComponent<BoxCollider>();
            Spawn();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (SystemWindowManager.MyInstance != null && SystemWindowManager.MyInstance.nameChangeWindow != null && SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents != null) {
                (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            windowEventSubscriptionsInitialized = false;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        private void Spawn() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.Spawn()");
            if (spawnPrefab != null) {
                spawnReference = Instantiate(spawnPrefab, gameObject.transform);
            }
            if (boxCollider != null) {
                boxCollider.enabled = true;
            }
            //interactable.InitializeMaterials();
            MiniMapStatusUpdateHandler(this);
        }

        private void DestroySpawn() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.DestroySpawn()");
            if (spawnReference != null) {
                Destroy(spawnReference);
                spawnReference = null;
            }
            boxCollider.enabled = false;
            MiniMapStatusUpdateHandler(this);
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact(): EVENT SUBSCRIPTIONS WERE ALREADY INITIALIZED!!! RETURNING");
                return false;
            }
            base.Interact(source);

            SystemWindowManager.MyInstance.nameChangeWindow.OpenWindow();
            (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnConfirmAction += HandleConfirmAction;
            (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }



        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.SetMiniMapText(" + text + ")");
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

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
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

    }

}