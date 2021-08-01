using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CraftingMaterial {

        [SerializeField]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private int count = 1;

        public Item MyItem { get => item; }
        public int MyCount { get => count; }

        public void SetupScriptableObjects() {

            item = null;
            if (itemName != null) {
                Item tmpItem = SystemDataFactory.Instance.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a crafting material.  CHECK INSPECTOR");
                }
            }
        }

    }

}