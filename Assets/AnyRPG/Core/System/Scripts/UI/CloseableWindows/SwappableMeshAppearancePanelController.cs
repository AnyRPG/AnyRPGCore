using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class SwappableMeshAppearancePanelController : AppearancePanel {

        [Header("Swappable Mesh")]

        [SerializeField]
        protected GameObject mainButtonsArea = null;

        [SerializeField]
        protected UINavigationListVertical mainOptionsNavigationController = null;

        [Header("Options Areas")]

        [SerializeField]
        protected GameObject optionsAreaLabel = null;

        [SerializeField]
        protected TextMeshProUGUI optionsAreaLabelText = null;

        [SerializeField]
        protected UINavigationListVertical listOptionsArea = null;

        [SerializeField]
        protected UINavigationGrid gridOptionsArea = null;

        [Header("Image")]

        [SerializeField]
        protected Sprite noOptionImage = null;

        [Header("Prefab")]

        [SerializeField]
        protected GameObject modelGroupButtonPrefab = null;

        [SerializeField]
        protected GameObject textListOptionButtonPrefab = null;

        [SerializeField]
        protected GameObject mixedListOptionButtonPrefab = null;

        [SerializeField]
        protected GameObject gridOptionButtonPrefab = null;

        private SwappableMeshModelController swappableMeshModelController = null;

        private Dictionary<string, SwappableMeshOptionGroup> optionGroups = new Dictionary<string, SwappableMeshOptionGroup>();

        // key, meshName
        private Dictionary<string, string> chosenOptions = new Dictionary<string, string>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SaveManager saveManager = null;

        public override void SetGameManagerReferences() {
            //Debug.Log("SwappableMeshAppearancePanelController.SetGameManagerReferences()");
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            saveManager = systemGameManager.SaveManager;
        }

        /*
        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("SwappableMeshAppearancePanelController.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }
        */


        /*
        public override void ProcessOpenWindowNotification() {
            //Debug.Log("UMACharacterEditorPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            //mainOptionsNavigationController.FocusCurrentButton();
        }
        */


        public override void SetupOptions() {
            //Debug.Log("SwappableMeshAppearancePanelController.SetupOptions()");

            base.SetupOptions();

            // deactivate option areas
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);

            // get reference to model controller
            SwappableMeshModelController oldSwappableMeshModelController = swappableMeshModelController;
            swappableMeshModelController = characterCreatorManager.PreviewUnitController.UnitModelController.ModelAppearanceController.GetModelAppearanceController<SwappableMeshModelController>();

            if (swappableMeshModelController == null) {
                // this panel has somehow been opened but the preview unit is not a swappable mesh model
                mainNoOptionsArea.SetActive(true);
                return;
            }

            // if the character doesn't have options
            if (swappableMeshModelController.ModelOptions.MeshGroups.Count == 0) {
                mainNoOptionsArea.SetActive(true);
                return;
            }

            // the character has options
            mainButtonsArea.SetActive(true);

            // no need to reset settings if this is still the same model
            if (swappableMeshModelController == oldSwappableMeshModelController) {
                return;
            }

            optionsAreaLabel.SetActive(false);

            // reset options
            optionGroups.Clear();
            chosenOptions.Clear();

            // clear all old buttons
            mainOptionsNavigationController.DeleteActiveButtons();

            AddMainButtons();

            mainOptionsNavigationController.FocusFirstButton();
            mainOptionsNavigationController.CurrentNavigableElement?.Interact();
            //mainOptionsNavigationController.FocusCurrentButton();
            //mainOptionsNavigationController.SelectCurrentNavigableElement();

        }

        private void AddMainButtons() {
            //Debug.Log("SwappableMeshAppearancePanelController.AddMainButtons()");

            foreach (SwappableMeshOptionGroup modelGroup in swappableMeshModelController.ModelOptions.MeshGroups) {

                if (modelGroup.Hidden == true) {
                    continue;
                }

                if (modelGroup.Meshes.Count == 0) {
                    continue;
                }

                // populate the group dictionary
                optionGroups.Add(modelGroup.GroupName, modelGroup);

                // set chosen option to none for group
                chosenOptions.Add(modelGroup.GroupName, "");

                MeshModelGroupButton meshModelGroupButton = objectPooler.GetPooledObject(modelGroupButtonPrefab, mainButtonsArea.transform).GetComponent<MeshModelGroupButton>();
                meshModelGroupButton.Configure(systemGameManager);
                meshModelGroupButton.ConfigureButton(this, modelGroup.GroupName);
                mainOptionsNavigationController.AddActiveButton(meshModelGroupButton);
            }
        }

        /*
        public override void HandleModelCreated() {
            //Debug.Log("SwappableMeshAppearancePanelController.HandleModelCreated()");

            base.HandleModelCreated();

            SetupOptions();
        }
        */

        public void ChooseOptionChoice(HighlightButton highlightButton, string groupName, string optionChoice) {
            //Debug.Log("SwappableMeshAppearancePanelController.ChooseOptionChoice(" + groupName + ", " + optionChoice + ")");

            swappableMeshModelController.SetGroupChoice(groupName, optionChoice);
            swappableMeshModelController.BuildModelAppearance();
            listOptionsArea.UnHightlightButtonBackgrounds();
            gridOptionsArea.UnHightlightButtonBackgrounds();
            highlightButton.HighlightBackground();

            if (chosenOptions.ContainsKey(groupName) == true) {
                chosenOptions[groupName] = optionChoice;
            } else {
                chosenOptions.Add(groupName, optionChoice);
            }
        }

        public void ShowModelGroup(HighlightButton highlightButton, string groupName) {
            //Debug.Log("SwappableMeshAppearancePanelController.ShowModelGroup(" + groupName + ")");

            if (optionGroups.ContainsKey(groupName) == false) {
                return;
            }

            optionsAreaLabel.SetActive(true);
            optionsAreaLabelText.text = groupName;
            if (optionGroups[groupName].DisplayAs == SwappableMeshOptionGroupType.List) {
                PopulateOptionList(optionGroups[groupName]);
            } else {
                PopulateOptionGrid(optionGroups[groupName]);
            }

            highlightButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(highlightButton);
        }

        private void PopulateOptionList(SwappableMeshOptionGroup optionGroup) {
            //Debug.Log("SwappableMeshAppearancePanelController.PopulateOptionList()");

            gridOptionsArea.gameObject.SetActive(false);
            listOptionsArea.gameObject.SetActive(true);
            listOptionsArea.DeleteActiveButtons();

            if (optionGroup.Optional == true) {
                AddOptionListButton(optionGroup.GroupName,  noOptionImage, "None", "");
            }

            foreach (SwappableMeshOptionChoice optionChoice in optionGroup.Meshes) {
                //AddOptionListButton(optionGroup.GroupName, optionChoice.Icon, optionChoice.DisplayName, optionChoice.MeshName);
                AddOptionListButton(optionGroup.GroupName, optionChoice.Icon, optionChoice.DisplayName, optionChoice.DisplayName);
            }
        }

        private void PopulateOptionGrid(SwappableMeshOptionGroup optionGroup) {
            //Debug.Log("SwappableMeshAppearancePanelController.PopulateOptionGrid()");

            listOptionsArea.gameObject.SetActive(false);
            gridOptionsArea.gameObject.SetActive(true);
            gridOptionsArea.DeleteActiveButtons();

            if (optionGroup.Optional == true) {
                AddOptionGridButton(optionGroup.GroupName, noOptionImage, "");
            }

            foreach (SwappableMeshOptionChoice optionChoice in optionGroup.Meshes) {
                AddOptionGridButton(optionGroup.GroupName, optionChoice.Icon, optionChoice.DisplayName);
            }
        }

        private void AddOptionListButton(string groupName, Sprite optionImage, string displayName, string optionChoice) {
            //Debug.Log($"SwappableMeshAppearancePanelController.AddOptionListButton({groupName}, {displayName}, {optionChoice})");

            SwappableMeshOptionChoiceButton optionChoiceButton = null;
            if (optionImage != null) {
                optionChoiceButton = objectPooler.GetPooledObject(mixedListOptionButtonPrefab, listOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            } else {
                optionChoiceButton = objectPooler.GetPooledObject(textListOptionButtonPrefab, listOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            }
            if (optionChoiceButton == null) {
                return;
            }

            optionChoiceButton.Configure(systemGameManager);
            optionChoiceButton.ConfigureButton(this, groupName, optionImage, displayName, optionChoice);
            listOptionsArea.AddActiveButton(optionChoiceButton);

            // highlight the button if it is already the selected choice for the group
            if (swappableMeshModelController.OptionGroupChoices.ContainsKey(groupName) && swappableMeshModelController.OptionGroupChoices[groupName] == optionChoice) {
                optionChoiceButton.HighlightBackground();
            }

        }

        private void AddOptionGridButton(string groupName, Sprite optionImage, string optionChoice) {
            SwappableMeshOptionChoiceButton optionChoiceButton = objectPooler.GetPooledObject(gridOptionButtonPrefab, gridOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            optionChoiceButton.Configure(systemGameManager);
            optionChoiceButton.ConfigureButton(this, groupName, optionImage, optionChoice, optionChoice);
            gridOptionsArea.AddActiveButton(optionChoiceButton);

            // highlight the button if it is already the selected choice for the group
            if (swappableMeshModelController.OptionGroupChoices.ContainsKey(groupName) && swappableMeshModelController.OptionGroupChoices[groupName] == optionChoice) {
                optionChoiceButton.HighlightBackground();
            }

        }


    }

}