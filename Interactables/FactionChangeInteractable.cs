using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private FactionChangeConfig factionChangeConfig = new FactionChangeConfig();

        private Faction faction;

        private bool windowEventSubscriptionsInitialized = false;

        public Faction MyFaction { get => faction; set => faction = value; }

        public override Sprite Icon { get => factionChangeConfig.Icon; }
        public override Sprite NamePlateImage { get => factionChangeConfig.NamePlateImage; }

        public FactionChangeInteractable(Interactable interactable, FactionChangeConfig factionChangeConfig) : base(interactable) {
            this.factionChangeConfig = factionChangeConfig;
            SetupScriptableObjects();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.factionChangeWindow != null && PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents != null && (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source);
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).Setup(MyFaction);
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
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
            if (factionChangeConfig != null && factionChangeConfig.FactionName != null && factionChangeConfig.FactionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionChangeConfig.FactionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionChangeConfig.FactionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }

        }
    }

}