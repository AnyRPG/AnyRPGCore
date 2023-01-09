using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CraftingMaterial : ConfiguredClass {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private int count = 1;

        private IDescribable describable = null;

        public Item Item { get => item; }
        public int Count { get => count; }


        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            Configure(systemGameManager);

            this.describable = describable;

            item = null;
            if (itemName != null) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a crafting material for " + describable.DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}