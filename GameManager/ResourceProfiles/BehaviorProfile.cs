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


        [SerializeField]
        private List<BehaviorNode> behaviorNodes = new List<BehaviorNode>();

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        // should this dialog open in a speech bubble and automatically progress
        [SerializeField]
        private bool automatic = false;

        // track whether it is completed to prevent it from repeating if it is automatic
        private bool completed = false;

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
        public bool MyCompleted {
            get => completed;
            set {
                //Debug.Log(MyName + ".BehaviorProfile.MyCompleted = " + value + "; id: " + GetInstanceID());
                completed = value;
            } 
        }

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
    }
}