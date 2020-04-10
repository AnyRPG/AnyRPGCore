using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        // the class that this interactable option offers
        [SerializeField]
        private string className = string.Empty;

        private CharacterClass characterClass;

        private bool windowEventSubscriptionsInitialized = false;


        public CharacterClass MyCharacterClass { get => characterClass; set => characterClass = value; }

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage : base.MyNamePlateImage); }

        protected override void Awake() {
            //Debug.Log("ClassChangeInteractable.Awake()");
            base.Awake();
            SetupScriptableObjects();
        }

        protected override void Start() {
            //Debug.Log("ClassChangeInteractable.Start()");
            base.Start();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.classChangeWindow != null && PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents != null && (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as ClassChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as ClassChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            windowEventSubscriptionsInitialized = false;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source);

            (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as ClassChangePanelController).Setup(MyCharacterClass);
            (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as ClassChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.classChangeWindow.MyCloseableWindowContents as ClassChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.classChangeWindow.CloseWindow();
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
            if (characterClass == null && className != null && className != string.Empty) {
                CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(className);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + className + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }

        }

    }

}