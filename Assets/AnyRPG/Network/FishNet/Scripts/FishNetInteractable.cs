using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace AnyRPG {
    public class FishNetInteractable : SpawnedNetworkObject {

        private Interactable interactable = null;

        private bool eventRegistrationComplete = false;

        // game manager references
        protected SystemGameManager systemGameManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected SystemItemManager systemItemManager = null;
        protected AuthenticationService authenticationService = null;

        public Interactable Interactable { get => interactable; }

        protected virtual void Awake() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.Awake() position: { gameObject.transform.position}");
        }

        protected virtual void Configure() {
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            systemItemManager = systemGameManager.SystemItemManager;
            authenticationService = systemGameManager.AuthenticationService;
            
            interactable = GetComponent<Interactable>();
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStartClient()");

            base.OnStartClient();

            Configure();
            if (systemGameManager == null) {
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            List<AutoConfiguredMonoBehaviour> autoConfiguredMonoBehaviours = GetComponents<AutoConfiguredMonoBehaviour>().ToList();
            foreach (AutoConfiguredMonoBehaviour autoConfiguredMonoBehaviour in autoConfiguredMonoBehaviours) {
               autoConfiguredMonoBehaviour.AutoConfigure(systemGameManager);
            }
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToClientInteractableEvents();
        }

        public override void OnStopClient() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStopClient()");

            base.OnStopClient();
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientInteractableEvents();
            //systemGameManager.NetworkManagerClient.ProcessStopClient(unitController);
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStartServer()");

            base.OnStartServer();

            Configure();
            if (systemGameManager == null) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStartServer(): systemGameManager is null");
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToServerInteractableEvents();
            SubscribeToSystemEvents();
        }

        public override void OnStopServer() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStopServer() {GetInstanceID()}");

            base.OnStopServer();

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            // enabling this here is needed because if a level unload occurs on the server as a result of the last player leaving the scene,
            // the object will not have Despawn() called on it.  Despawn() is only triggered by client level unloads, and stop server calls
            // now that all interactables can respond to level unloads on the server, this should no longer be needed
            //UnsubscribeFromServerInteractableEvents();
            //UnsubscribeFromSystemEvents();
        }

        private void SubscribeToSystemEvents() {
            systemGameManager.NetworkManagerServer.OnBeforeStopServer += HandleBeforeStopServer;
        }

        private void UnsubscribeFromSystemEvents() {
            //Debug.Log($"FishNetInteractable.UnsubscribeFromSystemEvents() {GetInstanceID()}");

            systemGameManager.NetworkManagerServer.OnBeforeStopServer -= HandleBeforeStopServer;
        }

        private void HandleBeforeStopServer() {
            //Debug.Log($"FishNetInteractable.HandleBeforeStopServer() {GetInstanceID()}");

            // stopping the server results in the objects being destroyed without levelUnload being called, which is the usual way to initiate cleanup
            UnsubscribeFromSystemEvents();
            UnsubscribeFromServerInteractableEvents();
            interactable.ProcessLevelUnload();
        }

        public void SubscribeToServerInteractableEvents() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.SubscribeToServerInteractableEvents()");

            if (eventRegistrationComplete == true) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.SubscribeToServerInteractableEvents(): already registered");
                return;
            }

            if (interactable == null) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.SubscribeToServerInteractableEvents(): interactable is null");
                // something went wrong
                return;
            }

            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement += HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted += HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode += HandlePlayDialogNode;
            interactable.OnInteractableResetSettings += HandleInteractableResetSettingsServer;
            interactable.InteractableEventController.OnDropLoot += HandleDropLoot;
            interactable.InteractableEventController.OnRemoveDroppedItem += HandleRemoveDroppedItem;
            interactable.InteractableEventController.OnStopMovementSound += HandleStopMovementSound;
            interactable.InteractableEventController.OnPlayMovementSound += HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopCastSound += HandleStopCastSound;
            interactable.InteractableEventController.OnPlayCastSound += HandlePlayCastSound;
            interactable.InteractableEventController.OnStopEffectSound += HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayEffectSound += HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopVoiceSound += HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayVoiceSound += HandlePlayVoiceSound;
            interactable.InteractableEventController.OnMiniMapStatusUpdate += HandleMiniMapStatusUpdate;
            interactable.InteractableEventController.OnSellItemToPlayer += HandleSellItemToPlayer;

            eventRegistrationComplete = true;
        }

        public void HandleInteractableResetSettingsServer() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleInteractableResetSettingsServer(): {GetInstanceID()}");

            UnsubscribeFromServerInteractableEvents();
            UnsubscribeFromSystemEvents();
        }

        public void UnsubscribeFromServerInteractableEvents() {
            if (interactable == null) {
                return;
            }
            if (eventRegistrationComplete == false) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.UnsubscribeFromServerInteractableEvents(): not registered");
                return;
            }
            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement -= HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted -= HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode -= HandlePlayDialogNode;
            interactable.OnInteractableResetSettings -= HandleInteractableResetSettingsServer;
            interactable.InteractableEventController.OnDropLoot -= HandleDropLoot;
            interactable.InteractableEventController.OnRemoveDroppedItem -= HandleRemoveDroppedItem;
            interactable.InteractableEventController.OnStopMovementSound -= HandleStopMovementSound;
            interactable.InteractableEventController.OnPlayMovementSound -= HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopCastSound -= HandleStopCastSound;
            interactable.InteractableEventController.OnPlayCastSound -= HandlePlayCastSound;
            interactable.InteractableEventController.OnStopEffectSound -= HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayEffectSound -= HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopVoiceSound -= HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayVoiceSound -= HandlePlayVoiceSound;
            interactable.InteractableEventController.OnMiniMapStatusUpdate -= HandleMiniMapStatusUpdate;
            interactable.InteractableEventController.OnSellItemToPlayer -= HandleSellItemToPlayer;

            eventRegistrationComplete = false;
        }

        private void HandleSellItemToPlayer(VendorItem item, int componentIndex, int collectionIndex, int itemIndex) {
            HandleSellItemToPlayerClient(item.ItemName, item.Quantity, componentIndex, collectionIndex, itemIndex);
        }

        [ObserversRpc]
        private void HandleSellItemToPlayerClient(string resourceName, int remainingQuantity, int componentIndex, int collectionIndex, int itemIndex) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                vendorComponent.HandleSellItemToPlayer(resourceName, remainingQuantity, componentIndex, collectionIndex, itemIndex);
            }
        }

        public void AdvertiseSellItemToPlayerClient(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                List<VendorCollection> localVendorCollections = vendorComponent.GetLocalVendorCollections();
                if (localVendorCollections.Count > collectionIndex && localVendorCollections[collectionIndex].VendorItems.Count > itemIndex) {
                    VendorItem vendorItem = localVendorCollections[collectionIndex].VendorItems[itemIndex];
                    if (vendorItem.Item.ResourceName == resourceName) {
                        vendorComponent.ProcessQuantityNotification(vendorItem, remainingQuantity, componentIndex, collectionIndex, itemIndex);
                    }
                }
            }
        }

        private void HandleMiniMapStatusUpdate(InteractableOptionComponent component) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleMiniMapStatusUpdate({component.GetType().Name})");

            HandleMiniMapStatusUpdateClient(component.GetOptionIndex());
        }

        [ObserversRpc]
        public void HandleMiniMapStatusUpdateClient(int componentIndex) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleMiniMapStatusUpdateClient({componentIndex})");

            if (interactable.Interactables.ContainsKey(componentIndex) == false) {
                return;
            }
            interactable.HandleMiniMapStatusUpdate(interactable.Interactables[componentIndex]);
        }

        private void HandlePlayVoiceSound(AudioClip clip) {
            HandlePlayVoiceSoundClient(clip.name);
        }

        [ObserversRpc]
        public void HandlePlayVoiceSoundClient(string clipName) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayVoiceSound(clip);
            }
        }

        private void HandleStopVoiceSound() {
            HandleStopVoiceSoundClient();
        }

        [ObserversRpc]
        public void HandleStopVoiceSoundClient() {
            interactable.UnitComponentController.StopVoiceSound();
        }

        private void HandlePlayEffectSound(AudioClip clip, bool loop) {
            HandlePlayEffectSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayEffectSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayEffectSound(clip, loop);
            }
        }

        private void HandleStopEffectSound() {
            HandleStopEffectSoundClient();
        }

        [ObserversRpc]
        public void HandleStopEffectSoundClient() {
            interactable.UnitComponentController.StopEffectSound();
        }

        private void HandlePlayCastSound(AudioClip clip, bool loop) {
            HandlePlayCastSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayCastSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayCastSound(clip, loop);
            }
        }

        private void HandleStopCastSound() {
            HandleStopCastSoundClient();
        }

        [ObserversRpc]
        public void HandleStopCastSoundClient() {
            interactable.UnitComponentController.StopCastSound();
        }

        private void HandlePlayMovementSound(AudioClip clip, bool loop) {
            HandlePlayMovementSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayMovementSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayMovementSound(clip, loop);
            }
        }

        private void HandleStopMovementSound(bool stopLoops) {
            HandleStopMovementSoundClient(stopLoops);
        }

        [ObserversRpc]
        public void HandleStopMovementSoundClient(bool stopLoops) {
            interactable.UnitComponentController.StopMovementSound(stopLoops);
        }

        public void SubscribeToClientInteractableEvents() {
            if (interactable == null) {
                // something went wrong
                return;
            }

            //unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
        }

        public void UnsubscribeFromClientInteractableEvents() {
            if (interactable == null) {
                return;
            }
            //unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
        }

        [ObserversRpc]
        public void HandlePlayDialogNode(string dialogName, int dialogIndex) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandlePlayDialogNode({dialogName}, {dialogIndex})");

            interactable.DialogController.PlayDialogNode(dialogName, dialogIndex);
        }


        /*
        public void HandleAnimatedObjectChooseMovementServer(UnitController sourceUnitController, int optionIndex) {
            
            NetworkCharacterUnit targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            HandleAnimatedObjectChooseMovementClient(targetNetworkCharacterUnit, optionIndex);
        }

        [ObserversRpc]
        public void HandleAnimatedObjectChooseMovementClient(NetworkCharacterUnit sourceNetworkCharacterUnit, int optionIndex) {
            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is AnimatedObjectComponent) {
                    (currentInteractables[optionIndex] as AnimatedObjectComponent).ChooseMovement(sourceUnitController, optionIndex);
                }
            }
        }
        */

        public void HandleInteractionWithOptionStarted(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleInteractionWithOptionStarted({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)}, {componentIndex}, {choiceIndex})");

            FishNetUnitController targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<FishNetUnitController>();
            }
            HandleInteractionWithOptionStartedClient(targetNetworkCharacterUnit.Owner, targetNetworkCharacterUnit, componentIndex, choiceIndex);
        }

        /// <summary>
        /// this triggers an event that results in ClientInteract() on players on their own clients
        /// </summary>
        /// <param name="sourceNetworkCharacterUnit"></param>
        /// <param name="componentIndex"></param>
        /// <param name="choiceIndex"></param>
        [TargetRpc]
        public void HandleInteractionWithOptionStartedClient(NetworkConnection networkConnection, FishNetUnitController sourceNetworkCharacterUnit, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleInteractionWithOptionStartedClient({(sourceNetworkCharacterUnit == null ? "null" : sourceNetworkCharacterUnit.gameObject.name)}, {componentIndex}, {choiceIndex})");

            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit == null) {
                return;
            }
            sourceUnitController = sourceNetworkCharacterUnit.UnitController;

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetSwitchInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(componentIndex)) {
                //currentInteractables[componentIndex].ClientInteract(sourceUnitController, componentIndex, choiceIndex);
                sourceUnitController.UnitEventController.NotifyOnStartInteractWithOption(currentInteractables[componentIndex], componentIndex, choiceIndex);
            }
        }

        private void HandleDropLoot(Dictionary<int, List<int>> lootDropIdLookup) {
            foreach (KeyValuePair<int, List<int>> kvp in lootDropIdLookup) {
                int accountId = kvp.Key;
                List<int> lootDropIds = kvp.Value;
                Dictionary<int, List<int>> targetLootDropIdLookup = new Dictionary<int, List<int>>();
                targetLootDropIdLookup.Add(accountId, lootDropIds);
                if (authenticationService.LoggedInAccounts.ContainsKey(accountId) && base.NetworkManager.ServerManager.Clients.ContainsKey(authenticationService.LoggedInAccounts[accountId].clientId)) {
                    NetworkConnection networkConnection = base.NetworkManager.ServerManager.Clients[authenticationService.LoggedInAccounts[accountId].clientId];
                    HandleDropLootTarget(networkConnection, targetLootDropIdLookup);
                }
            }
        }

        [TargetRpc]
        public void HandleDropLootTarget(NetworkConnection networkConnection, Dictionary<int, List<int>> lootDropIdLookup) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleDropLootTarget()");
            if (interactable == null) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleDropLootTarget(): interactable is null");
                return;
            }
            interactable.InteractableEventController.NotifyOnDropLoot(lootDropIdLookup);
        }

        private void HandleRemoveDroppedItem(int lootDropId, int accountId) {
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) && base.NetworkManager.ServerManager.Clients.ContainsKey(authenticationService.LoggedInAccounts[accountId].clientId)) {
                NetworkConnection networkConnection = base.NetworkManager.ServerManager.Clients[authenticationService.LoggedInAccounts[accountId].clientId];
                HandleRemoveDroppedItemTarget(networkConnection, lootDropId, accountId);
            }
        }

        [TargetRpc]
        public void HandleRemoveDroppedItemTarget(NetworkConnection networkConnection, int lootDropId, int accountId) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleRemoveDroppedItemTarget({lootDropId}, {accountId})");
            if (interactable == null) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleRemoveDroppedItemTarget(): interactable is null");
                return;
            }
            interactable.InteractableEventController.NotifyOnRemoveDroppedItemClient(lootDropId, accountId);
        }

    }
}

