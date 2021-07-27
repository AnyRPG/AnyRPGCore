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

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

        // a reference to the actual craft ability
        private CraftAbility craftAbility;

        public Item MyOutput { get => output; set => output = value; }
        public List<CraftingMaterial> MyCraftingMaterials { get => craftingMaterials; set => craftingMaterials = value; }
        public int MyOutputCount { get => outputCount; set => outputCount = value; }
        public CraftAbility CraftAbility { get => craftAbility; set => craftAbility = value; }
        public bool AutoLearn { get => autoLearn; set => autoLearn = value; }
        public int RequiredLevel { get => requiredLevel; set => requiredLevel = value; }
        public List<AbilityAttachmentNode> HoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            craftAbility = null;
            if (craftAbilityName != null) {
                BaseAbility baseAbility = SystemAbilityManager.Instance.GetResource(craftAbilityName);
                if (baseAbility != null) {
                    craftAbility = baseAbility as CraftAbility;
                } else {
                    Debug.LogError("Recipe.SetupScriptableObjects(): Could not find ability : " + craftAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (holdableObjectList != null) {
                foreach (AbilityAttachmentNode holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects();
                    }
                }
            }

            output = null;
            if (itemOutputName != null && itemOutputName != string.Empty) {
                Item item = SystemItemManager.Instance.GetResource(itemOutputName);
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