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

        [Header("Prefab")]

        [SerializeField]
        protected GameObject modelGroupButtonPrefab = null;

        private SwappableMeshModelController swappableMeshModelController = null;


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
                return;
            }

            // the character has options
            mainButtonsArea.SetActive(true);

            foreach (SwappableMeshModelGroup modelGroup in swappableMeshModelController.ModelOptions.MeshGroups) {
                if (modelGroup.MeshNames.Count == 0) {
                    continue;
                }
                MeshModelGroupButton meshModelGroupButton = objectPooler.GetPooledObject(modelGroupButtonPrefab, mainButtonsArea.transform).GetComponent<MeshModelGroupButton>();
                meshModelGroupButton.Configure(systemGameManager);
                meshModelGroupButton.ConfigureButton(this, modelGroup.GroupName);
                uINavigationControllers[0].AddActiveButton(meshModelGroupButton);
            }
            uINavigationControllers[0].FocusCurrentButton();

        }

        public void HandleTargetReady() {
            //Debug.Log("SwappableMeshAppearancePanelController.HandleTargetReady()");
            SetupOptions();
        }

        public void ShowModelGroup(string groupName) {

        }

    }

}