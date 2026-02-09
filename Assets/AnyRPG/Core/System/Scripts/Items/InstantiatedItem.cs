using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Superclass for all items
    /// </summary>
    public class InstantiatedItem : ConfiguredClass, IMoveable, IDescribable, IUseable {

        protected long instanceId;
        protected Item item;
        protected ItemQuality itemQuality;
        protected string displayName = string.Empty;
        protected int dropLevel;

        // A reference to the slot that this item is sitting on
        protected InventorySlot slot = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected MessageFeedManager messageFeedManager = null;

        public long InstanceId { get => instanceId; set => instanceId = value; }
        public Item Item { get => item; set => item = value; }
        public ItemQuality ItemQuality {
            get {
                if (itemQuality == null) {
                    return item.ItemQuality;
                }
                return itemQuality;
            }
            set => itemQuality = value;
        }
        public string DisplayName {
            get {
                if (displayName != string.Empty) {
                    return displayName;
                }
                return item.DisplayName;
            }
            set => displayName = value;
        }
        public int DropLevel {
            get => dropLevel;
            set {
                dropLevel = (int)Mathf.Clamp(Mathf.Min(value, (item.LevelCap > 0 ? item.LevelCap : value)), 1, Mathf.Infinity);
            }
        }
        public InventorySlot Slot { get => slot; set => slot = value; }
        public virtual Sprite Icon { get => item.Icon; }

        public string ResourceName { get => item.ResourceName; }
        public string Description { get => item.Description; }
        public virtual float CoolDown { get => 0f; }
        public virtual bool RequireOutOfCombat { get => false; }
        public virtual bool RequireStealth { get => false; }
        public bool AlwaysDisplayCount { get => true; }

        public InstantiatedItem(SystemGameManager systemGameManager, long instanceId, Item item, ItemQuality itemQuality) {
            this.instanceId = instanceId;
            this.item = item;
            //if (itemQuality == null) {
                //this.itemQuality = item.ItemQuality;
            //} else {
                this.itemQuality = itemQuality;
            //}
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
        }

        public virtual void InitializeNewItem(ItemQuality usedItemQuality) {
            //Debug.Log($"{ResourceName}.InstantiatedItem.InitializeNewItem({(usedItemQuality == null ? "null" : usedItemQuality.ResourceName)})");

            item.InitializeNewItem(this, usedItemQuality);
            PostInitialization();
        }

        public virtual void PostInitialization() {
            // nothing here in base class for now
        }

        public virtual ItemInstanceSaveData GetItemSaveData() {
            //Debug.Log($"{ResourceName}.InstantiatedItem.GetSlotSaveData()");

            ItemInstanceSaveData saveData = new ItemInstanceSaveData();
            saveData.ItemName = ResourceName;
            saveData.DisplayName = displayName;
            if (itemQuality != null) {
                saveData.ItemQuality = itemQuality.ResourceName;
            }
            saveData.DropLevel = DropLevel;
            saveData.RandomSecondaryStatIndexes = new List<int>();
            saveData.ItemInstanceId = instanceId;
            return saveData;

        }

        public virtual void LoadSaveData(ItemInstanceSaveData itemInstanceSaveData) {
            displayName = itemInstanceSaveData.DisplayName;
            dropLevel = itemInstanceSaveData.DropLevel;
        }

        public int GetItemLevel(int characterLevel) {

            // frozen drop level overrides all other calculations
            if (item.FreezeDropLevel == true) {
                return (int)Mathf.Clamp(DropLevel, 1, Mathf.Infinity);
            }

            int returnLevel = item.GetItemLevel(characterLevel);

            // item quality can override regular individual item scaling (example, heirlooms always scale)
            if (ItemQuality == null) {
                return returnLevel;
            } else {
                if (ItemQuality.DynamicItemLevel) {
                    return (int)Mathf.Clamp(characterLevel, 1, (item.LevelCap > 0 ? item.LevelCap : Mathf.Infinity));
                } else {
                    return returnLevel;
                }
            }
        }


        public virtual bool Use(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.InstantiatedItem.Use({sourceUnitController.gameObject.name})");

            if (!item.CharacterClassRequirementIsMet(sourceUnitController.BaseCharacter)) {
                sourceUnitController.WriteMessageFeedMessage("You are not the right character class to use " + DisplayName);
                return false;
            }
            //if (GetItemLevel(playerManager.UnitController.CharacterStats.Level) > playerManager.UnitController.CharacterStats.Level) {
            if (item.UseLevel > sourceUnitController.CharacterStats.Level) {
                sourceUnitController.WriteMessageFeedMessage("You are too low level to use " + DisplayName);
                return false;
            }

            return true;
        }

        public bool ActionButtonUse(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.InstantiatedItem.ActionButtonUse({sourceUnitController.gameObject.name})");

            List<InstantiatedItem> itemList = sourceUnitController.CharacterInventoryManager?.GetItems(ResourceName, 1);
            if (itemList == null || itemList.Count == 0) {
                return false;
            }
            InstantiatedItem newInstantiatedItem = itemList[0];
            if (newInstantiatedItem == null) {
                return false;
            }
            //return newInstantiatedItem.Use(sourceUnitController);

            if (systemGameManager.GameMode == GameMode.Local) {
                newInstantiatedItem.Use(sourceUnitController);
            } else {
                sourceUnitController.UnitEventController.NotifyOnRequestUseItem(newInstantiatedItem.Slot.GetCurrentInventorySlotIndex(sourceUnitController));
            }
            return true;
        }

        public virtual Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            return null;
        }

        public virtual bool IsUseableStale(UnitController sourceUnitController) {
            // items are never stale
            // they should stay on action buttons in case the player picks up more
            return false;
        }


        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log($"{ResourceName}.InstantiatedItem.UpdateActionButtonVisual({actionButton.gameObject.name})");

            int count = playerManager.UnitController.CharacterInventoryManager.GetUseableCount(this);

            // redundant since this is already done in ActionButton.UpdateVisual()
            //uIManager.UpdateStackSize(actionButton, count, true);

            if (count == 0) {
                actionButton.EnableFullCoolDownIcon();
            } else {

                // check for ability cooldown here and only disable if no cooldown exists
                if (!item.HadSpecialIcon(actionButton)) {
                    actionButton.DisableCoolDownIcon();
                } else {
                    ProcessUpdateActionButtonVisual(actionButton);
                }
            }
        }

        public virtual void ProcessUpdateActionButtonVisual(ActionButton actionButton) {
            // do nothing, override in subclasses
        }

        public virtual int GetChargeCount() {
            //Debug.Log(DisplayName + ".Item.UpdateChargeCount()");
            return playerManager.UnitController.CharacterInventoryManager.GetUseableCount(this);
        }

        public IUseable GetFactoryUseable() {
            //return systemDataFactory.GetResource<Item>(ResourceName);
            return systemItemManager.GetNewInstantiatedItem(ResourceName);
        }

        public void AssignToActionButton(ActionButton actionButton) {
            //Debug.Log("the useable is an item");
            if (playerManager.UnitController.CharacterInventoryManager.FromSlot != null) {
                // white, really?  this doesn't actually happen...
                playerManager.UnitController.CharacterInventoryManager.FromSlot.Icon.color = Color.white;
                playerManager.UnitController.CharacterInventoryManager.FromSlot = null;
            } else {
                //Debug.Log("ActionButton.SetUseable(): This must have come from another actionbar, not the inventory");
            }
            uIManager.SetItemBackground(item, actionButton.BackgroundImage, new Color32(0, 0, 0, 255), ItemQuality);
        }

        public void HandleRemoveFromActionButton(ActionButton actionButton) {
        }


        public void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton) {
            // do nothing
        }

        public void AssignToHandScript(Image backgroundImage) {
            //Debug.Log("the useable is an item");

            uIManager.SetItemBackground(item, backgroundImage, new Color32(0, 0, 0, 255), ItemQuality);
        }

        /// <summary>
        /// removes the item from the inventory system
        /// </summary>
        public void Remove() {
            //Debug.Log("Item " + GetInstanceID().ToString() + " is about to ask the slot to remove itself");
            if (Slot != null) {
                //Debug.Log("The item's myslot is not null");
                Slot.RemoveItem(this);
                Slot = null;
            } else {
                //Debug.Log("The item's myslot is null!!!");
            }
        }

        public void RemoveFrom(InventorySlot inventorySlot) {
            if (inventorySlot != null) {
                inventorySlot.RemoveItem(this);
            }
        }

        public virtual string GetSummary() {
            //Debug.Log($"{item.ResourceName}.InstantiatedItem.GetSummary()");

            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(ItemQuality), DisplayName, GetDescription());
        }



        public virtual string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedItem.GetDescription()");

            return item.GetItemDescription(ItemQuality, GetItemLevel(playerManager.UnitController.CharacterStats.Level));
        }


    }
 }