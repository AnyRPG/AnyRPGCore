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

        [SerializeField]
        protected NamePlateProps namePlateProps = new NamePlateProps();

        // created components
        protected BaseNamePlateController namePlateController = null;

        // track startup state
        protected bool namePlateReady = false;

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

        protected override void Awake() {
            base.Awake();
            namePlateController = new BaseNamePlateController(this);
        }

        public override void ProcessInit() {
            //Debug.Log(gameObject.name + ".NamePlateUnit.ProcessInit()");
            base.ProcessInit();

            // InitializeNamePlateController called from interactable.HandlePrerequisiteUpdates
            //InitializeNamePlateController();
        }

        /// <summary>
        /// initialize a nameplate if it has not been initialied yet
        /// </summary>
        public virtual void InitializeNamePlateController() {
            //Debug.Log(gameObject.name + ".UnitController.InitializeNamePlateController()");
            if (namePlateReady == true) {
                return;
            }
            InitializeNamePlate();
            namePlateReady = true;
        }

        /// <summary>
        /// directly initialize a nameplate
        /// </summary>
        protected void InitializeNamePlate() {
            NamePlateController.InitializeNamePlate();
            OnInitializeNamePlate();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            if (!PlayerManager.MyInstance.PlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates(): player unit not spawned.  returning");
                return;
            }
            UpdateNamePlateImage();
        }

        public void UpdateNamePlateImage() {

            //Debug.Log(gameObject.name + ".NamePlateUnit.UpdateNamePlateImage()");
            if (PlayerManager.MyInstance.MyCharacter == null || PlayerManager.MyInstance.UnitController == null) {
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): player has no character");
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
            int currentInteractableCount = GetCurrentInteractables().Count;
            //Debug.Log(gameObject.name + ".Interactable.UpdateDialogStatus(): currentInteractableCount: " + currentInteractableCount);

            // determine if one of our current interactables is a questgiver
            bool questGiverCurrent = false;
            foreach (InteractableOptionComponent interactableOption in GetCurrentInteractables()) {
                if (interactableOption is QuestGiverComponent) {
                    questGiverCurrent = true;
                }
            }
            //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): MADE IT PAST QUESTIVER CHECK!!");

            if (currentInteractableCount == 0 || questGiverCurrent == true) {
                // questgiver should override all other nameplate images since it's special and appears separately
                NamePlateController.NamePlate.MyGenericIndicatorImage.gameObject.SetActive(false);
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): interactable count is zero or questgiver is true");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1 or more");
                if (currentInteractableCount == 1) {
                    //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1");
                    if (GetCurrentInteractables()[0].InteractableOptionProps.NamePlateImage != null) {
                        //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1 and image is not null");
                        NamePlateController.NamePlate.MyGenericIndicatorImage.gameObject.SetActive(true);
                        NamePlateController.NamePlate.MyGenericIndicatorImage.sprite = GetCurrentInteractables()[0].InteractableOptionProps.NamePlateImage;
                    } else {
                        //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1 and image is null");
                    }
                } else {
                    //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is MORE THAN 1");
                    NamePlateController.NamePlate.MyGenericIndicatorImage.gameObject.SetActive(true);
                    NamePlateController.NamePlate.MyGenericIndicatorImage.sprite = SystemConfigurationManager.MyInstance.MyMultipleInteractionNamePlateImage;
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
            //Debug.Log(gameObject.name + ".NamePlateUnit.ProcessDialogTextUpdate(" + newText + ")");

            base.ProcessDialogTextUpdate(newText);
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.SetSpeechText(newText);
            }
        }

        public override void ProcessShowQuestIndicator(string indicatorText, QuestGiverComponent questGiverComponent) {
            base.ProcessShowQuestIndicator(indicatorText, questGiverComponent);
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.MyQuestIndicatorBackground.SetActive(true);
                //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Indicator is active.  Setting to: " + indicatorType);
                questGiverComponent.SetIndicatorText(indicatorText, NamePlateController.NamePlate.MyQuestIndicator);
            }
        }

        public override void ProcessHideQuestIndicator() {
            base.ProcessHideQuestIndicator();
            if (NamePlateController != null && NamePlateController.NamePlate != null) {
                NamePlateController.NamePlate.MyQuestIndicatorBackground.SetActive(false);
            }
        }

        public override Color GetGlowColor() {
            return Faction.GetFactionColor(this);
        }

        public override Color GetDescriptionColor() {
            if (NamePlateController != null && NamePlateController.Faction != null) {
                return Faction.GetFactionColor(this);
            }
            return base.GetDescriptionColor();
        }

        public override string GetTitleString() {
            if (NamePlateController != null && NamePlateController.Faction != null) {
                return "\n" + NamePlateController.Faction.DisplayName;
            }
            return base.GetTitleString();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            //Debug.Log(gameObject.name + ".NamePlateUnit.OnDestroy()");
            if (NamePlateController != null) {
                NamePlateController.Cleanup();
            }
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".UnitController.OnDisable()");
            base.OnDisable();
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this);
            }
        }

        public virtual void OnEnable() {
            // characters can get disabled by cutscenes, so need to initialize nameplate on re-enable
            if (startHasRun && namePlateController != null) {
                namePlateController.InitializeNamePlate();
            }
        }

    }

}