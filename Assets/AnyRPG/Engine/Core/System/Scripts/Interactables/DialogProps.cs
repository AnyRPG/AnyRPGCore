using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class DialogProps : InteractableOptionProps {

        [Header("Dialog")]

        [Tooltip("The names of the dialogs available to this interactable")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Dialog))]
        private List<string> dialogNames = new List<string>();

        private List<Dialog> dialogList = new List<Dialog>();

        // need to track this for access to some of its functions
        private DialogComponent dialogComponent = null;

        public override Sprite Icon { get => (systemConfigurationManager.DialogInteractionPanelImage != null ? systemConfigurationManager.DialogInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.DialogNamePlateImage != null ? systemConfigurationManager.DialogNamePlateImage : base.NamePlateImage); }

        public override string GetInteractionPanelTitle(int optionIndex = 0) {
                List<Dialog> currentList = dialogComponent.GetCurrentOptionList();
                if (currentList.Count > optionIndex) {
                    return currentList[optionIndex].DisplayName;
                }
                return base.GetInteractionPanelTitle(optionIndex);
        }

        public List<Dialog> DialogList { get => dialogList; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            dialogComponent = new DialogComponent(interactable, this, systemGameManager);
            foreach (Dialog dialog in dialogList) {
                dialog.RegisterPrerequisiteOwner(dialogComponent);
            }
            if (interactableOption != null) {
                interactableOption.SetComponent(dialogComponent);
            }
            return dialogComponent;
        }

        public void CleanupPrerequisiteOwner(DialogComponent dialogComponent) {
            foreach (Dialog dialog in dialogList) {
                dialog.UnregisterPrerequisiteOwner(dialogComponent);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("DialogProps.SetupScriptableObjects()");
            base.SetupScriptableObjects(systemGameManager);

            if (dialogNames != null) {
                foreach (string dialogName in dialogNames) {
                    Dialog tmpDialog = systemDataFactory.GetResource<Dialog>(dialogName);
                    if (tmpDialog != null) {
                        dialogList.Add(tmpDialog);
                    } else {
                        Debug.LogError("DialogComponent.SetupScriptableObjects(): Could not find dialog " + dialogName + " while initializing Dialog Interactable.");
                    }
                }
            }

        }
    }

}