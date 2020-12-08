using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public BehaviorProps Props { get => interactableOptionProps as BehaviorProps; }

        private int behaviorIndex = 0;

        private float maxBehaviorTime = 300f;

        private Coroutine behaviorCoroutine = null;

        private bool suppressNameplateImage = false;

        public int BehaviorIndex { get => behaviorIndex; }

        public BehaviorComponent(Interactable interactable, BehaviorProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            foreach (BehaviorProfile behaviorProfile in Props.BehaviorList) {
                behaviorProfile.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
            }
        }

        /*
        public override void Init() {
            base.Init();
            HandlePrerequisiteUpdates();
        }
        */

        public override void Cleanup() {
            base.Cleanup();
            CleanupDialog();
        }

        /*
        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                if (unitProfile.BehaviorProps.BehaviorNames != null) {
                    foreach (string behaviorName in unitProfile.BehaviorProps.BehaviorNames) {
                        BehaviorProfile tmpBehaviorProfile = null;
                        if (unitProfile.BehaviorProps.UseBehaviorCopy == true) {
                            tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetNewResource(behaviorName);
                        } else {
                            tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetResource(behaviorName);
                        }
                        if (tmpBehaviorProfile != null) {
                            tmpBehaviorProfile.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
                            behaviorList.Add(tmpBehaviorProfile);
                        }
                    }
                }
            }
            HandlePrerequisiteUpdates();
        }
        */


        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.Interact()");
            List<BehaviorProfile> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else if (currentList.Count == 1) {
                TryPlayBehavior(currentList[0]);
                base.Interact(source);
                interactable.CloseInteractionWindow();
            } else {
                interactable.OpenInteractionWindow();
            }
            return true;
        }

        private void TryPlayBehavior(BehaviorProfile behaviorProfile) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.TryPlayBehavior()");
            if (behaviorCoroutine == null) {
                behaviorCoroutine = interactable.StartCoroutine(playBehavior(behaviorProfile));
            }
        }

        private void CleanupDialog() {
            //nameplate
            if (behaviorCoroutine != null) {
                interactable.StopCoroutine(behaviorCoroutine);
            }
            behaviorCoroutine = null;

            // is this really needed ?  If the component is being destroyed, then other code will remove the nameplate anyway
            // testing if anything breaks
            /*
            if (interactable != null && interactable.NamePlateController.NamePlate != null) {
                interactable.NamePlateController.NamePlate.HideSpeechBubble();
            }
            */
        }

        public IEnumerator playBehavior(BehaviorProfile behaviorProfile) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.MyName) + ")");
            float elapsedTime = 0f;
            behaviorIndex = 0;
            BehaviorNode currentbehaviorNode = null;
            suppressNameplateImage = true;

            behaviorProfile.ResetStatus();

            // give the interactable a chance to update the nameplate image and minimap indicator since we want the option to interact to be gone while the behavior is playing
            ProcessBehaviorBeginEnd();
            while (behaviorIndex < behaviorProfile.MyBehaviorNodes.Count) {
                foreach (BehaviorNode behaviorNode in behaviorProfile.MyBehaviorNodes) {
                    if (behaviorNode.MyStartTime <= elapsedTime && behaviorNode.MyCompleted == false) {
                        currentbehaviorNode = behaviorNode;

                        if (currentbehaviorNode.MyBehaviorActionNodes != null) {
                            foreach (BehaviorActionNode behaviorActionNode in currentbehaviorNode.MyBehaviorActionNodes) {
                                if (behaviorActionNode.MyBehaviorMethod != null && behaviorActionNode.MyBehaviorMethod != string.Empty) {
                                    //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(): sending Message " + behaviorActionNode.MyBehaviorMethod + "(" + behaviorActionNode.MyBehaviorParameter + ")");
                                    if (behaviorActionNode.MyBehaviorParameter != null && behaviorActionNode.MyBehaviorParameter != string.Empty) {
                                        interactable.gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, behaviorActionNode.MyBehaviorParameter, SendMessageOptions.DontRequireReceiver);
                                    } else {
                                        interactable.gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                            }
                        }

                        behaviorNode.MyCompleted = true;
                        behaviorIndex++;
                    }
                }
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxBehaviorTime) {
                    break;
                }
                yield return null;
            }
            //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.MyName) + ") : END LOOP");
            behaviorCoroutine = null;
            suppressNameplateImage = false;
            behaviorProfile.Completed = true;

            // give the interactable a chance to update the nameplate image and minimap indicator since we want the option to interact to be gone while the behavior is playing
            ProcessBehaviorBeginEnd();

            // hope this doesn't cause stack overflow ?  it shouldn't because technically this one exits immediately after that call ?
            if (behaviorProfile.Looping == true) {
                behaviorCoroutine = interactable.StartCoroutine(playBehavior(behaviorProfile));
            }
        }

        public List<BehaviorProfile> GetCurrentOptionList() {
            //Debug.Log("BehaviorInteractable.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            foreach (BehaviorProfile behaviorProfile in Props.BehaviorList) {
                if (behaviorProfile.MyPrerequisitesMet == true && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)) {
                    //Debug.Log("BehaviorInteractable.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.MyName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(behaviorProfile);
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.CanInteract()");
            if (!base.CanInteract()) {
                return false;
            }
            if (GetCurrentOptionCount() == 0 || suppressNameplateImage == true) {
                return false;
            }
            return true;

        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
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

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.GetCurrentOptionCount()");
            if (behaviorCoroutine == null) {
                //return GetCurrentOptionList().Count;
                int count = 0;
                foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                    if (behaviorProfile.AllowManualStart == true) {
                        count++;
                    }
                }
                return count;
            } else {
                return 0;
            }
        }

        public void ProcessBehaviorBeginEnd() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
            PlayAutomaticBehaviors();
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            foreach (BehaviorProfile behaviorProfile in Props.BehaviorList) {
                behaviorProfile.UpdatePrerequisites(false);
            }
            MiniMapStatusUpdateHandler(this);
            PlayAutomaticBehaviors();
        }


        public void PlayAutomaticBehaviors() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.PlayAutomaticBehaviors()");
            foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                if (behaviorProfile.MyAutomatic == true && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)) {
                    TryPlayBehavior(behaviorProfile);
                }
            }
        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (BehaviorProfile behaviorProfile in Props.BehaviorList) {
                behaviorProfile.OnPrerequisiteUpdates -= HandlePrerequisiteUpdates;
            }
        }

        public void StopBackgroundMusic() {
            AudioManager.MyInstance.StopMusic();
        }

        public void StartBackgroundMusic() {
            LevelManager.MyInstance.PlayLevelSounds();
        }

        public override bool CanShowMiniMapIcon() {
            if (suppressNameplateImage == true) {
                return false;
            }
            return base.CanShowMiniMapIcon();
        }

    }

}