using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class EventTriggers : ConfiguredClass {

        [SerializeField]
        private List<UseInteractableTrigger> interactionTriggers = new List<UseInteractableTrigger>();

        private List<IEventTriggerOwner> eventTriggerOwners = new List<IEventTriggerOwner>();

        List<List<IEventTrigger>> allEventTriggers = new List<List<IEventTrigger>>();


        // game manager references
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        private void HandleEventTriggered() {
            //Debug.Log("EventTriggers.HandleEventTriggered()");

            foreach (IEventTriggerOwner eventTriggerOwner in eventTriggerOwners) {
                if (eventTriggerOwner != null) {
                    eventTriggerOwner.HandleEventTriggered();
                }
            }
        }

        private void CreateAllList() {
            if (allEventTriggers.Count == 0) {
                allEventTriggers.Add(interactionTriggers.Cast<IEventTrigger>().ToList());
            }
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, IEventTriggerOwner eventTriggerOwner) {
            //Debug.Log($"EventTriggers.SetupScriptableObjects({eventTriggerOwner.DisplayName})");

            Configure(systemGameManager);
            CreateAllList();

            if (eventTriggerOwners.Count == 0) {
                foreach (List<IEventTrigger> eventTriggerList in allEventTriggers) {
                    foreach (IEventTrigger eventTrigger in eventTriggerList) {
                        eventTrigger.SetupScriptableObjects(systemGameManager, eventTriggerOwner.DisplayName);
                        eventTrigger.OnEventTriggered += HandleEventTriggered;
                    }
                }
            }

            eventTriggerOwners.Add(eventTriggerOwner);
        }

        public void CleanupScriptableObjects(IEventTriggerOwner eventTriggerOwner) {

            eventTriggerOwners.Remove(eventTriggerOwner);

            if (eventTriggerOwners.Count == 0) {
                foreach (List<IEventTrigger> eventTriggerList in allEventTriggers) {
                    foreach (IEventTrigger eventTrigger in eventTriggerList) {
                        eventTrigger.CleanupScriptableObjects();
                        eventTrigger.OnEventTriggered -= HandleEventTriggered;
                    }
                }
            }
        }

    }

}