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
    public class BehaviorProfile : DescribableResource, IPrerequisiteOwner, IEventTriggerOwner {

        public event System.Action<UnitController> OnPrerequisiteUpdates = delegate { };
        public event System.Action<BehaviorProfile> OnEventTriggered = delegate { };

        [Header("Behavior")]

        [SerializeField]
        private List<BehaviorNode> behaviorNodes = new List<BehaviorNode>();

        [Header("Conditions")]

        [Tooltip("Game conditions that must be satisfied for this behavior to be available.")]
        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [Header("Event Triggers")]

        [Tooltip("Event triggers that will cause this behavior to play.")]
        [SerializeField]
        private List<EventTriggers> eventTriggers = new List<EventTriggers>();

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

        // game manager references
        //protected SaveManager saveManager = null;

        public bool PrerequisitesMet(UnitController sourceUnitController) {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
        }

        public List<BehaviorNode> BehaviorNodes { get => behaviorNodes; set => behaviorNodes = value; }
        public bool Automatic { get => automatic; set => automatic = value; }

        public bool Repeatable { get => repeatable; set => repeatable = value; }
        public bool Looping { get => looping; set => looping = value; }
        public bool AllowManualStart { get => allowManualStart; set => allowManualStart = value; }
        public List<PrerequisiteConditions> PrerequisiteConditions { get => prerequisiteConditions; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //saveManager = systemGameManager.SaveManager;
        }

        public void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.BehaviorProfile.HandlePrerequisiteUpdates()");

            // call back to owner
            OnPrerequisiteUpdates(sourceUnitController);
        }

        public void HandleEventTriggered() {
            //Debug.Log($"{ResourceName}.BehaviorProfile.HandleEventTriggered()");

            OnEventTriggered(this);
        }

        public bool Completed(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.BehaviorProfile.Completed({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)})");

            return sourceUnitController.CharacterSaveManager.GetBehaviorSaveData(this).Completed;
        }

        public void SetCompleted(UnitController sourceUnitController, bool value) {
            // if this behaviour was not triggered by a player, we cannot save the state
            if (sourceUnitController == null) {
                return;
            }
            sourceUnitController.CharacterSaveManager.SetBehaviorCompleted(this, value);
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
            foreach (EventTriggers _eventTriggers in eventTriggers) {
                if (_eventTriggers != null) {
                    _eventTriggers.SetupScriptableObjects(systemGameManager, this);
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
            foreach (EventTriggers _eventTriggers in eventTriggers) {
                if (_eventTriggers != null) {
                    _eventTriggers.CleanupScriptableObjects(this);
                }
            }
        }

        public void UpdatePrerequisites(UnitController sourceUnitController, bool notify = true) {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, notify);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the completion status of this profile and all its nodes
        /// </summary>
        public void ResetStatus(UnitController sourceUnitController, BehaviorProfileState behaviorProfileState) {
            if (repeatable == true) {
                SetCompleted(sourceUnitController, false);
                foreach (BehaviorNode behaviorNode in behaviorNodes) {
                    behaviorNode.ResetStatus(behaviorProfileState.BehaviorNodeStates[behaviorNode]);
                }
            }
        }

    }
}