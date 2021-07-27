using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MainMapManager : MonoBehaviour {

        #region Singleton
        private static MainMapManager instance;

        public static MainMapManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
            Init();
        }
        #endregion

        [SerializeField]
        private GameObject mapIndicatorPrefab = null;

        protected bool eventSubscriptionsInitialized = false;

        private Dictionary<Interactable, MainMapIndicatorController> mapIndicatorControllers = new Dictionary<Interactable, MainMapIndicatorController>();

        public Dictionary<Interactable, MainMapIndicatorController> MapIndicatorControllers { get => mapIndicatorControllers; }

        public void Init() {
            CreateEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<Interactable> removeList = new List<Interactable>();
            removeList.AddRange(mapIndicatorControllers.Keys);
            foreach (Interactable interactable in removeList) {
                RemoveIndicator(interactable);
            }
        }

        public MainMapIndicatorController AddIndicator(Interactable interactable) {
            //Debug.Log("MainMapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject mainMapIndicator = ObjectPooler.Instance.GetPooledObject(mapIndicatorPrefab, (PopupWindowManager.Instance.mainMapWindow.CloseableWindowContents as MainMapController).MapGraphic.transform);
                if (mainMapIndicator != null) {
                    MainMapIndicatorController mapIndicatorController = mainMapIndicator.GetComponent<MainMapIndicatorController>();
                    if (mapIndicatorController != null) {
                        mapIndicatorControllers.Add(interactable, mapIndicatorController);
                        mapIndicatorController.SetInteractable(interactable);
                    }
                }
            }

            return mapIndicatorControllers[interactable];
        }

        public void RemoveIndicator(Interactable interactable) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
                mapIndicatorControllers[interactable].ResetSettings();
                ObjectPooler.Instance.ReturnObjectToPool(mapIndicatorControllers[interactable].gameObject);
                mapIndicatorControllers.Remove(interactable);
            }
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