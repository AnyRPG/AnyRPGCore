using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootDrop : IDescribable {

        public virtual Sprite MyIcon => null;

        public virtual string MyDisplayName => string.Empty;

        public virtual ItemQuality MyItemQuality {
            get {
                return null;
            }
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
        private string logSummary = string.Empty;

        public override Sprite MyIcon {
            get {
                return icon;
            }
        }

        public override string MyDisplayName {
            get {
                return GetDescription();
            }
        }
        private Dictionary<LootableCharacter, CurrencyNode> currencyNodes = new Dictionary<LootableCharacter, CurrencyNode>();

        public Dictionary<LootableCharacter, CurrencyNode> CurrencyNodes { get => currencyNodes; set => currencyNodes = value; }

        public void AddCurrencyNode(LootableCharacter lootableCharacter, CurrencyNode currencyNode) {
            //Debug.Log("LootableDrop.AddCurrencyNode(" + lootableCharacter.name + ", " + currencyNode.currency.MyName + " " + currencyNode.MyAmount +")");

            currencyNodes.Add(lootableCharacter, currencyNode);

            List<CurrencyNode> usedCurrencyNodes  = new List<CurrencyNode>();
            foreach (CurrencyNode tmpCurrencyNode in currencyNodes.Values) {
                usedCurrencyNodes.Add(tmpCurrencyNode);
            }
            RecalculateValues(usedCurrencyNodes);
        }

        public override string GetDescription() {
            //Debug.Log("LootableDrop.GetDescription()");
            return GetSummary();
        }

        public override bool TakeLoot() {
            base.TakeLoot();
            foreach (LootableCharacter lootableCharacter in currencyNodes.Keys) {
                if (currencyNodes[lootableCharacter].currency != null) {
                    PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.AddCurrency(currencyNodes[lootableCharacter].currency, currencyNodes[lootableCharacter].MyAmount);
                    List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                    tmpCurrencyNode.Add(currencyNodes[lootableCharacter]);
                    CombatLogUI.MyInstance.WriteSystemMessage("Gained " + RecalculateValues(tmpCurrencyNode, false));
                    lootableCharacter.TakeCurrencyLoot();
                }
            }
            return true;
        }

        public string RecalculateValues(List<CurrencyNode> usedCurrencyNodes, bool setIcon = true) {
            //Debug.Log("LootableDrop.RecalculateValues()");
            List<string> returnStrings = new List<string>();
            Dictionary<Currency, CurrencyNode> squishedNodes = new Dictionary<Currency, CurrencyNode>();
            foreach (CurrencyNode currencyNode in usedCurrencyNodes) {
                if (squishedNodes.ContainsKey(currencyNode.currency)) {
                    CurrencyNode tmp = squishedNodes[currencyNode.currency];
                    tmp.MyAmount += currencyNode.MyAmount;
                    squishedNodes[currencyNode.currency] = tmp;
                } else {
                    CurrencyNode tmp = new CurrencyNode();
                    tmp.currency = currencyNode.currency;
                    tmp.MyAmount = currencyNode.MyAmount;
                    squishedNodes.Add(tmp.currency, tmp);
                }
            }
            if (squishedNodes.Count > 0) {
                //Debug.Log("LootableDrop.RecalculateValues(): squishedNodes.count: " + squishedNodes.Count);
                bool nonZeroFound = false;
                foreach (KeyValuePair<Currency, int> keyValuePair in CurrencyConverter.RedistributeCurrency(squishedNodes.ElementAt(0).Value.currency, squishedNodes.ElementAt(0).Value.MyAmount)) {
                    if (keyValuePair.Value > 0 && nonZeroFound == false) {
                        nonZeroFound = true;
                        if (setIcon) {
                            icon = keyValuePair.Key.MyIcon;
                        }
                    }
                    if (nonZeroFound == true) {
                        returnStrings.Add(keyValuePair.Value + " " + keyValuePair.Key.MyDisplayName);
                    }
                }
            }

            summary = string.Join("\n", returnStrings);
            //Debug.Log("LootableDrop.RecalculateValues(): " + summary);
            return string.Join(", ", returnStrings);
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
                    return MyItem.MyItemQuality;
                }
                return base.MyItemQuality;
            }
        }

        public override Sprite MyIcon {
            get {
                if (MyItem != null) {
                    return MyItem.MyIcon;
                }
                return base.MyIcon;
            }
        }

        public override string MyDisplayName {
            get {
                if (MyItem != null) {
                    return MyItem.MyDisplayName;
                }
                return base.MyDisplayName;
            }
        }

        public Item MyItem { get; set; }

        public LootTable MyLootTable { get; set; }

        public ItemLootDrop(Item item, LootTable lootTable) {
            MyLootTable = lootTable;
            MyItem = item;
        }

        public override bool TakeLoot() {
            base.TakeLoot();
            return InventoryManager.MyInstance.AddItem(MyItem);
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