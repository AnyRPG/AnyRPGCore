using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class AppearancePanel : WindowContentController {

        //public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Appearance")]

        [SerializeField]
        protected GameObject mainNoOptionsArea = null;

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        [Header("Buttons")]

        [SerializeField]
        protected HighlightButton maleButton = null;

        [SerializeField]
        protected HighlightButton femaleButton = null;

        protected ICapabilityConsumer capabilityConsumer = null;

        protected UnitModelController unitModelController = null;

        //private bool panelVisible = false;

        // game manager references
        protected CharacterCreatorManager characterCreatorManager = null;


        public GameObject MainNoOptionsArea { get => mainNoOptionsArea; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterCreatorManager = systemGameManager.CharacterCreatorManager;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("AppearancePanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            InitializeGenderButtons();
            //uINavigationControllers[0].FocusCurrentButton();
            DisablePanelDisplay();
        }
        


        public virtual void SetCapabilityConsumer(ICapabilityConsumer capabilityConsumer) {
            this.capabilityConsumer = capabilityConsumer;
        }

        public virtual void HidePanel() {
            //Debug.Log("AppearancePanel.HidePanel()");
            if (canvasGroup.alpha == 1) {
                DisablePanelDisplay();
                ShowEquipment();
            }
            //HidePanel(true);
        }

        /*
        public virtual void HidePanel(bool showEquipment) {

            DisablePanelDisplay();

            if (showEquipment) {
                ShowEquipment();
            }
        }
        */

        public void DisablePanelDisplay() {
            //Debug.Log(gameObject.name + ".AppearancePanel.DisablePanelDisplay()");

            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public virtual void ShowPanel() {
            //Debug.Log(gameObject.name + ".AppearancePanel.ShowPanel()");

            if (canvasGroup.alpha == 0) {
                HideEquipment();
                EnablePanelDisplay();
            }
        }

        public void EnablePanelDisplay() {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public virtual void SetupOptions() {
            //Debug.Log(gameObject.name + ".AppearancePanel.SetupOptions()");

            InitializeGenderButtons();

            GetUnitModelController();
        }

        public virtual void GetUnitModelController() {
            //Debug.Log(gameObject.name + ".AppearancePanel.GetUnitModelController()");

            unitModelController = characterCreatorManager.PreviewUnitController?.UnitModelController;
        }

        protected void InitializeGenderButtons() {
            //Debug.Log(gameObject.name + ".AppearancePanel.InitializeGenderButtons()");

            if (capabilityConsumer.CharacterRace == null) {
                // no race set so no way to get the proper gender model
                return;
            }

            if (capabilityConsumer.CharacterRace.MaleUnitProfile == null || capabilityConsumer.CharacterRace.FemaleUnitProfile == null) {
                DisableGenderButtons();
                return;
            }

            EnableGenderButtons();

            if (characterCreatorManager.UnitProfile == capabilityConsumer.CharacterRace.MaleUnitProfile) {
                HighlightMaleButton();
            } else {
                HighlightFemaleButton();
            }

        }

        public virtual void DisableGenderButtons() {
            //Debug.Log(gameObject.name + ".AppearancePanel.DisableGenderButtons()");

            maleButton.gameObject.SetActive(false);
            femaleButton.gameObject.SetActive(false);
        }

        public virtual void EnableGenderButtons() {
            //Debug.Log(gameObject.name + ".AppearancePanel.EnableGenderButtons()");

            maleButton.gameObject.SetActive(true);
            femaleButton.gameObject.SetActive(true);
        }

        public virtual void SetMale() {
            //Debug.Log("AppearancePanel.SetMale()");

            if (capabilityConsumer.CharacterRace == null) {
                // no race set so no way to get the proper gender model
                return;
            }

            if (characterCreatorManager.UnitProfile == capabilityConsumer.CharacterRace.MaleUnitProfile) {
                // already male, nothing to do
                return;
            }

            ProcessBeforeSetMale();

            HighlightMaleButton();

            ProcessSetMale();
        }

        public virtual void ProcessBeforeSetMale() {
            // nothing to do here for now
        }


        public virtual void ProcessSetMale() {
            characterCreatorManager.DespawnUnit();
            characterCreatorManager.SpawnUnit(capabilityConsumer.CharacterRace.MaleUnitProfile);
            /*
            GetUnitModelController();
            unitModelController.SuppressEquipment = true;
            */
        }

        public virtual void HighlightMaleButton() {
            //Debug.Log("AppearancePanel.HighlightMaleButton()");

            femaleButton.UnHighlightBackground();
            maleButton.HighlightBackground();
        }

        public virtual void SetFemale() {
            //Debug.Log(gameObject.name + ".AppearancePanel.SetFemale()");

            if (capabilityConsumer.CharacterRace == null) {
                // no race set so no way to get the proper gender model
                return;
            }

            if (characterCreatorManager.UnitProfile == capabilityConsumer.CharacterRace.FemaleUnitProfile) {
                // already male, nothing to do
                return;
            }

            ProcessBeforeSetFemale();

            HighlightFemaleButton();

            ProcessSetFemale();
        }

        public virtual void ProcessBeforeSetFemale() {
            // nothing to do here for now
        }

        public virtual void ProcessSetFemale() {
            //Debug.Log(gameObject.name + ".AppearancePanel.ProcessSetFemale()");

            characterCreatorManager.DespawnUnit();
            characterCreatorManager.SpawnUnit(capabilityConsumer.CharacterRace.FemaleUnitProfile);
            /*
            GetUnitModelController();
            unitModelController.SuppressEquipment = true;
            */
        }

        public virtual void HighlightFemaleButton() {
            //Debug.Log(gameObject.name + ".AppearancePanel.HighlightFemaleButton()");

            maleButton.UnHighlightBackground();
            femaleButton.HighlightBackground();
        }

        protected void HideEquipment() {
            //Debug.Log(gameObject.name + ".AppearancePanel.HideEquipment()");

            unitModelController.HideEquipment();
        }

        protected void ShowEquipment() {
            //Debug.Log(gameObject.name + ".AppearancePanel.ShowEquipment()");

            if (unitModelController == null) {
                return;
            }

            unitModelController.ShowEquipment();
        }

        public virtual void HandleUnitCreated() {
            Debug.Log(gameObject.name + ".AppearancePanel.HandleUnitCreated()");

            GetUnitModelController();
            //if (panelVisible == true) {
            if (canvasGroup.alpha == 1) {
                Debug.Log(gameObject.name + ".AppearancePanel.HandleTargetCreated() suppressing equipment");
                unitModelController.SuppressEquipment = true;
            }
        }

        public virtual void HandleModelCreated() {
            Debug.Log("UMAAppearanceEditorPanelController.HandleModelCreated()");
        }
    }

}