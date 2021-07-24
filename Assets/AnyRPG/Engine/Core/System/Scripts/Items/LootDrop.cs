using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootDrop : IDescribable {

        public virtual Sprite Icon => null;

        public virtual string DisplayName => string.Empty;

        public virtual ItemQuality MyItemQuality {
            get {
                return null;
            }
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
        private Dictionary<LootableCharacterComponent, CurrencyNode> currencyNodes = new Dictionary<LootableCharacterComponent, CurrencyNode>();

        public Dictionary<LootableCharacterComponent, CurrencyNode> CurrencyNodes { get => currencyNodes; set => currencyNodes = value; }

        public void AddCurrencyNode(LootableCharacterComponent lootableCharacter, CurrencyNode currencyNode) {
            //Debug.Log("LootableDrop.AddCurrencyNode(" + lootableCharacter.name + ", " + currencyNode.currency.MyName + " " + currencyNode.MyAmount +")");

            currencyNodes.Add(lootableCharacter, currencyNode);

            List<CurrencyNode> usedCurrencyNodes  = new List<CurrencyNode>();
            foreach (CurrencyNode tmpCurrencyNode in currencyNodes.Values) {
                usedCurrencyNodes.Add(tmpCurrencyNode);
            }
            KeyValuePair<Sprite, string> keyValuePair = CurrencyConverter.RecalculateValues(usedCurrencyNodes, true);
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
                    PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.AddCurrency(currencyNodes[lootableCharacter].currency, currencyNodes[lootableCharacter].MyAmount);
                    List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                    tmpCurrencyNode.Add(currencyNodes[lootableCharacter]);
                    CombatLogUI.MyInstance.WriteSystemMessage("Gained " + CurrencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", "));
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

        public override ItemQuality MyItemQuality {
            get {
                if (MyItem != null) {
                    return MyItem.ItemQuality;
                }
                return base.MyItemQuality;
            }
        }

        public override Sprite Icon {
            get {
                if (MyItem != null) {
                    return MyItem.Icon;
                }
                return base.Icon;
            }
        }

        public override string DisplayName {
            get {
                if (MyItem != null) {
                    return MyItem.DisplayName;
                }
                return base.DisplayName;
            }
        }

        public Item MyItem { get; set; }

        public LootTable MyLootTable { get; set; }

        public ItemLootDrop(Item item, LootTable lootTable) {
            MyLootTable = lootTable;
            MyItem = item;
        }

        public override void SetBackgroundImage(Image backgroundImage) {
            base.SetBackgroundImage(backgroundImage);
            UIManager.MyInstance.SetItemBackground(MyItem, backgroundImage, new Color32(0, 0, 0, 255));
        }

        public override bool TakeLoot() {
            base.TakeLoot();
            return InventoryManager.Instance.AddItem(MyItem);
        }

        public override void Remove() {
            base.Remove();
            MyLootTable.MyDroppedItems.Remove(this);
        }

        public override void AfterLoot() {
            base.AfterLoot();
            if ((MyItem as CurrencyItem) is CurrencyItem) {
                //Debug.Log("LootUI.TakeAllLoot(): item is currency: " + MyLoot.MyName);
                (MyItem as CurrencyItem).Use();
            } else if ((MyItem as QuestStartItem) is QuestStartItem) {
                //Debug.Log("LootUI.TakeAllLoot(): item is questStartItem: " + MyLoot.MyName);
                (MyItem as QuestStartItem).Use();
            } else {
                //Debug.Log("LootUI.TakeAllLoot(): item is normal item");
            }
        }

        public override string GetDescription() {
            if (MyItem != null) {
                return MyItem.GetDescription();
            }
            return base.GetDescription();
        }

        public override string GetSummary() {
            if (MyItem != null) {
                return MyItem.GetSummary();
            }
            return base.GetSummary();
        }
    }

}