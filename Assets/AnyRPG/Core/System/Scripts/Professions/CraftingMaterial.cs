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

        public Item Item { get => item; }
        public int Count { get => count; }

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);

            item = null;
            if (itemName != null) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a crafting material.  CHECK INSPECTOR");
                }
            }
        }

    }

}