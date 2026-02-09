using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class InteractableEventController : ConfiguredClass {

        //public event System.Action<UnitController, int> OnAnimatedObjectChooseMovement = delegate { };
        //public event System.Action<UnitController, int> OnInteractionWithOptionStarted = delegate { };
        public event System.Action<string, int> OnPlayDialogNode = delegate { };
        public event System.Action<UnitController, InstantiatedItem> OnAddToBuyBackCollection = delegate { };
        public event System.Action<VendorItem, int, int, int> OnSellItemToPlayer = delegate { };
        public event System.Action<Dictionary<int, List<int>>> OnDropLoot = delegate { };
        public event System.Action<int, int> OnRemoveDroppedItem = delegate { };
        public event System.Action<bool> OnStopMovementSound = delegate { };
        public event System.Action OnStopVoiceSound = delegate { };
        public event System.Action OnStopEffectSound = delegate { };
        public event System.Action OnStopCastSound = delegate { };
        public event System.Action<AudioClip, bool> OnPlayMovementSound = delegate { };
        public event System.Action<AudioClip> OnPlayVoiceSound = delegate { };
        public event System.Action<AudioClip, bool> OnPlayEffectSound = delegate { };
        public event System.Action<AudioClip, bool> OnPlayCastSound = delegate { };
        public event System.Action<InteractableOptionComponent> OnMiniMapStatusUpdate = delegate { };

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

        public void NotifyOnDropLoot(Dictionary<int, List<int>> lootDropIdLookup) {
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