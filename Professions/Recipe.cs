using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Recipe", menuName = "AnyRPG/Recipes/Recipe")]
    public class Recipe : DescribableResource {

        [Header("Recipe")]

        [Tooltip("If true, this recipe is automatically learned at the appropriate level")]
        [SerializeField]
        private bool autoLearn = false;

        [Tooltip("The level that is required to learn this recipe")]
        [SerializeField]
        private int requiredLevel = 1;

        [Header("Crafting")]

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

        [Header("Prefabs")]

        [Tooltip("The names of items to spawn while casting this ability")]
        [SerializeField]
        private List<string> holdableObjectNames = new List<string>();

        //[SerializeField]
        private List<PrefabProfile> holdableObjects = new List<PrefabProfile>();

        // a reference to the actual craft ability
        private CraftAbility craftAbility;

        public Item MyOutput { get => output; set => output = value; }
        public List<CraftingMaterial> MyCraftingMaterials { get => craftingMaterials; set => craftingMaterials = value; }
        public int MyOutputCount { get => outputCount; set => outputCount = value; }
        public CraftAbility CraftAbility { get => craftAbility; set => craftAbility = value; }
        public List<PrefabProfile> HoldableObjects { get => holdableObjects; set => holdableObjects = value; }
        public bool AutoLearn { get => autoLearn; set => autoLearn = value; }
        public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            craftAbility = null;
            if (craftAbilityName != null) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(craftAbilityName);
                if (baseAbility != null) {
                    craftAbility = baseAbility as CraftAbility;
                } else {
                    Debug.LogError("Recipe.SetupScriptableObjects(): Could not find ability : " + craftAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            holdableObjects = new List<PrefabProfile>();
            if (holdableObjectNames != null) {
                foreach (string holdableObjectName in holdableObjectNames) {
                    PrefabProfile holdableObject = SystemPrefabProfileManager.MyInstance.GetResource(holdableObjectName);
                    if (holdableObject != null) {
                        holdableObjects.Add(holdableObject);
                    } else {
                        Debug.LogError("Recipe.SetupScriptableObjects(): Could not find holdableObject: " + holdableObjectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            output = null;
            if (itemOutputName != null && itemOutputName != string.Empty) {
                Item item = SystemItemManager.MyInstance.GetResource(itemOutputName);
                if (item != null) {
                    output = item;
                } else {
                    Debug.LogError("Recipe.SetupScriptableObjects(): Could not find item : " + itemOutputName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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