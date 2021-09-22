using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootDrop : ConfiguredClass, IDescribable {

        public virtual Sprite Icon => null;

        public virtual string DisplayName => string.Empty;

        public virtual ItemQuality ItemQuality {
            get {
                return null;
            }
        }

        public LootDrop(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public virtual void SetBackgroundImage(Image backgroundImage) {
            // do nothing, only used in child classes
        }


        public virtual bool TakeLoot() {
            // need a fake value by default
            return true;
        }

        public virtual void AfterLoot() {

        }

        public virtual void Remove() {
            //Debug.Log("LootDrop.Remove()");
            
        }

        public virtual string GetDescription() {
            return string.Empty;
        }

        public virtual string GetSummary() {
            return string.Empty;
        }
    }

    public class CurrencyLootDrop : LootDrop {

        private Sprite icon = null;

        private string summary = string.Empty;

        public override Sprite Icon {
            get {
                return icon;
            }
        }

        public override string DisplayName {
            get {
                return GetDescription();
            }
        }

        // game manager references
        private CurrencyConverter currencyConverter = null;
        private PlayerManager playerManager = null;
        private LogManager logManager = null;

        private Dictionary<LootableCharacterComponent, CurrencyNode> currencyNodes = new Dictionary<LootableCharacterComponent, CurrencyNode>();

        public Dictionary<LootableCharacterComponent, CurrencyNode> CurrencyNodes { get => currencyNodes; set => currencyNodes = value; }

        public CurrencyLootDrop(SystemGameManager systemGameManager) : base(systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
        }

        public void AddCurrencyNode(LootableCharacterComponent lootableCharacter, CurrencyNode currencyNode) {
            //Debug.Log("LootableDrop.AddCurrencyNode(" + lootableCharacter.name + ", " + currencyNode.currency.DisplayName + " " + currencyNode.MyAmount +")");

            currencyNodes.Add(lootableCharacter, currencyNode);

            List<CurrencyNode> usedCurrencyNodes  = new List<CurrencyNode>();
            foreach (CurrencyNode tmpCurrencyNode in currencyNodes.Values) {
                usedCurrencyNodes.Add(tmpCurrencyNode);
            }
            KeyValuePair<Sprite, string> keyValuePair = currencyConverter.RecalculateValues(usedCurrencyNodes, true);
            icon = keyValuePair.Key;
            summary = keyValuePair.Value;
        }

        public override string GetDescription() {
            //Debug.Log("LootableDrop.GetDescription()");
            return GetSummary();
        }

        public override bool TakeLoot() {
            base.TakeLoot();
            foreach (LootableCharacterComponent lootableCharacter in currencyNodes.Keys) {
                if (currencyNodes[lootableCharacter].currency != null) {
                    playerManager.MyCharacter.CharacterCurrencyManager.AddCurrency(currencyNodes[lootableCharacter].currency, currencyNodes[lootableCharacter].Amount);
                    List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                    tmpCurrencyNode.Add(currencyNodes[lootableCharacter]);
                    logManager.WriteSystemMessage("Gained " + currencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", "));
                    lootableCharacter.TakeCurrencyLoot();
                }
            }
            return true;
        }

        public override string GetSummary() {
            //Debug.Log("LootableDrop.GetSummary()");
            return summary;
        }

    }

    public class ItemLootDrop : LootDrop {

        // game manager references
        private UIManager uIManager = null;
        private InventoryManager inventoryManager = null;

        public override ItemQuality ItemQuality {
            get {
                if (Item != null) {
                    return Item.ItemQuality;
                }
                return base.ItemQuality;
            }
        }

        public override Sprite Icon {
            get {
                if (Item != null) {
                    return Item.Icon;
                }
                return base.Icon;
            }
        }

        public override string DisplayName {
            get {
                if (Item != null) {
                    return Item.DisplayName;
                }
                return base.DisplayName;
            }
        }

        public Item Item { get; set; }

        public LootTableState LootTableState { get; set; }

        public ItemLootDrop(Item item, LootTableState lootTableState, SystemGameManager systemGameManager) : base(systemGameManager) {
            LootTableState = lootTableState;
            Item = item;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            inventoryManager = systemGameManager.InventoryManager;
        }

        public override void SetBackgroundImage(Image backgroundImage) {
            base.SetBackgroundImage(backgroundImage);
            uIManager.SetItemBackground(Item, backgroundImage, new Color32(0, 0, 0, 255));
        }

        public override bool TakeLoot() {
            base.TakeLoot();
            return inventoryManager.AddItem(Item);
        }

        public override void Remove() {
            base.Remove();
            LootTableState.DroppedItems.Remove(this);
        }

        public override void AfterLoot() {
            base.AfterLoot();
            if ((Item as CurrencyItem) is CurrencyItem) {
                //Debug.Log("LootUI.TakeAllLoot(): item is currency: " + MyLoot.MyName);
                (Item as CurrencyItem).Use();
            } else if ((Item as QuestStartItem) is QuestStartItem) {
                //Debug.Log("LootUI.TakeAllLoot(): item is questStartItem: " + MyLoot.MyName);
                (Item as QuestStartItem).Use();
            } else {
                //Debug.Log("LootUI.TakeAllLoot(): item is normal item");
            }
        }

        public override string GetDescription() {
            if (Item != null) {
                return Item.GetDescription();
            }
            return base.GetDescription();
        }

        public override string GetSummary() {
            if (Item != null) {
                return Item.GetSummary();
            }
            return base.GetSummary();
        }
    }

}