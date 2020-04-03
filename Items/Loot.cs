using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class Loot {

        [SerializeField]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private float dropChance = 0f;

        [SerializeField]
        private int minDrops = 1;

        [SerializeField]
        private int maxDrops = 1;

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        public Item MyItem { get => item; }
        public float MyDropChance { get => dropChance; }
        public int MyMinDrops { get => minDrops; set => minDrops = value; }
        public int MyMaxDrops { get => maxDrops; set => maxDrops = value; }

        public bool MyPrerequisitesMet {
            get {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }

        public void SetupScriptableObjects() {
            item = null;
            if (itemName != null) {
                Item tmpItem = SystemItemManager.MyInstance.GetResource(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + itemName + " while inititalizing a loot.  CHECK INSPECTOR");
                }
            }

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects();
                    }
                }
            }
        }

    }

}