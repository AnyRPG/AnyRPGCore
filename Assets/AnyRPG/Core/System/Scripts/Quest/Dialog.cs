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

        private List<IPrerequisiteOwner> prerequisiteOwners = new List<IPrerequisiteOwner>();

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemEventManager systemEventManager = null;

        /// <summary>
        /// Track whether this dialog has been turned in
        /// </summary>
        public bool TurnedIn {
            get {
                return saveManager.GetDialogSaveData(this).turnedIn;
                //return false;
            }
            set {
                DialogSaveData saveData = saveManager.GetDialogSaveData(this);
                saveData.turnedIn = value;
                saveManager.DialogSaveDataDictionary[saveData.DialogName] = saveData;
                if (saveData.turnedIn == true) {
                    //Debug.Log(DisplayName + ".Dialog.TurnedIn = true");
                    // these events are for things that need the dialog turned in as a prerequisite
                    systemEventManager.NotifyOnDialogCompleted(this);
                    OnDialogCompleted();
                }

            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void RegisterPrerequisiteOwner(IPrerequisiteOwner prerequisiteOwner) {
            //Debug.Log(DisplayName + ".Dialog.RegisterPrerequisiteOwner()");
            if (prerequisiteOwners.Contains(prerequisiteOwner) == false) {
                prerequisiteOwners.Add(prerequisiteOwner);
            }
        }

        public void UnregisterPrerequisiteOwner(IPrerequisiteOwner prerequisiteOwner) {
            //Debug.Log(DisplayName + ".Dialog.RegisterPrerequisiteOwner()");
            if (prerequisiteOwners.Contains(prerequisiteOwner) == true) {
                prerequisiteOwners.Remove(prerequisiteOwner);
            }
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

        public bool PrerequisitesMet {
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

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
                    }
                }
            }

            if (audioProfileName != null && audioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
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
            if (prerequisiteOwners != null) {
                // this event is for the interactable that will display this dialog and needs to know when it becomes available
                //Debug.Log(DisplayName + ".Dialog.HandlePrerequisiteUpdates()");
                foreach (IPrerequisiteOwner prerequisiteOwner in prerequisiteOwners) {
                    prerequisiteOwner.HandlePrerequisiteUpdates();
                }
            }
        }
    }
}