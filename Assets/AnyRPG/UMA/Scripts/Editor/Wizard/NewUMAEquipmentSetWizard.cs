using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMA;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace AnyRPG {
    public class NewUMAEquipmentSetWizard : NewEquipmentSetWizardBase {

        [Header("Equipment Pieces")]

        [SerializeField] private bool useHead = true;
        [SerializeField] private string headName = string.Empty;
        [SerializeField] private Sprite headIcon = null;
        [SerializeField] private List<UMATextRecipe> headUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useShoulders = true;
        [SerializeField] private string shouldersName = string.Empty;
        [SerializeField] private Sprite shouldersIcon = null;
        [SerializeField] private List<UMATextRecipe> shouldersUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useChest = true;
        [SerializeField] private string chestName = string.Empty;
        [SerializeField] private Sprite chestIcon = null;
        [SerializeField] private List<UMATextRecipe> chestUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useWrists = true;
        [SerializeField] private string wristsName = string.Empty;
        [SerializeField] private Sprite wristsIcon = null;
        [SerializeField] private List<UMATextRecipe> wristsUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useHands = true;
        [SerializeField] private string handsName = string.Empty;
        [SerializeField] private Sprite handsIcon = null;
        [SerializeField] private List<UMATextRecipe> handsUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useWaist = true;
        [SerializeField] private string waistName = string.Empty;
        [SerializeField] private Sprite waistIcon = null;
        [SerializeField] private List<UMATextRecipe> waistUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useLegs = true;
        [SerializeField] private string legsName = string.Empty;
        [SerializeField] private Sprite legsIcon = null;
        [SerializeField] private List<UMATextRecipe> legsUMARecipes = new List<UMATextRecipe>();
        
        [SerializeField] private bool useFeet = true;
        [SerializeField] private string feetName = string.Empty;
        [SerializeField] private Sprite feetIcon = null;
        [SerializeField] private List<UMATextRecipe> feetUMARecipes = new List<UMATextRecipe>();

        private Dictionary<string, List<UMATextRecipe>> recipeDictionary = new Dictionary<string, List<UMATextRecipe>>();

        public override bool UseHead { get => useHead; set => useHead = value; }
        public override string HeadName { get => headName; set => headName = value; }
        public override Sprite HeadIcon { get => headIcon; set => headIcon = value; }
               
        public override bool UseShoulders { get => useShoulders; set => useShoulders = value; }
        public override string ShouldersName { get => shouldersName; set => shouldersName = value; }
        public override Sprite ShouldersIcon { get => shouldersIcon; set => shouldersIcon = value; }
               
        public override bool UseChest { get => useChest; set => useChest = value; }
        public override string ChestName { get => chestName; set => chestName = value; }
        public override Sprite ChestIcon { get => chestIcon; set => chestIcon = value; }
               
        public override bool UseWrists { get => useWrists; set => useWrists = value; }
        public override string WristsName { get => wristsName; set => wristsName = value; }
        public override Sprite WristsIcon { get => wristsIcon; set => wristsIcon = value; }
               
        public override bool UseHands { get => useHands; set => useHands = value; }
        public override string HandsName { get => handsName; set => handsName = value; }
        public override Sprite HandsIcon { get => handsIcon; set => handsIcon = value; }
               
        public override bool UseWaist { get => useWaist; set => useWaist = value; }
        public override string WaistName { get => waistName; set => waistName = value; }
        public override Sprite WaistIcon { get => waistIcon; set => waistIcon = value; }
               
        public override bool UseLegs { get => useLegs; set => useLegs = value; }
        public override string LegsName { get => legsName; set => legsName = value; }
        public override Sprite LegsIcon { get => legsIcon; set => legsIcon = value; }
               
        public override bool UseFeet { get => useFeet; set => useFeet = value; }
        public override string FeetName { get => feetName; set => feetName = value; }
        public override Sprite FeetIcon { get => feetIcon; set => feetIcon = value; }

        [MenuItem("Tools/AnyRPG/Wizard/UMA/New Equipment Set Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewEquipmentSetWizard>("New Equipment Set Wizard", "Create");
        }

        protected override void PopulateEquipmentSetWizardSettings() {
            base.PopulateEquipmentSetWizardSettings();

            recipeDictionary.Add("Helm", headUMARecipes);
            recipeDictionary.Add("Pauldrons", shouldersUMARecipes);
            recipeDictionary.Add("Armor", chestUMARecipes);
            recipeDictionary.Add("Bracers", wristsUMARecipes);
            recipeDictionary.Add("Gloves", handsUMARecipes);
            recipeDictionary.Add("Belt", waistUMARecipes);
            recipeDictionary.Add("Pants", legsUMARecipes);
            recipeDictionary.Add("Boots", feetUMARecipes);
        }

        protected override void CustomizeEquipmentItem(Armor equipmentItem, string equipmentSlotType) {
            base.CustomizeEquipmentItem(equipmentItem, equipmentSlotType);

            // uma properties
            if (recipeDictionary[equipmentSlotType].Count == 0) {
                return;
            }

            UMAEquipmentModel umaEquipmentModel = new UMAEquipmentModel();
            umaEquipmentModel.Properties.UMARecipes = recipeDictionary[equipmentSlotType];
            equipmentItem.InlineEquipmentModels.EquipmentModels.Add(umaEquipmentModel);
            /*
            // disabled for now due to null reference in UMA code when trying to access shared colors
            // make unique list of shared color names
            List<string> sharedColorNames = new List<string>();
            foreach (UMATextRecipe uMATextRecipe in uMATextRecipes) {

                foreach (OverlayColorData overlayColorData in uMATextRecipe.SharedColors) {
                    if (overlayColorData.IsASharedColor == true) {
                        if (sharedColorNames.Contains(overlayColorData.name) == false) {
                            sharedColorNames.Add(overlayColorData.name);
                        }
                    }
                }
            }

            // add shared color nodes based on unique list of names
            foreach (string sharedColorName in sharedColorNames) {
                SharedColorNode sharedColorNode = new SharedColorNode();
                sharedColorNode.SharedColorname = sharedColorName;
                equipmentItem.UMARecipeProfileProperties.SharedColors.Add(sharedColorNode);
            }
            */
        }

    }

}
