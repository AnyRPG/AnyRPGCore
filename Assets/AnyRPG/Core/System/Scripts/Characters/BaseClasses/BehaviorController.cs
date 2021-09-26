using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorController : ConfiguredClass {

        private UnitController unitController = null;

        private int behaviorIndex = 0;

        private float maxBehaviorTime = 300f;

        private Coroutine behaviorCoroutine = null;

        private bool behaviorPlaying = false;

        private bool suppressNameplateImage = false;

        private Dictionary<BehaviorProfile, BehaviorProfileState> behaviorList = new Dictionary<BehaviorProfile, BehaviorProfileState>();

        private BehaviorComponent behaviorComponent = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private AudioManager audioManager = null;
        private LevelManager levelManager = null;

        public int BehaviorIndex { get => behaviorIndex; }
        public bool BehaviorPlaying { get => behaviorPlaying; set => behaviorPlaying = value; }
        public bool SuppressNameplateImage { get => suppressNameplateImage; }
        public Dictionary<BehaviorProfile, BehaviorProfileState> BehaviorList { get => behaviorList; set => behaviorList = value; }

        public BehaviorController(UnitController unitController, SystemGameManager systemGameManager) {
            //Debug.Log(unitController.gameObject.name + "BehaviorController.Constructor()");

            this.unitController = unitController;
            Configure(systemGameManager);

            SetupScriptableObjects();
            //HandlePrerequisiteUpdates();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
            levelManager = systemGameManager.LevelManager;
        }

        // this should be run after the unit profile is set
        public void Init() {
            //Debug.Log(unitController.gameObject.name + "BehaviorController.Init()");

            behaviorComponent = BehaviorComponent.GetBehaviorComponent(unitController);

            PlayAutomaticBehaviors();
        }


        public void Cleanup() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupDialog();
            CleanupScriptableObjects();
        }

        public void SetBehaviorPlaying(bool newValue) {
            behaviorPlaying = newValue;
        }

        public void TryPlayBehavior(BehaviorProfile behaviorProfile, BehaviorComponent caller = null) {
            //Debug.Log(unitController.gameObject.name + ".BehaviorInteractable.TryPlayBehavior()");

            if (behaviorPlaying == false) {
                behaviorCoroutine = unitController.StartCoroutine(PlayBehavior(behaviorProfile, caller));
            }
        }

        private void CleanupDialog() {
            //nameplate
            if (behaviorCoroutine != null) {
                unitController.StopCoroutine(behaviorCoroutine);
            }
            behaviorCoroutine = null;
            SetBehaviorPlaying(false);
            if (unitController != null && unitController.NamePlateController.NamePlate != null) {
                unitController.NamePlateController.NamePlate.HideSpeechBubble();
            }
        }

        public IEnumerator PlayBehavior(BehaviorProfile behaviorProfile, BehaviorComponent caller = null) {
            //Debug.Log(unitController.gameObject.name + ".BehaviorController.PlayBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.DisplayName) + ")");

            SetBehaviorPlaying(true);

            float elapsedTime = 0f;
            behaviorIndex = 0;
            BehaviorNode currentbehaviorNode = null;
            suppressNameplateImage = true;

            behaviorProfile.ResetStatus(behaviorList[behaviorProfile]);

            // give the interactable a chance to update the nameplate image and minimap indicator since we want the option to interact to be gone while the behavior is playing
            if (caller != null) {
                caller.ProcessBehaviorBeginEnd();
            }
            //ProcessBehaviorBeginEnd();
            while (behaviorIndex < behaviorProfile.BehaviorNodes.Count) {
                foreach (BehaviorNode behaviorNode in behaviorProfile.BehaviorNodes) {
                    if (behaviorNode.StartTime <= elapsedTime && behaviorList[behaviorProfile].BehaviorNodeStates[behaviorNode].Completed == false) {
                        currentbehaviorNode = behaviorNode;

                        if (currentbehaviorNode.BehaviorActionNodes != null) {
                            foreach (BehaviorActionNode behaviorActionNode in currentbehaviorNode.BehaviorActionNodes) {
                                if (behaviorActionNode.BehaviorMethod != null && behaviorActionNode.BehaviorMethod != string.Empty) {
                                    //Debug.Log(unitController.gameObject.name + ".BehaviorInteractable.playBehavior(): sending Message " + behaviorActionNode.MyBehaviorMethod + "(" + behaviorActionNode.MyBehaviorParameter + ")");
                                    if (behaviorActionNode.BehaviorParameter != null && behaviorActionNode.BehaviorParameter != string.Empty) {
                                        unitController.gameObject.SendMessage(behaviorActionNode.BehaviorMethod, behaviorActionNode.BehaviorParameter, SendMessageOptions.DontRequireReceiver);
                                    } else {
                                        unitController.gameObject.SendMessage(behaviorActionNode.BehaviorMethod, SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                            }
                        }

                        behaviorList[behaviorProfile].BehaviorNodeStates[behaviorNode].Completed = true;
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
            //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.DisplayName) + ") : END LOOP");
            behaviorCoroutine = null;
            SetBehaviorPlaying(false);
            suppressNameplateImage = false;
            behaviorProfile.Completed = true;

            // give the interactable a chance to update the nameplate image and minimap indicator since we want the option to interact to be gone while the behavior is playing
            //ProcessBehaviorBeginEnd();
            if (caller != null) {
                caller.ProcessBehaviorBeginEnd();
            }

            // hope this doesn't cause stack overflow ?  it shouldn't because technically this one exits immediately after that call ?
            if (behaviorProfile.Looping == true) {
                behaviorCoroutine = unitController.StartCoroutine(PlayBehavior(behaviorProfile));
            }
        }

        /*
        public void ProcessBehaviorBeginEnd() {
            HandlePrerequisiteUpdates();
        }
        */

        public void HandlePrerequisiteUpdates() {
            //Debug.Log(unitController.gameObject.name + ".BehaviorController.HandlePrerequisiteUpdates()");
            if (unitController.UnitControllerMode != UnitControllerMode.AI) {
                return;
            }

            PlayAutomaticBehaviors();

            if (behaviorComponent != null) {
                behaviorComponent.HandlePrerequisiteUpdates();
            }
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log(unitController.gameObject.name + ".BehaviorController.HandlePlayerUnitSpawn()");
            if (unitController.UnitControllerMode != UnitControllerMode.AI) {
                return;
            }

            // since player unit spawn doesn't trigger prerequisite update on individual behaviors, a manual check is needed
            foreach (BehaviorProfile behaviorProfile in behaviorList.Keys) {
                behaviorProfile.UpdatePrerequisites(false);
            }
            PlayAutomaticBehaviors();
            // the behavior component may have already triggered on this event, so trigger it manually since a prerequisite update was just performed
            if (behaviorComponent != null) {
                behaviorComponent.HandlePlayerUnitSpawn();
            }
        }


        public void PlayAutomaticBehaviors() {
            //Debug.Log(unitController.gameObject.name + ".Controller.PlayAutomaticBehaviors()");

            if (unitController.UnitControllerMode != UnitControllerMode.AI) {
                return;
            }

            foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                if (behaviorProfile.Automatic == true && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)) {
                    TryPlayBehavior(behaviorProfile);
                }
            }
        }

        public List<BehaviorProfile> GetCurrentOptionList() {
            //Debug.Log(unitController.gameObject.name + ".BehaviorController.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            foreach (BehaviorProfile behaviorProfile in behaviorList.Keys) {
                if (behaviorProfile.PrerequisitesMet == true
                    && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)) {
                    //Debug.Log("BehaviorInteractable.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.DisplayName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(behaviorProfile);
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public void AddToBehaviorList(BehaviorProfile behaviorProfile) {
            behaviorList.Add(behaviorProfile, new BehaviorProfileState(behaviorProfile));
            behaviorProfile.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
        }

        public void SetupScriptableObjects() {
            //Debug.Log(unitController.gameObject.name + ".BehaviorController.SetupScriptableObjects()");

            // local behaviors
            if (unitController.BehaviorNames != null) {
                foreach (string behaviorName in unitController.BehaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                        tmpBehaviorProfile = systemDataFactory.GetResource<BehaviorProfile>(behaviorName);
                    if (tmpBehaviorProfile != null) {
                        AddToBehaviorList(tmpBehaviorProfile);
                    }
                }
            }

        }

        public void CleanupScriptableObjects() {
            foreach (BehaviorProfile behaviorProfile in behaviorList.Keys) {
                behaviorProfile.OnPrerequisiteUpdates -= HandlePrerequisiteUpdates;
            }
        }

        public void StopBackgroundMusic() {
            audioManager.StopMusic();
        }

        public void StartBackgroundMusic() {
            levelManager.PlayLevelSounds();
        }


    }

}