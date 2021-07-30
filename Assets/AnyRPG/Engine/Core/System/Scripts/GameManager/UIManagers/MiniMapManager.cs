using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MiniMapManager : MonoBehaviour {

        // events
        public event System.Action<Interactable> OnAddIndicator = delegate { };
        public event System.Action<Interactable> OnRemoveIndicator = delegate { };
        public event System.Action<Interactable> OnUpdateIndicatorRotation = delegate { };
        public event System.Action<Interactable, InteractableOptionComponent> OnInteractableStatusUpdate = delegate { };

        // state
        protected bool eventSubscriptionsInitialized = false;

        // indicators
        private List<Interactable> mapIndicatorControllers = new List<Interactable>();

        public List<Interactable> MapIndicatorControllers { get => mapIndicatorControllers; set => mapIndicatorControllers = value; }

        public void Init() {
            CreateEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<Interactable> removeList = new List<Interactable>();
            removeList.AddRange(mapIndicatorControllers);
            foreach (Interactable interactable in removeList) {
                mapIndicatorControllers.Remove(interactable);
                OnRemoveIndicator(interactable);
            }
        }

        public void AddIndicator(Interactable interactable) {
            //Debug.Log("MainMapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.Contains(interactable) == false) {
                mapIndicatorControllers.Add(interactable);
                OnAddIndicator(interactable);
            }

            //return mapIndicatorControllers[interactable];
        }

        public void RemoveIndicator(Interactable interactable) {
            if (mapIndicatorControllers.Contains(interactable)) {
                mapIndicatorControllers.Remove(interactable);
                OnRemoveIndicator(interactable);
            }
        }

        public void InteractableStatusUpdate(Interactable interactable, InteractableOptionComponent interactableOptionComponent) {
            OnInteractableStatusUpdate(interactable, interactableOptionComponent);
        }

        public void UpdateIndicatorRotation(Interactable interactable) {
            OnUpdateIndicatorRotation(interactable);
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("MainMapController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = false;
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }


    }

}