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
    public class Dialog : DescribableResource {

        [SerializeField]
        private List<DialogNode> dialogNodes = new List<DialogNode>();

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        // should this dialog open in a speech bubble and automatically progress
        // also used to allow cutscenes to send messages to the dialog to advance it
        [SerializeField]
        private bool automatic = false;

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