using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class Loot : ConfiguredClass, IPrerequisiteOwner {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private float dropChance = 100f;

        [SerializeField]
        private int minDrops = 1;

        [SerializeField]
        private int maxDrops = 1;

        [Header("Restrictions")]

        [Tooltip("If set to true, character class requirements on the item must match for it to drop.")]
        [SerializeField]
        protected bool matchItemRestrictions = true;

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        public string ItemName { get => itemName; set => itemName = value; }
        public Item Item { get => item; }
        public float DropChance { get => dropChance; set => dropChance = value; }
        public int MinDrops { get => minDrops; set => minDrops = value; }
        public int MaxDrops { get => maxDrops; set => maxDrops = value; }

        public bool PrerequisitesMet {
            get {
                //Debug.Log(itemName + ".MyPrerequisitesMet");

                // match standard prerequisites
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    // realtime check for loot
                    prerequisiteCondition.UpdatePrerequisites();
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }

                // match character class
                if (matchItemRestrictions) {
                    if (!item.RequirementsAreMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                //Debug.Log(itemName + ".MyPrerequisitesMet: nothing false");
                return true;
            }
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            item = null;
            if (itemName != null) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a loot.  CHECK INSPECTOR");
                }
            }

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
                    }
                }
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }
        }

        public void HandlePrerequisiteUpdates() {
            // do nothing
        }
    }

}