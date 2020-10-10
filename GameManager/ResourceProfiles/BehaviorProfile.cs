using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    //[System.Serializable]
    [CreateAssetMenu(fileName = "New Behavior Profile", menuName = "AnyRPG/BehaviorProfile")]
    public class BehaviorProfile : DescribableResource, IPrerequisiteOwner {

        public event System.Action OnPrerequisiteUpdates = delegate { };

        [Header("Behavior")]

        [SerializeField]
        private List<BehaviorNode> behaviorNodes = new List<BehaviorNode>();

        [Header("Conditions")]

        [Tooltip("Game conditions that must be satisfied for this behavior to be available.")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [Header("Options")]

        [Tooltip("Should this behavior automatically play when the object that is referencing it is activated.")]
        [SerializeField]
        private bool automatic = false;

        [Tooltip("Can this behavior be started manually by interaction with the player")]
        [SerializeField]
        private bool allowManualStart = true;

        [Tooltip("Can this behavior be repeated or should it only play once per game.")]
        [SerializeField]
        private bool repeatable = false;

        [Tooltip("Should this behavior restart when complete.")]
        [SerializeField]
        private bool looping = false;

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

        public List<BehaviorNode> MyBehaviorNodes { get => behaviorNodes; set => behaviorNodes = value; }
        public bool MyAutomatic { get => automatic; set => automatic = value; }

        // track whether it is completed to prevent it from repeating if it is automatic
        public bool Completed {
            get {
                return SaveManager.MyInstance.GetBehaviorSaveData(this).completed;
            }
            set {
                BehaviorSaveData saveData = SaveManager.MyInstance.GetBehaviorSaveData(this);
                saveData.completed = value;
                SaveManager.MyInstance.BehaviorSaveDataDictionary[saveData.MyName] = saveData;
            }
        }

        public bool Repeatable { get => repeatable; set => repeatable = value; }
        public bool Looping { get => looping; set => looping = value; }
        public bool AllowManualStart { get => allowManualStart; set => allowManualStart = value; }

        public void HandlePrerequisiteUpdates() {
            // call back to owner
            OnPrerequisiteUpdates();
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

        public void UpdatePrerequisites(bool notify = true) {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(notify);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the completion status of this profile and all its nodes
        /// </summary>
        public void ResetStatus() {
            if (repeatable == true) {
                Completed = false;
                foreach (BehaviorNode behaviorNode in behaviorNodes) {
                    behaviorNode.ResetStatus();
                }
            }
        }

    }
}