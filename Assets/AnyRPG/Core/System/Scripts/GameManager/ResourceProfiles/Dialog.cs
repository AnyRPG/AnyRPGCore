using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    //[System.Serializable]
    [CreateAssetMenu(fileName = "New Dialog", menuName = "AnyRPG/Dialog")]
    public class Dialog : DescribableResource, IPrerequisiteOwner {

        public event System.Action<UnitController> OnDialogCompleted = delegate { };

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
        public bool TurnedIn(UnitController sourceUnitController) {
            return sourceUnitController.CharacterSaveManager.GetDialogSaveData(this).TurnedIn;
        }

        public void NotifyOnDialogCompleted(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.Dialog.NotifyOnDialogCompleted({sourceUnitController.gameObject.name})");

            OnDialogCompleted(sourceUnitController);
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


        public virtual void UpdatePrerequisites(UnitController sourceUnitController, bool notify = true) {
            //Debug.Log($"{gameObject.name}.Dialog.UpdatePrerequisites()");
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, false);
                    }
                }
            } else {
                //HandlePrerequisiteUpdates();
            }
            //HandlePrerequisiteUpdates();
        }

        public bool PrerequisitesMet(UnitController sourceUnitController) {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
        }

        public List<DialogNode> DialogNodes { get => dialogNodes; set => dialogNodes = value; }
        public bool Automatic { get => automatic; set => automatic = value; }
        //public AudioProfile AudioProfile { get => audioProfile; set => audioProfile = value; }
        public bool Repeatable { get => repeatable; set => repeatable = value; }

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
                    for (int i = 0; i < dialogNodes.Count; i++) {
                        if (audioProfile.AudioClips != null && audioProfile.AudioClips.Count > i && dialogNodes[i].AudioClip == null) {
                            dialogNodes[i].AudioClip = audioProfile.AudioClips[i];
                        }
                    }
                } else {
                    Debug.LogError("Dialog.SetupScriptableObjects(): Could not find audioProfile " + audioProfileName + " while initializing " + ResourceName);
                }
            }

            foreach (DialogNode dialogNode in dialogNodes) {
                if (dialogNode != null && dialogNode.AudioClip != null) {
                    systemGameManager.AudioManager.RegisterAudioClip(dialogNode.AudioClip);
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

        public void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".Dialog.HandlePrerequisiteUpdates()");
            if (prerequisiteOwners != null) {
                // this event is for the interactable that will display this dialog and needs to know when it becomes available
                //Debug.Log(DisplayName + ".Dialog.HandlePrerequisiteUpdates()");
                foreach (IPrerequisiteOwner prerequisiteOwner in prerequisiteOwners) {
                    prerequisiteOwner.HandlePrerequisiteUpdates(sourceUnitController);
                }
            }
        }
    }
}