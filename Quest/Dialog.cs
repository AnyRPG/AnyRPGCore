using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    //[System.Serializable]
    [CreateAssetMenu(fileName = "New Dialog", menuName = "AnyRPG/Dialog/Dialog")]
    public class Dialog : DescribableResource, IPrerequisiteOwner {

        public event System.Action OnDialogCompleted = delegate { };

        [Header("Dialog")]

        [Tooltip("This should be set to true for cutscene subtitles and npc speech bubble monologues to allow them to advance on a timer")]
        [SerializeField]
        private bool automatic = false;

        [Tooltip("This should be set to true for dialogs that will be included in cutscenes to allow cutscene replay.")]
        [SerializeField]
        private bool repeatable = false;

        [Tooltip("The name of an audio profile to play when this dialog is started.")]
        [SerializeField]
        private string audioProfileName = string.Empty;

        private AudioProfile audioProfile;

        [SerializeField]
        private List<DialogNode> dialogNodes = new List<DialogNode>();

        [Tooltip("Game conditions that must be satisfied for this dialog to be available")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        private IPrerequisiteOwner prerequisiteOwner = null;

        /// <summary>
        /// Track whether this dialog has been turned in
        /// </summary>
        public bool TurnedIn {
            get {
                return SaveManager.MyInstance.GetDialogSaveData(this).turnedIn;
                //return false;
            }
            set {
                DialogSaveData saveData = SaveManager.MyInstance.GetDialogSaveData(this);
                saveData.turnedIn = value;
                SaveManager.MyInstance.DialogSaveDataDictionary[saveData.MyName] = saveData;
                if (saveData.turnedIn == true) {
                    //Debug.Log(DisplayName + ".Dialog.TurnedIn = true");
                    // these events are for things that need the dialog turned in as a prerequisite
                    SystemEventManager.MyInstance.NotifyOnDialogCompleted(this);
                    OnDialogCompleted();
                }

            }
        }

        public void RegisterPrerequisiteOwner(IPrerequisiteOwner prerequisiteOwner) {
            //Debug.Log(DisplayName + ".Dialog.RegisterPrerequisiteOwner()");
            this.prerequisiteOwner = prerequisiteOwner;
        }

        public virtual void UpdatePrerequisites(bool notify = true) {
            //Debug.Log(gameObject.name + ".Dialog.UpdatePrerequisites()");
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
            } else {
                //HandlePrerequisiteUpdates();
            }
            //HandlePrerequisiteUpdates();
        }

        public bool MyPrerequisitesMet {
            get {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }



        public List<DialogNode> MyDialogNodes { get => dialogNodes; set => dialogNodes = value; }
        public bool MyAutomatic { get => automatic; set => automatic = value; }
        public AudioProfile MyAudioProfile { get => audioProfile; set => audioProfile = value; }
        public bool Repeatable { get => repeatable; set => repeatable = value; }

        /// <summary>
        /// Set the shown value to false for all dialog Nodes and reset the turned in status
        /// </summary>
        public void ResetStatus() {
            if (repeatable == false) {
                return;
            }
            TurnedIn = false;
            foreach (DialogNode dialogNode in dialogNodes) {
                dialogNode.ResetStatus();
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(this);
                    }
                }
            }
            if (audioProfileName != null && audioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(audioProfileName);
                if (tmpAudioProfile != null) {
                    audioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("Dialog.SetupScriptableObjects(): COULD NOT FIND audioProfile " + audioProfileName + " WHILE INITIALIZING " + DisplayName);
                }
            }
        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects();
                    }
                }
            }
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log(DisplayName + ".Dialog.HandlePrerequisiteUpdates()");
            if (prerequisiteOwner != null) {
                // this event is for the interactable that will display this dialog and needs to know when it becomes available
                //Debug.Log(DisplayName + ".Dialog.HandlePrerequisiteUpdates()");
                prerequisiteOwner.HandlePrerequisiteUpdates();
            } else {

            }
        }
    }
}