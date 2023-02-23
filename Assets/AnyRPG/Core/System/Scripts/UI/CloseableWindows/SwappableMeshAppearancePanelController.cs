using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class SwappableMeshAppearancePanelController : AppearancePanel {

        //public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Appearance")]

        [SerializeField]
        protected GameObject mainButtonsArea = null;

        [SerializeField]
        protected GameObject mainNoOptionsArea = null;

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


        // game manager references
        protected CharacterCreatorManager characterCreatorManager = null;
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SaveManager saveManager = null;

        public GameObject MainNoOptionsArea { get => mainNoOptionsArea; }
        
        public override void SetGameManagerReferences() {
            //Debug.Log("SwappableMeshAppearancePanelController.SetGameManagerReferences()");
            base.SetGameManagerReferences();

            characterCreatorManager = systemGameManager.CharacterCreatorManager;
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
            //uINavigationControllers[0].FocusCurrentButton();
        }
        */


        public override void SetupOptions() {
            //Debug.Log("SwappableMeshAppearancePanelController.SetupOptions()");

            base.SetupOptions();

            // deactivate option areas
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);

            // get reference to model controller
            swappableMeshModelController = characterCreatorManager.PreviewUnitController.UnitModelController.ModelAppearanceController.GetModelAppearanceController<SwappableMeshModelController>();

            if (swappableMeshModelController == null) {
                // this panel has somehow been opened but the preview unit is not a swappable mesh model
                mainNoOptionsArea.SetActive(true);
                return;
            }

            // if the character doesn't have options
            if (swappableMeshModelController.ModelOptions.MeshGroups.Count == 0) {
                mainNoOptionsArea.SetActive(true);
                optionsAreaLabel.SetActive(false);
                return;
            }

            // the character has options
            mainButtonsArea.SetActive(true);

            optionGroups.Clear();

            foreach (SwappableMeshOptionGroup modelGroup in swappableMeshModelController.ModelOptions.MeshGroups) {
                
                // populate the group dictionary
                optionGroups.Add(modelGroup.GroupName, modelGroup);

                if (modelGroup.Meshes.Count == 0) {
                    continue;
                }
                MeshModelGroupButton meshModelGroupButton = objectPooler.GetPooledObject(modelGroupButtonPrefab, mainButtonsArea.transform).GetComponent<MeshModelGroupButton>();
                meshModelGroupButton.Configure(systemGameManager);
                meshModelGroupButton.ConfigureButton(this, modelGroup.GroupName);
                uINavigationControllers[0].AddActiveButton(meshModelGroupButton);
            }
            //uINavigationControllers[0].FocusCurrentButton();
            uINavigationControllers[0].FocusFirstButton();

        }

        public void HandleTargetReady() {
            //Debug.Log("SwappableMeshAppearancePanelController.HandleTargetReady()");
            SetupOptions();
        }

        public void ChooseOptionChoice(HighlightButton highlightButton, string groupName, string optionChoice) {
            Debug.Log("SwappableMeshAppearancePanelController.ChooseOptionChoice(" + groupName + ", " + optionChoice + ")");

            swappableMeshModelController.SetGroupChoice(groupName, optionChoice);
            listOptionsArea.UnHightlightButtonOutlines();
            gridOptionsArea.UnHightlightButtonOutlines();
            highlightButton.HighlightOutline();
        }

        public void ShowModelGroup(string groupName) {
            Debug.Log("SwappableMeshAppearancePanelController.ShowModelGroup(" + groupName + ")");

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
        }

        private void PopulateOptionList(SwappableMeshOptionGroup optionGroup) {
            Debug.Log("SwappableMeshAppearancePanelController.PopulateOptionList()");

            gridOptionsArea.gameObject.SetActive(false);
            listOptionsArea.gameObject.SetActive(true);
            listOptionsArea.DeleteActiveButtons();

            if (optionGroup.Optional == true) {
                AddOptionListButton(optionGroup.GroupName,  noOptionImage, "None", "");
            }

            foreach (SwappableMeshOptionChoice optionChoice in optionGroup.Meshes) {
                AddOptionListButton(optionGroup.GroupName, optionChoice.Icon, optionChoice.DisplayName, optionChoice.MeshName);
            }
        }

        private void PopulateOptionGrid(SwappableMeshOptionGroup optionGroup) {
            Debug.Log("SwappableMeshAppearancePanelController.PopulateOptionGrid()");

            listOptionsArea.gameObject.SetActive(false);
            gridOptionsArea.gameObject.SetActive(true);
            gridOptionsArea.DeleteActiveButtons();

            if (optionGroup.Optional == true) {
                AddOptionGridButton(optionGroup.GroupName, noOptionImage, "");
            }

            foreach (SwappableMeshOptionChoice optionChoice in optionGroup.Meshes) {
                AddOptionGridButton(optionGroup.GroupName, optionChoice.Icon, optionChoice.MeshName);
            }
        }

        private void AddOptionListButton(string groupName, Sprite optionImage, string displayName, string optionChoice) {
            Debug.Log("SwappableMeshAppearancePanelController.AddOptionListButton()");

            SwappableMeshOptionChoiceButton optionChoiceButton = null;
            if (optionImage != null) {
                optionChoiceButton = objectPooler.GetPooledObject(mixedListOptionButtonPrefab, listOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            } else {
                optionChoiceButton = objectPooler.GetPooledObject(textListOptionButtonPrefab, listOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            }
            if (optionChoiceButton != null) {
                optionChoiceButton.Configure(systemGameManager);
                optionChoiceButton.ConfigureButton(this, groupName, optionImage, displayName, optionChoice);
                listOptionsArea.AddActiveButton(optionChoiceButton);
            }
        }

        private void AddOptionGridButton(string groupName, Sprite optionImage, string optionChoice) {
            SwappableMeshOptionChoiceButton optionChoiceButton = objectPooler.GetPooledObject(gridOptionButtonPrefab, gridOptionsArea.transform).GetComponent<SwappableMeshOptionChoiceButton>();
            optionChoiceButton.Configure(systemGameManager);
            optionChoiceButton.ConfigureButton(this, groupName, optionImage, "", optionChoice);
            gridOptionsArea.AddActiveButton(optionChoiceButton);
        }


    }

}