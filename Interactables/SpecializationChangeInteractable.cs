using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private SpecializationChangeConfig specializationChangeConfig = null;

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        private string specializationName = string.Empty;

        private ClassSpecialization classSpecialization;

        private bool windowEventSubscriptionsInitialized = false;

        public ClassSpecialization MyClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public SpecializationChangeInteractable(Interactable interactable, SpecializationChangeConfig interactableOptionConfig) : base(interactable) {
            this.specializationChangeConfig = interactableOptionConfig;
            SetupScriptableObjects();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.specializationChangeWindow != null && PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents != null && (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as SpecializationChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as SpecializationChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
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

            (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as SpecializationChangePanelController).Setup(MyClassSpecialization);
            (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as SpecializationChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.specializationChangeWindow.MyCloseableWindowContents as SpecializationChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.specializationChangeWindow.CloseWindow();
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
            if (classSpecialization == null && specializationName != null && specializationName != string.Empty) {
                ClassSpecialization tmpClassSpecialization = SystemClassSpecializationManager.MyInstance.GetResource(specializationName);
                if (tmpClassSpecialization != null) {
                    classSpecialization = tmpClassSpecialization;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find specialization : " + specializationName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }

        }

    }

}