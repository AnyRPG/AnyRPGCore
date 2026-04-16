using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
            if (interactable == null) { 
                Debug.LogError($"{gameObject.name}.FishNetInteractable.Configure(): no interactable found on object");
            }
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
            interactable.ProcessStopNetworkClient();
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStartServer()");

            base.OnStartServer();

            Configure();
            if (systemGameManager == null) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.OnStartServer(): systemGameManager is null");
                return;
            }

            SubscribeToServerInteractableEvents();
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

            interactable.InteractableEventController.OnInteractionWithOptionStarted += HandleInteractionWithOptionStarted;
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
            interactable.InteractableEventController.OnLootableNodeSpawnObjectSetActive += HandleLootableNodeSpawnObjectSetActive;
            interactable.InteractableEventController.OnActivatableObjectSetActive += HandleActivatableObjectSetActive;
            interactable.InteractableEventController.OnSetDroppedItems += HandleSetDroppedItems;
            interactable.InteractableEventController.OnRemoveItemFromStorageContainerSlot += HandleRemoveItemFromStorageContainerSlot;
            interactable.InteractableEventController.OnAddItemToStorageContainerSlot += HandleAddItemToStorageContainerSlot;

            eventRegistrationComplete = true;
        }

        public void HandleInteractableResetSettingsServer() {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleInteractableResetSettingsServer(): {GetInstanceID()}");

            UnsubscribeFromServerInteractableEvents();
        }

        public void UnsubscribeFromServerInteractableEvents() {
            if (interactable == null) {
                return;
            }
            if (eventRegistrationComplete == false) {
               //Debug.Log($"{gameObject.name}.FishNetInteractable.UnsubscribeFromServerInteractableEvents(): not registered");
                return;
            }
            interactable.InteractableEventController.OnInteractionWithOptionStarted -= HandleInteractionWithOptionStarted;
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
            interactable.InteractableEventController.OnLootableNodeSpawnObjectSetActive -= HandleLootableNodeSpawnObjectSetActive;
            interactable.InteractableEventController.OnActivatableObjectSetActive -= HandleActivatableObjectSetActive;
            interactable.InteractableEventController.OnSetDroppedItems -= HandleSetDroppedItems;
            interactable.InteractableEventController.OnRemoveItemFromStorageContainerSlot -= HandleRemoveItemFromStorageContainerSlot;
            interactable.InteractableEventController.OnAddItemToStorageContainerSlot -= HandleAddItemToStorageContainerSlot;


            eventRegistrationComplete = false;
        }

        private void HandleAddItemToStorageContainerSlot(int slotIndex, long itemInstanceId) {
            HandleAddItemToStorageContainerSlotClient(slotIndex, itemInstanceId);
        }

        [ObserversRpc]
        private void HandleAddItemToStorageContainerSlotClient(int slotIndex, long   itemInstanceId) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            foreach (KeyValuePair<int, InteractableOptionComponent> kvp in currentInteractables) {
                if (kvp.Value is StorageContainerComponent) {
                    (kvp.Value as StorageContainerComponent).AddInventoryItem(itemInstanceId, slotIndex);
                    break;
                }
            }
        }

        private void HandleRemoveItemFromStorageContainerSlot(int slotIndex, long itemInstanceId) {
            HandleRemoveItemFromStorageContainerSlotClient(slotIndex, itemInstanceId);
        }

        [ObserversRpc]
        private void HandleRemoveItemFromStorageContainerSlotClient(int slotIndex, long itemInstanceId) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            foreach (KeyValuePair<int, InteractableOptionComponent> kvp in currentInteractables) {
                if (kvp.Value is StorageContainerComponent) {
                    (kvp.Value as StorageContainerComponent).RemoveInventoryItemFromSlot(slotIndex, itemInstanceId);
                    break;
                }
            }
        }

        private void HandleSetDroppedItems(List<InstantiatedItem> list) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleSetDroppedItems()");

            DroppedItemNetworkData droppedItemNetworkData = new DroppedItemNetworkData();
            foreach (InstantiatedItem instantiatedItem in list) {
                droppedItemNetworkData.itemInstanceIds.Add(instantiatedItem.InstanceId);
            }
            droppedItemNetworkData.BundleItems(systemItemManager);

            HandleSetDroppedItemsClient(droppedItemNetworkData);
        }

        [ObserversRpc]
        private void HandleSetDroppedItemsClient(DroppedItemNetworkData droppedItemNetworkData) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleSetDroppedItemsClient()");

            systemItemManager.LoadItemInstanceListSaveData(droppedItemNetworkData.ItemInstanceListSaveData);
            List<InstantiatedItem> instantiatedItems = new List<InstantiatedItem>();
            foreach (long itemInstanceId in droppedItemNetworkData.itemInstanceIds) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (instantiatedItem != null) {
                    instantiatedItems.Add(instantiatedItem);
                }
            }
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            foreach (KeyValuePair<int, InteractableOptionComponent> kvp in currentInteractables) {
                if (kvp.Value is DroppedItemComponent) {
                    (kvp.Value as DroppedItemComponent).SetDroppedItems(instantiatedItems);
                    break;
                }
            }
        }

        private void HandleActivatableObjectSetActive(bool active) {
            HandleActivatableObjectSetActiveClient(active);
        }

        [ObserversRpc]
        private void HandleActivatableObjectSetActiveClient(bool active) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandleActivatableObjectSetActiveClient({active})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            foreach (KeyValuePair<int, InteractableOptionComponent> kvp in currentInteractables) {
                if (kvp.Value is ActivatableObjectComponent) {
                    if (active == true) {
                        (kvp.Value as ActivatableObjectComponent).Spawn();
                    } else {
                        (kvp.Value as ActivatableObjectComponent).Despawn();
                    }
                    break;
                }
            }
        }

        private void HandleLootableNodeSpawnObjectSetActive(bool active) {
            HandleLootableNodeSpawnObjectSetActiveClient(active);
        }

        [ObserversRpc]
        private void HandleLootableNodeSpawnObjectSetActiveClient(bool active) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.Interactables;
            foreach (KeyValuePair<int, InteractableOptionComponent> kvp in currentInteractables) {
                if (kvp.Value is LootableNodeComponent) {
                    if (active == true) {
                        (kvp.Value as LootableNodeComponent).Spawn();
                    } else {
                        (kvp.Value as LootableNodeComponent).Despawn();
                    }
                    break;
                }
            }
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
            //Debug.Log($"{gameObject.name}.FishNetInteractable.HandlePlayVoiceSound({(clip == null ? "null" : clip.name)})");

            if (clip == null) {
                //Debug.LogWarning($"{gameObject.name}.FishNetInteractable.HandlePlayVoiceSound(): clip is null");
                return;
            }
            HandlePlayVoiceSoundClient(clip.name);
        }

        [ObserversRpc]
        public void HandlePlayVoiceSoundClient(string clipName) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.InteractableEventController.NotifyOnPlayVoiceSound(clip);
            }
        }

        private void HandleStopVoiceSound() {
            HandleStopVoiceSoundClient();
        }

        [ObserversRpc]
        public void HandleStopVoiceSoundClient() {
            interactable.InteractableEventController.NotifyOnStopVoiceSound();
        }

        private void HandlePlayEffectSound(AudioClip audioClip, bool loop) {
            if (audioClip == null) {
                return;
            }
            HandlePlayEffectSoundClient(audioClip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayEffectSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.InteractableEventController.NotifyOnPlayEffectSound(clip, loop);
            }
        }

        private void HandleStopEffectSound() {
            HandleStopEffectSoundClient();
        }

        [ObserversRpc]
        public void HandleStopEffectSoundClient() {
            interactable.InteractableEventController.NotifyOnStopEffectSound();
        }

        private void HandlePlayCastSound(AudioClip audioClip, bool loop) {
            if (audioClip == null) {
                return;
            }
            HandlePlayCastSoundClient(audioClip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayCastSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.InteractableEventController.NotifyOnPlayCastSound(clip, loop);
            }
        }

        private void HandleStopCastSound() {
            HandleStopCastSoundClient();
        }

        [ObserversRpc]
        public void HandleStopCastSoundClient() {
            interactable.InteractableEventController.NotifyOnStopCastSound();
        }

        private void HandlePlayMovementSound(AudioClip audioClip, bool loop) {
            if (audioClip == null) {
                return;
            }
            HandlePlayMovementSoundClient(audioClip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayMovementSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.InteractableEventController.NotifyOnPlayMovementSound(clip, loop);
            }
        }

        private void HandleStopMovementSound(bool stopLoops) {
            HandleStopMovementSoundClient(stopLoops);
        }

        [ObserversRpc]
        public void HandleStopMovementSoundClient(bool stopLoops) {
            interactable.InteractableEventController.NotifyOnStopMovementSound(stopLoops);
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

        public void HandleInteractionWithOptionStarted(UnitController sourceUnitController, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
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

        private void HandleDropLoot(Dictionary<int, LootDropIdList> lootDropIdLookup) {
            foreach (KeyValuePair<int, LootDropIdList> kvp in lootDropIdLookup) {
                int accountId = kvp.Key;
                LootDropIdList lootDropIds = kvp.Value;
                Dictionary<int, LootDropIdList> targetLootDropIdLookup = new Dictionary<int, LootDropIdList>();
                targetLootDropIdLookup.Add(accountId, lootDropIds);
                if (authenticationService.LoggedInAccounts.ContainsKey(accountId) && base.NetworkManager.ServerManager.Clients.ContainsKey(authenticationService.LoggedInAccounts[accountId].clientId)) {
                    NetworkConnection networkConnection = base.NetworkManager.ServerManager.Clients[authenticationService.LoggedInAccounts[accountId].clientId];
                    HandleDropLootTarget(networkConnection, targetLootDropIdLookup);
                }
            }
        }

        [TargetRpc]
        public void HandleDropLootTarget(NetworkConnection networkConnection, Dictionary<int, LootDropIdList> lootDropIdLookup) {
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

        public override void OnSpawnServer(NetworkConnection connection) {
            //Debug.Log($"{gameObject.name}.FishNetInteractable.OnSpawnServer()");

            base.OnSpawnServer(connection);
            InteractableSaveData interactableSaveData = interactable.GetInteractableSaveData();
            interactableSaveData.BundleItems(systemItemManager);
            HandleSpawnServerClient(connection, interactableSaveData);
        }

        [TargetRpc]
        private void HandleSpawnServerClient(NetworkConnection networkConnection, InteractableSaveData interactableSaveData) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnServerClient()");

            interactable.LoadInteractableSaveData(interactableSaveData);
        }

    }
}

