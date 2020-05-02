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

        [Tooltip("This should be set to true for cutscene subtitles and npc speech bubble monologues to allow them to advance on a timer")]
        [SerializeField]
        private bool automatic = false;

        [SerializeField]
        private string audioProfileName = string.Empty;

        private AudioProfile audioProfile;

        [SerializeField]
        private List<DialogNode> dialogNodes = new List<DialogNode>();

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        private IPrerequisiteOwner prerequisiteOwner = null;

        /// <summary>
        /// Track whether this dialog has been turned in
        /// </summary>
        private bool turnedIn = false;

        public bool TurnedIn {
            get {
                return turnedIn;
            }

            set {
                turnedIn = value;
                if (turnedIn == true) {
                    // these events are for things that need the dialog turned in as a prerequisite
                    SystemEventManager.MyInstance.NotifyOnDialogCompleted(this);
                    OnDialogCompleted();
                }
            }
        }

        public void RegisterPrerequisiteOwner(IPrerequisiteOwner prerequisiteOwner) {
            this.prerequisiteOwner = prerequisiteOwner;
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
                    Debug.LogError("Dialog.SetupScriptableObjects(): COULD NOT FIND audioProfile " + audioProfileName + " WHILE INITIALIZING " + MyName);
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
            if (prerequisiteOwner != null) {
                // this event is for the interactable that will display this dialog and needs to know when it becomes available
                prerequisiteOwner.HandlePrerequisiteUpdates();
            }
        }
    }
}