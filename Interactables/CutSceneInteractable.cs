using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private CutsceneProps interactableOptionProps = new CutsceneProps();

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        [SerializeField]
        private string cutsceneName = string.Empty;

        private Cutscene cutscene = null;

        public CutSceneInteractable(Interactable interactable, CutsceneProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);
            //Debug.Log(gameObject.name + ".CutSceneInteractable.Interact()");
            // save character position and stuff here
            //PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (cutscene != null) {
                if (cutscene.Viewed == false || cutscene.Repeatable == true) {
                    if (cutscene.RequirePlayerUnitSpawn == false || (cutscene.RequirePlayerUnitSpawn == true && PlayerManager.MyInstance.PlayerUnitSpawned == true)) {
                        if (cutscene.MyLoadScene != null) {
                            LevelManager.MyInstance.LoadCutSceneWithDelay(cutscene);
                        } else {
                            UIManager.MyInstance.MyCutSceneBarController.StartCutScene(cutscene);
                        }
                    }
                }
            }
            // CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            //PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
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
            text.color = Color.white;
            return true;
        }

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
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

            if (cutsceneName != null && cutsceneName != string.Empty) {
                Cutscene tmpCutscene = SystemCutsceneManager.MyInstance.GetResource(cutsceneName);
                if (tmpCutscene != null) {
                    cutscene = tmpCutscene;
                } else {
                    Debug.LogError("CutSceneInteractable.SetupScriptableObjects(): Could not find cutscene : " + cutsceneName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}