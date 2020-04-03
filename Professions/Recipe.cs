using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Recipe", menuName = "AnyRPG/Recipes/Recipe")]
    public class Recipe : DescribableResource {

        [SerializeField]
        private List<CraftingMaterial> craftingMaterials = new List<CraftingMaterial>();

        [SerializeField]
        private string itemOutputName = string.Empty;

        //[SerializeField]
        private Item output;

        [SerializeField]
        private int outputCount = 0;

        [SerializeField]
        private string craftAbilityName = string.Empty;

        //[SerializeField]
        private CraftAbility craftAbility;

        public Item MyOutput { get => output; set => output = value; }
        public List<CraftingMaterial> MyCraftingMaterials { get => craftingMaterials; set => craftingMaterials = value; }
        public int MyOutputCount { get => outputCount; set => outputCount = value; }
        public CraftAbility MyCraftAbility { get => craftAbility; set => craftAbility = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            craftAbility = null;
            if (craftAbilityName != null) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(craftAbilityName);
                if (baseAbility != null) {
                    craftAbility = baseAbility as CraftAbility;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + craftAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            output = null;
            if (itemOutputName != null && itemOutputName != string.Empty) {
                Item item = SystemItemManager.MyInstance.GetResource(itemOutputName);
                if (item != null) {
                    output = item;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemOutputName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            if (craftingMaterials != null) {
                foreach (CraftingMaterial craftingMaterial in craftingMaterials) {
                    craftingMaterial.SetupScriptableObjects();
                }
            }
        }

    }

}