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

        [Header("Dialog Settings")]

        [Tooltip("This should be set to true for npc speech bubble monologues to allow them to advance on a timer")]
        [SerializeField]
        private bool automatic = false;

        [Tooltip("If true, this dialog can be completed more than once")]
        [SerializeField]
        private bool repeatable = false;

        [Header("Dialog Properties")]

        [Tooltip("The name of an audio profile to play when this dialog is started.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string audioProfileName = string.Empty;

        private AudioProfile audioProfile;

        [SerializeField]
        private List<DialogNode> dialogNodes = new List<DialogNode>();

        [Header("Prerequisites")]

        [Tooltip("Game conditions that must be satisfied for this dialog to be available")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        private IPrerequisiteOwner prerequisiteOwner = null;

        /// <summary>
        /// Track whether this dialog has been turned in
        /// </summary>
        public bool TurnedIn {
            get {
                return SystemGameManager.Instance.SaveManager.GetDialogSaveData(this).turnedIn;
                //return false;
            }
            set {
                DialogSaveData saveData = SystemGameManager.Instance.SaveManager.GetDialogSaveData(this);
                saveData.turnedIn = value;
                SystemGameManager.Instance.SaveManager.DialogSaveDataDictionary[saveData.MyName] = saveData;
                if (saveData.turnedIn == true) {
                    //Debug.Log(DisplayName + ".Dialog.TurnedIn = true");
                    // these events are for things that need the dialog turned in as a prerequisite
                    SystemGameManager.Instance.SystemEventManager.NotifyOnDialogCompleted(this);
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



        public List<DialogNode> DialogNodes { get => dialogNodes; set => dialogNodes = value; }
        public bool Automatic { get => automatic; set => automatic = value; }
        public AudioProfile AudioProfile { get => audioProfile; set => audioProfile = value; }
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
                AudioProfile tmpAudioProfile = SystemDataFactory.Instance.GetResource<AudioProfile>(audioProfileName);
                if (tmpAudioProfile != null) {
                    audioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("Dialog.SetupScriptableObjects(): Could not find audioProfile " + audioProfileName + " while initializing " + DisplayName);
                }
            }
        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
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