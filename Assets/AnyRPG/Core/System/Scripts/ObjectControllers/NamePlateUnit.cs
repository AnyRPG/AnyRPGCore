using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class NamePlateUnit : Interactable {

        public event System.Action OnInitializeNamePlate = delegate { };
        public virtual event System.Action OnCameraTargetReady = delegate { };

        [SerializeField]
        protected NamePlateProps namePlateProps = new NamePlateProps();

        // created components
        protected BaseNamePlateController namePlateController = null;

        // track startup state
        protected bool namePlateReady = false;
        protected bool cameraTargetReady = true;

        public virtual BaseNamePlateController NamePlateController { get => namePlateController; }
        public virtual NamePlateProps NamePlateProps { get => namePlateProps; set => namePlateProps = value; }
        public override string DisplayName {
            get {
                if (namePlateProps.DisplayName != string.Empty) {
                    return namePlateProps.DisplayName;
                }
                return base.DisplayName;
            }
        }

        public virtual bool CameraTargetReady { get => cameraTargetReady; }

        /// <summary>
        /// initialize a nameplate if it has not been initialied yet
        /// </summary>
        public virtual void InitializeNamePlateController() {
            //Debug.Log($"{gameObject.name}.UnitController.InitializeNamePlateController()");
            if (namePlateReady == true) {
                return;
            }
            if (InitializeNamePlate()) {
                namePlateReady = true;
            }
        }

        /// <summary>
        /// directly initialize a nameplate
        /// </summary>
        public bool InitializeNamePlate() {
            //Debug.Log($"{gameObject.name}.NamePlateUnit.InitializenamePlate() namePlateReady: " + namePlateReady);
            // account for characters that spawn dead 
            if (namePlateReady == true) {
                return false;
            }
            if (NamePlateController.InitializeNamePlate()) {
                OnInitializeNamePlate();
                return true;
            }
            return false;
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log($"{gameObject.name}.Interactable.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            if (!playerManager.PlayerUnitSpawned) {
                //Debug.Log($"{gameObject.name}.Interactable.HandlePrerequisiteUpdates(): player unit not spawned.  returning");
                return;
            }
            UpdateNamePlateImage();
        }

        public void UpdateNamePlateImage() {
            //Debug.Log($"{gameObject.name}.NamePlateUnit.UpdateNamePlateImage()");

            if (playerManager.UnitController == null || playerManager.UnitController == null) {
                //Debug.Log($"{gameObject.name}.Interactable.UpdateNamePlateImage(): player has no character");
                return;
            }
            // if there is a nameplate unit give it a chance to initialize its nameplate.
            // inanimate units cannot be directly interacted with and are not interactableoptions so they won't receive prerequisite updates directly
            // this means the only way they can spawn their nameplate is through a direct call
            if (NamePlateController.NamePlate == null) {
                InitializeNamePlateController();
                if (NamePlateController.NamePlate == null) {
                    return;
                }
            }

            List<InteractableOptionComponent> currentInteractables = GetCurrentInteractables();

            int currentInteractableCount = currentInteractables.Count;
            //Debug.Log($"{gameObject.name}.Interactable.UpdateDialogStatus(): currentInteractableCount: " + currentInteractableCount);

            // determine if one of our current interactables is a questgiver
            bool questGiverCurrent = false;
            foreach (InteractableOptionComponent interactableOption in currentInteractables) {
                if (interactableOption is QuestGiverComponent) {
                    questGiverCurrent = true;
                    (interactableOption as QuestGiverComponent).UpdateQuestStatus();
                }
            }

            if (currentInteractableCount == 0 || questGiverCurrent == true) {
                // questgiver should override all other nameplate images since it's special and appears separately
                NamePlateController.NamePlate.GenericIndicatorImage.gameObject.SetActive(false);
            } else {
                if (currentInteractableCount == 1) {
                    // there is only one interactable.  set the specific nameplate image for it
                    if (currentInteractables[0].InteractableOptionProps.NamePlateImage != null) {
                        NamePlateController.NamePlate.GenericIndicatorImage.gameObject.SetActive(true);
                        NamePlateController.NamePlate.GenericIndicatorImage.sprite = currentInteractables[0].InteractableOptionProps.NamePlateImage;
                    }
                } else {
                    // set a generic indicator if there is more than 1 interactable
                    NamePlateController.NamePlate.GenericIndicatorImage.gameObject.SetActive(true);
                    NamePlateController.NamePlate.GenericIndicatorImage.sprite = systemConfigurationManager.MultipleInteractionNamePlateImage;
                }
            }
        }

        public override void ProcessBeginDialog() {
            base.ProcessBeginDialog();
            if (NamePlateController != null &&  NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.ShowSpeechBubble();
            }
        }

        public override void ProcessEndDialog() {
            base.ProcessEndDialog();
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.HideSpeechBubble();
            }
        }

        public override void ProcessDialogTextUpdate(string newText) {
            //Debug.Log($"{gameObject.name}.NamePlateUnit.ProcessDialogTextUpdate(" + newText + ")");

            base.ProcessDialogTextUpdate(newText);
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.SetSpeechText(newText);
            }
        }

        public override void ProcessShowQuestIndicator(string indicatorText, QuestGiverComponent questGiverComponent) {
            base.ProcessShowQuestIndicator(indicatorText, questGiverComponent);
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.QuestIndicatorBackground.SetActive(true);
                //Debug.Log($"{gameObject.name}:QuestGiver.UpdateQuestStatus() Indicator is active.  Setting to: " + indicatorType);
                questGiverComponent.SetIndicatorText(indicatorText, NamePlateController.NamePlate.QuestIndicator);
            }
        }

        public override void ProcessHideQuestIndicator() {
            base.ProcessHideQuestIndicator();
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.QuestIndicatorBackground.SetActive(false);
            }
        }

        public override Color GetGlowColor() {
            return Faction.GetFactionColor(playerManager, this);
        }

        public virtual void ConfigureUnitFrame(UnitFrameController unitFrameController) {
            unitFrameController.ConfigureSnapshotPortrait();
        }

        public override Color GetDescriptionColor() {
            if (NamePlateController != null && NamePlateController.Faction != null) {
                return Faction.GetFactionColor(playerManager, this);
            }
            return base.GetDescriptionColor();
        }

        public override string GetTitleString() {
            if (NamePlateController != null && NamePlateController.Faction != null) {
                return "\n" + NamePlateController.Faction.DisplayName;
            }
            return base.GetTitleString();
        }

        public override void ResetSettings() {
            RemoveNamePlate();
            cameraTargetReady = true;

            // base is intentionally last because we want to uninitialize children first
            base.ResetSettings();
        }

        public void RemoveNamePlate() {
            //Debug.Log($"{gameObject.name}.NamePlateUnit.RemoveNamePlate()");
            namePlateController?.RemoveNamePlate();
            namePlateReady = false;
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            namePlateController = new BaseNamePlateController(this, systemGameManager);
            if (startHasRun && namePlateController != null) {
                namePlateController.InitializeNamePlate();
            }
        }

        // some nameplates seem to get removed late due to async loading
        // attempt to remove them before the level load to avoid this
        public override void ProcessLevelUnload() {
            base.ProcessLevelUnload();
            RemoveNamePlate();
        }

        public virtual void OnDisable() {
            // characters can get disabled by cutscenes, so need to remove nameplate
            RemoveNamePlate();
        }

        // this method needs to exist to allow timeline controlled units to add a nameplate when enabled
        public void OnEnable() {
            // characters can get disabled by cutscenes, so need to initialize nameplate on re-enable
            if (startHasRun && namePlateController != null) {
                namePlateController.InitializeNamePlate();
            }
        }

        public override void ConfigureDialogPanel(DialogPanelController dialogPanelController) {
            dialogPanelController.ConfigureSnapshotPortrait();
        }
    }

}