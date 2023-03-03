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
        }
        


        public virtual void SetCapabilityConsumer(ICapabilityConsumer capabilityConsumer) {
            this.capabilityConsumer = capabilityConsumer;
        }

        public virtual void HidePanel() {
            //Debug.Log("AppearancePanel.HidePanel()");
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public virtual void ShowPanel() {
            Debug.Log(gameObject.name + ".AppearancePanel.ShowPanel()");

            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            SetupOptions();
        }

        public virtual void SetupOptions() {
            Debug.Log(gameObject.name + ".AppearancePanel.SetupOptions()");

            InitializeGenderButtons();
        }

        public virtual void HandleTargetReady() {

        }

        protected void InitializeGenderButtons() {
            Debug.Log(gameObject.name + ".AppearancePanel.InitializeGenderButtons()");

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
            Debug.Log(gameObject.name + ".AppearancePanel.DisableGenderButtons()");

            maleButton.gameObject.SetActive(false);
            femaleButton.gameObject.SetActive(false);
        }

        public virtual void EnableGenderButtons() {
            Debug.Log(gameObject.name + ".AppearancePanel.EnableGenderButtons()");

            maleButton.gameObject.SetActive(true);
            femaleButton.gameObject.SetActive(true);
        }

        public virtual void SetMale() {
            Debug.Log("AppearancePanel.SetMale()");

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
        }

        public virtual void HighlightMaleButton() {
            Debug.Log("AppearancePanel.HighlightMaleButton()");

            femaleButton.UnHighlightBackground();
            maleButton.HighlightBackground();
        }

        public virtual void SetFemale() {
            Debug.Log(gameObject.name + ".AppearancePanel.SetFemale()");

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
            Debug.Log(gameObject.name + ".AppearancePanel.ProcessSetFemale()");

            characterCreatorManager.DespawnUnit();
            characterCreatorManager.SpawnUnit(capabilityConsumer.CharacterRace.FemaleUnitProfile);
        }

        public virtual void HighlightFemaleButton() {
            Debug.Log(gameObject.name + ".AppearancePanel.HighlightFemaleButton()");

            maleButton.UnHighlightBackground();
            femaleButton.HighlightBackground();
        }

    }

}