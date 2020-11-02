using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorController {

        private UnitController unitController = null;

        private int behaviorIndex = 0;

        private float maxBehaviorTime = 300f;

        private Coroutine behaviorCoroutine = null;

        private bool suppressNameplateImage = false;

        private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        public int MyBehaviorIndex { get => behaviorIndex; }

        public BehaviorController(UnitController unitController) {
            this.unitController = unitController;
            AddUnitProfileSettings();
            HandlePrerequisiteUpdates();
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupDialog();
        }

        public void AddUnitProfileSettings() {
            if (unitController != null && unitController.UnitProfile != null) {
                if (unitController.UnitProfile.BehaviorProps.BehaviorNames != null) {
                    foreach (string behaviorName in unitController.UnitProfile.BehaviorProps.BehaviorNames) {
                        BehaviorProfile tmpBehaviorProfile = null;
                        if (unitController.UnitProfile.BehaviorProps.UseBehaviorCopy == true) {
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



        private void TryPlayBehavior(BehaviorProfile behaviorProfile) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.TryPlayBehavior()");
            if (behaviorCoroutine == null) {
                behaviorCoroutine = unitController.StartCoroutine(playBehavior(behaviorProfile));
            }
        }

        private void CleanupDialog() {
            //nameplate
            if (behaviorCoroutine != null) {
                unitController.StopCoroutine(behaviorCoroutine);
            }
            behaviorCoroutine = null;
            if (unitController != null && unitController.NamePlateController.NamePlate != null) {
                unitController.NamePlateController.NamePlate.HideSpeechBubble();
            }
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
                                        unitController.gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, behaviorActionNode.MyBehaviorParameter, SendMessageOptions.DontRequireReceiver);
                                    } else {
                                        unitController.gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, SendMessageOptions.DontRequireReceiver);
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
                behaviorCoroutine = unitController.StartCoroutine(playBehavior(behaviorProfile));
            }
        }

        public void ProcessBehaviorBeginEnd() {
            HandlePrerequisiteUpdates();
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.HandlePrerequisiteUpdates()");
            PlayAutomaticBehaviors();
        }

        public void HandlePlayerUnitSpawn() {
            foreach (BehaviorProfile behaviorProfile in behaviorList) {
                behaviorProfile.UpdatePrerequisites(false);
            }
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

        public List<BehaviorProfile> GetCurrentOptionList() {
            //Debug.Log("BehaviorInteractable.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            foreach (BehaviorProfile behaviorProfile in behaviorList) {
                if (behaviorProfile.MyPrerequisitesMet == true && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)) {
                    //Debug.Log("BehaviorInteractable.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.MyName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(behaviorProfile);
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }


        public void SetupScriptableObjects() {
            behaviorList = new List<BehaviorProfile>();
            if (unitController.BehaviorNames != null) {
                foreach (string behaviorName in unitController.BehaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                    if (unitController.UseBehaviorCopy == true) {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetNewResource(behaviorName);
                    } else {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetResource(behaviorName);
                    }
                    if (tmpBehaviorProfile != null) {
                        behaviorList.Add(tmpBehaviorProfile);
                        tmpBehaviorProfile.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
                    }
                }
            }
        }

        public void CleanupScriptableObjects() {
            foreach (BehaviorProfile behaviorProfile in behaviorList) {
                behaviorProfile.OnPrerequisiteUpdates -= HandlePrerequisiteUpdates;
            }
        }

        public void StopBackgroundMusic() {
            AudioManager.MyInstance.StopMusic();
        }

        public void StartBackgroundMusic() {
            LevelManager.MyInstance.PlayLevelSounds();
        }


    }

}