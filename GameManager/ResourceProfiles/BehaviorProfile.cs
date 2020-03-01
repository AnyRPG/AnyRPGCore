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
    public class BehaviorProfile : DescribableResource {

        [SerializeField]
        private List<BehaviorNode> behaviorNodes = new List<BehaviorNode>();

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        // should this dialog open in a speech bubble and automatically progress
        [SerializeField]
        private bool automatic = false;

        // track whether it is completed to prevent it from repeating if it is automatic
        private bool completed = false;

        /*
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
                    SystemEventManager.MyInstance.NotifyOnDialogCompleted(this);
                }
            }
        }
        */

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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects();
                    }
                }
            }
        }
    }
}