using UnityEngine;

namespace AnyRPG {
    public class Interactable : InteractableBase {

        [Header("Locking")]

        [Tooltip("If true, the interactable will be locked and require a key to interact with it.  If false, the interactable will not be locked and can be interacted with without a key.")]
        [SerializeField]
        private bool locked = false;

        [Tooltip("The name of the key item required to interact with the interactable if it is locked.  This should be the resource name of the item, not the display name. If this value is blank, the interactable will not be locked.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string keyName = string.Empty;

        [Tooltip("If true, the key will be removed from the player's inventory when they interact with the interactable.  If false, the player will still have the key after interacting with the interactable.")]
        [SerializeField]
        private bool removeKeyOnInteract = false;

        [Tooltip("If true, the interactable will be unlocked when the player interacts with it if they have the key.  If false, the player will still be able to interact with it, but it will remain locked for others.")]
        [SerializeField]
        private bool unlockOnInteract = false;

        public override bool CanInteract(UnitController sourceUnitController) {
            if (locked == true && keyName != string.Empty && sourceUnitController.CharacterInventoryManager.HasItem(keyName) == false) {
                return false;
            }
            return base.CanInteract(sourceUnitController);
        }

        public override bool PerformPreInteractionCheck(UnitController sourceUnitController, bool passedRangeCheck) {
            if (locked == true && keyName != string.Empty) {
                if (passedRangeCheck == false) {
                    return false;
                }
                if (sourceUnitController.CharacterInventoryManager.HasItem(keyName) == false) {
                    sourceUnitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"You do not have the correct key to interact with {DisplayName}");
                    return false;
                }
                if (removeKeyOnInteract == true) {
                    sourceUnitController.CharacterInventoryManager.RemoveItem(keyName);
                }
                if (unlockOnInteract == true) {
                    Unlock();
                }
            }
            return base.PerformPreInteractionCheck(sourceUnitController, passedRangeCheck);
        }

        public override void LoadInteractableSaveData(InteractableSaveData interactableSaveData) {
            base.LoadInteractableSaveData(interactableSaveData);
            locked = interactableSaveData.Locked;
        }

        public override InteractableSaveData GetInteractableSaveData() {
            InteractableSaveData interactableSaveData = base.GetInteractableSaveData();
            interactableSaveData.Locked = locked;
            return interactableSaveData;
        }

        public override void Unlock() {
            base.Unlock();
            locked = false;
            interactableEventController.NotifyOnUnlock();
        }

        public override string GetDescription() {
            if (locked == false) {
                return base.GetDescription();
            }
            return base.GetDescription() + $"\n<color=red>Locked</color>";
        }

    }

}