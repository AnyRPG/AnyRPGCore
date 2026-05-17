using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class InteractableEventController : ConfiguredClass {

        //public event System.Action<UnitController, int> OnAnimatedObjectChooseMovement = delegate { };
        //public event System.Action<UnitController, int> OnInteractionWithOptionStarted = delegate { };
        public event Action<string, int> OnPlayDialogNode = delegate { };
        public event Action<UnitController, InstantiatedItem> OnAddToBuyBackCollection = delegate { };
        public event Action<VendorItem, int, int, int> OnSellItemToPlayer = delegate { };
        public event Action<Dictionary<int, LootDropIdList>> OnDropLoot = delegate { };
        public event Action<int, int> OnRemoveDroppedItem = delegate { };
        public event Action<bool> OnStopMovementSound = delegate { };
        public event Action OnStopVoiceSound = delegate { };
        public event Action OnStopEffectSound = delegate { };
        public event Action OnStopCastSound = delegate { };
        public event Action<AudioClip, bool> OnPlayMovementSound = delegate { };
        public event Action<AudioClip> OnPlayVoiceSound = delegate { };
        public event Action<AudioClip, bool> OnPlayEffectSound = delegate { };
        public event Action<AudioClip, bool> OnPlayCastSound = delegate { };
        public event Action<InteractableOptionComponent> OnMiniMapStatusUpdate = delegate { };
        public event Action OnTargeted = delegate { };
        public event Action OnUnTargeted = delegate { };
        public event Action<Vector3> OnSetNameplatePosition = delegate { };
        public event Action OnEnableInteractableRange = delegate { };
        public event Action<UnitController, InteractableOptionComponent, int, int> OnInteractionWithOptionStarted = delegate { };
        public event Action<bool> OnLootableNodeSpawnObjectSetActive = delegate { };
        public event Action<bool> OnActivatableObjectSetActive = delegate { };
        public event Action<List<InstantiatedItem>> OnSetDroppedItems = delegate { };
        public event Action<int, long> OnRemoveItemFromStorageContainerSlot = delegate { };
        public event Action<int, long> OnAddItemToStorageContainerSlot = delegate { };

        // interactable this controller is attached to
        private Interactable interactable;

        public InteractableEventController() {
            //this.Interactable = interactable;
        }

        public void SetInteractable(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public InteractableEventController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        #region EventNotifications

        public void NotifyOnPlayDialogNode(Dialog dialog, int dialogIndex) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableEventController.NotifyOnPlayDialogNode({dialog.ResourceName}, {dialogIndex})");

            OnPlayDialogNode(dialog.ResourceName, dialogIndex);
        }

        public void NotifyOnAddToBuyBackCollection(UnitController sourceUnitController, InstantiatedItem newInstantiatedItem) {
            OnAddToBuyBackCollection(sourceUnitController, newInstantiatedItem);
        }

        public void NotifyOnSellItemToPlayer(VendorItem vendorItem, int componentIndex, int collectionIndex, int itemIndex) {
            OnSellItemToPlayer(vendorItem, componentIndex, collectionIndex, itemIndex);
        }

        public void NotifyOnDropLoot(Dictionary<int, LootDropIdList> lootDropIdLookup) {
            OnDropLoot(lootDropIdLookup);
        }

        public void NotifyOnRemoveDroppedItem(LootDrop lootDrop, int accountId) {
            OnRemoveDroppedItem(lootDrop.LootDropId, accountId);
        }

        public void NotifyOnRemoveDroppedItemClient(int lootDropId, int accountId) {
            OnRemoveDroppedItem(lootDropId, accountId);
        }

        public void NotifyOnStopMovementSound(bool stopLoopsOnly) {
            OnStopMovementSound(stopLoopsOnly);
        }

        public void NotifyOnStopVoiceSound() {
            OnStopVoiceSound();
        }

        public void NotifyOnStopEffectSound() {
            OnStopEffectSound();
        }

        public void NotifyOnStopCastSound() {
            OnStopCastSound();
        }

        public void NotifyOnPlayMovementSound(AudioClip audioClip, bool loop) {
            OnPlayMovementSound(audioClip, loop);
        }

        public void NotifyOnPlayVoiceSound(AudioClip audioClip) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableEventController.NotifyOnPlayVoiceSound({(audioClip == null ? "null" : audioClip.name)})");

            OnPlayVoiceSound(audioClip);
        }

        public void NotifyOnPlayEffectSound(AudioClip audioClip, bool loop) {
            OnPlayEffectSound(audioClip, loop);
        }

        public void NotifyOnPlayCastSound(AudioClip audioClip, bool loop) {
            OnPlayCastSound(audioClip, loop);
        }

        public void NotifyOnMiniMapStatusUpdate(InteractableOptionComponent interactableOptionComponent) {
            OnMiniMapStatusUpdate(interactableOptionComponent);
        }

        public void NotifyOnTargeted() {
            OnTargeted();
        }

        public void NotifyOnUnTargeted() {
            OnUnTargeted();
        }

        public void NotifyOnSetNameplatePosition(Vector3 overridePosition) {
            OnSetNameplatePosition(overridePosition);
        }

        public void NotifyOnEnableInteractableRange() {
            OnEnableInteractableRange();
        }

        public void NotifyOnInteractionWithOptionStarted(UnitController sourceUnitController, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.Interactable.NotifyOnInteractionWithOptionStarted({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex})");

            OnInteractionWithOptionStarted(sourceUnitController, interactableOptionComponent, componentIndex, choiceIndex);
        }

        public void NotifyOnLootableNodeSpawnObjectSetActive(bool active) {
            OnLootableNodeSpawnObjectSetActive(active);
        }

        public void NotifyOnActivatableObjectSetActive(bool active) {
            OnActivatableObjectSetActive(active);
        }

        public void NotifyOnSetDroppedItems(List<InstantiatedItem> itemsToDrop) {
            OnSetDroppedItems(itemsToDrop);
        }

        public void NotifyOnRemoveItemFromStorageContainerSlot(int slotIndex, InstantiatedItem instantiatedItem) {
            OnRemoveItemFromStorageContainerSlot(slotIndex, instantiatedItem.InstanceId);
        }

        public void NotifyOnAddItemToStorageContainerSlot(int slotIndex, InstantiatedItem instantiatedItem) {
            OnAddItemToStorageContainerSlot(slotIndex, instantiatedItem.InstanceId);
        }


        // temporarily disabled because this object is not created early enough in the process when its a unitcontroller
        // this if fixed now ^

        //public void NotifyOnAnimatedObjectChooseMovement(UnitController sourceUnitController, int optionIndex) {
        //    OnAnimatedObjectChooseMovement(sourceUnitController, optionIndex);
        //}

        //public void NotifyOnInteractionWithOptionStarted(UnitController sourceUnitController, int optionIndex) {
        //    OnInteractionWithOptionStarted(sourceUnitController, optionIndex);
        //}

        #endregion


    }

}