using System.Collections.Generic;

namespace AnyRPG {
    public class MiniMapManager : ConfiguredMonoBehaviour {

        // events
        public event System.Action<Interactable> OnAddIndicator = delegate { };
        public event System.Action<Interactable> OnRemoveIndicator = delegate { };
        public event System.Action<Interactable> OnUpdateIndicatorRotation = delegate { };
        public event System.Action<Interactable, InteractableOptionComponent> OnInteractableStatusUpdate = delegate { };

        // state
        protected bool eventSubscriptionsInitialized = false;

        // indicators
        private List<Interactable> mapIndicatorControllers = new List<Interactable>();

        // game manager references
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManagerClient levelManagerClient = null;

        public List<Interactable> MapIndicatorControllers { get => mapIndicatorControllers; set => mapIndicatorControllers = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            //Debug.Log($"MiniMapManager.HandleLevelUnload({sceneHandle}, {sceneName})");

            List<Interactable> removeList = new List<Interactable>();
            removeList.AddRange(mapIndicatorControllers);
            foreach (Interactable interactable in removeList) {
                mapIndicatorControllers.Remove(interactable);
                OnRemoveIndicator(interactable);
            }
        }

        public void AddIndicator(Interactable interactable) {
            
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
            levelManagerClient.OnLevelUnload += HandleLevelUnload;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            levelManagerClient.OnLevelUnload -= HandleLevelUnload;
            eventSubscriptionsInitialized = false;
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }


    }

}