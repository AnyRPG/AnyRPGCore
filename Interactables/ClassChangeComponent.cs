using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        private ClassChangeProps interactableOptionProps = null;

        private CharacterClass characterClass;

        private bool windowEventSubscriptionsInitialized = false;

        public CharacterClass MyCharacterClass { get => characterClass; set => characterClass = value; }

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        public ClassChangeComponent(Interactable interactable, ClassChangeProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            SetupScriptableObjects();
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
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnClassChange -= HandleClassChange;
            }
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            // because the skill is a special type of prerequisite, we need to be notified when it changes
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("SystemEventManager Not Found.  Is the GameManager prefab in the scene?");
                return;
            }
            SystemEventManager.MyInstance.OnClassChange += HandleClassChange;
        }

        public void HandleClassChange(CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates();
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
            if (interactableOptionProps != null && interactableOptionProps.ClassName != null && interactableOptionProps.ClassName != string.Empty) {
                CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(interactableOptionProps.ClassName);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("ClassChangeComponent.SetupScriptableObjects(): Could not find faction : " + interactableOptionProps.ClassName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }

        // character class is a special type of prerequisite
        public override bool MyPrerequisitesMet {
            get {
                bool returnValue = base.MyPrerequisitesMet;
                if (returnValue == false) {
                    return false;
                }
                if (PlayerManager.MyInstance.MyCharacter.CharacterClass == characterClass) {
                    return false;
                }
                return returnValue;
            }
        }

    }

}