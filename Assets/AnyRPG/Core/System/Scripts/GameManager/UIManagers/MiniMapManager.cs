using System.Collections.Generic;

namespace AnyRPG {
    public class MiniMapManager : ConfiguredMonoBehaviour {

        // events
        public event System.Action<InteractableBase> OnAddIndicator = delegate { };
        public event System.Action<InteractableBase> OnRemoveIndicator = delegate { };
        public event System.Action<InteractableBase> OnUpdateIndicatorRotation = delegate { };
        public event System.Action<InteractableBase, InteractableOptionComponent> OnInteractableStatusUpdate = delegate { };

        // state
        protected bool eventSubscriptionsInitialized = false;

        // indicators
        private List<InteractableBase> mapIndicatorControllers = new List<InteractableBase>();

        // game manager references
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManagerClient levelManagerClient = null;

        public List<InteractableBase> MapIndicatorControllers { get => mapIndicatorControllers; set => mapIndicatorControllers = value; }

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

            List<InteractableBase> removeList = new List<InteractableBase>();
            removeList.AddRange(mapIndicatorControllers);
            foreach (InteractableBase interactable in removeList) {
                mapIndicatorControllers.Remove(interactable);
                OnRemoveIndicator(interactable);
            }
        }

        public void AddIndicator(InteractableBase interactable) {
            
            if (mapIndicatorControllers.Contains(interactable) == false) {
                mapIndicatorControllers.Add(interactable);
                OnAddIndicator(interactable);
            }

            //return mapIndicatorControllers[interactable];
        }

        public void RemoveIndicator(InteractableBase interactable) {
            if (mapIndicatorControllers.Contains(interactable)) {
                mapIndicatorControllers.Remove(interactable);
                OnRemoveIndicator(interactable);
            }
        }

        public void InteractableStatusUpdate(InteractableBase interactable, InteractableOptionComponent interactableOptionComponent) {
            OnInteractableStatusUpdate(interactable, interactableOptionComponent);
        }

        public void UpdateIndicatorRotation(InteractableBase interactable) {
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