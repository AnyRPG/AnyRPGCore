using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeComponent : InteractableOptionComponent {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private SpecializationChangeProps interactableOptionProps = null;

        private ClassSpecialization classSpecialization;

        private bool windowEventSubscriptionsInitialized = false;

        public ClassSpecialization MyClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public SpecializationChangeComponent(Interactable interactable, SpecializationChangeProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
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
            if (classSpecialization == null && interactableOptionProps.SpecializationName != null && interactableOptionProps.SpecializationName != string.Empty) {
                ClassSpecialization tmpClassSpecialization = SystemClassSpecializationManager.MyInstance.GetResource(interactableOptionProps.SpecializationName);
                if (tmpClassSpecialization != null) {
                    classSpecialization = tmpClassSpecialization;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find specialization : " + interactableOptionProps.SpecializationName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }

    }

}