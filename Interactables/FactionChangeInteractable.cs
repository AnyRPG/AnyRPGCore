using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        // the faction that this interactable option offers
        [SerializeField]
        private string factionName;

        private Faction faction;

        public Faction MyFaction { get => faction; set => faction = value; }

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage : base.MyNamePlateImage); }

        protected override void Awake() {
            //Debug.Log("FactionChangeInteractable.Awake()");
            base.Awake();
            SetupScriptableObjects();
        }

        protected override void Start() {
            //Debug.Log("FactionChangeInteractable.Start()");
            base.Start();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupEventSubscriptions();
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.factionChangeWindow != null && PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents != null && (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            eventSubscriptionsInitialized = false;
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.Interact()");
            if (eventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source);
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).Setup(MyFaction);
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            eventSubscriptionsInitialized = true;
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

        public override bool SetMiniMapText(Text text) {
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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (faction == null && factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }

        }
    }

}