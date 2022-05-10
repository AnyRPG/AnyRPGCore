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
    public class NewEquipmentSetWizard : ScriptableWizard {

        SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        // paths to default equipment images
        private const string headDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotHead.png";
        private const string shouldersDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotShoulder.png";
        private const string wristsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotWrist.png";
        private const string handsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotHand.png";
        private const string chestDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotChest.png";
        private const string waistDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotWaist.png";
        private const string legsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotLegs.png";
        private const string feetDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotFeet.png";

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Equipment Set")]

        public string equipmentSetName = string.Empty;

        [Tooltip("If true, the equipment will display the set they belong to in the tooltip, as well as receive the following set bonuses")]
        public bool useSetBonuses = false;

        [Tooltip("Each line represents the number of items required for that bonus.  For example, if 2 pieces and 5 pieces give a bonus, then the list should have 5 items, with 1, 3, and 4 blank.")]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        public List<string> setBonuses = new List<string>();

        [Header("Armor Class")]

        [Tooltip("If true, the character must have the following armor class skill to equip the item")]
        public bool requireArmorClass = false;

        [Tooltip("the armor class this item gets its armor value from")]
        [ResourceSelector(resourceType = typeof(ArmorClass))]
        public string armorClass = string.Empty;

        [Header("Optional Content")]

        [Tooltip("If true, a vendor collection with all the equipment will be created")]
        public bool createVendorCollection = true;

        [Tooltip("The name of the vendor collection that shows up the vendor dropdown list")]
        public string vendorCollectionName = string.Empty;


        [Tooltip("If true, a loot table that can drop the equipment when an enemy is defeated will be created")]
        public bool createLootTable = true;

        [Header("Item Quality")]

        [Tooltip("The name of the item quality to use")]
        [ResourceSelector(resourceType = typeof(ItemQuality))]
        public string itemQuality = string.Empty;

        [Tooltip("If true, a random item quality will be selected")]
        public bool randomItemQuality = false;

        [Header("Item Level")]

        [Tooltip("If true, this item level will scale to match the character level")]
        public bool dynamicLevel = false;

        [Tooltip("If true, and dynamic level is true, the item level will be frozen at the level it dropped at")]
        public bool freezeDropLevel = false;

        [Tooltip("If dynamic level is true and this value is greater than zero, the item scaling will be capped at this level")]
        public int levelCap = 0;

        [Tooltip("If dynamic level is not true, this value will be used for the static level")]
        public int itemLevel = 1;

        [Tooltip("The level the character must be to use this item")]
        public int useLevel = 1;

        [Header("Primary Stats")]

        [Tooltip("When equipped, the wearer will have these primary stats affected")]
        public List<ItemPrimaryStatNode> primaryStats = new List<ItemPrimaryStatNode>();

        [Header("Secondary Stats")]

        [Tooltip("If true, the secondary stats will be chosen randomly up to a limit defined by the item quality")]
        public bool randomSecondaryStats = false;

        [Tooltip("When equipped, the wearer will have these secondary stats affected")]
        public List<ItemSecondaryStatNode> secondaryStats = new List<ItemSecondaryStatNode>();

        [Header("Equipment Pieces")]

        public bool useHead = true;
        public string headName = string.Empty;
        public Sprite headIcon = null;
        public List<UMATextRecipe> headUMARecipes = new List<UMATextRecipe>();

        public bool useShoulders = true;
        public string shouldersName = string.Empty;
        public Sprite shouldersIcon = null;
        public List<UMATextRecipe> shouldersUMARecipes = new List<UMATextRecipe>();

        public bool useChest = true;
        public string chestName = string.Empty;
        public Sprite chestIcon = null;
        public List<UMATextRecipe> chestUMARecipes = new List<UMATextRecipe>();

        public bool useWrists = true;
        public string wristsName = string.Empty;
        public Sprite wristsIcon = null;
        public List<UMATextRecipe> wristsUMARecipes = new List<UMATextRecipe>();

        public bool useHands = true;
        public string handsName = string.Empty;
        public Sprite handsIcon = null;
        public List<UMATextRecipe> handsUMARecipes = new List<UMATextRecipe>();

        public bool useWaist = true;
        public string waistName = string.Empty;
        public Sprite waistIcon = null;
        public List<UMATextRecipe> waistUMARecipes = new List<UMATextRecipe>();

        public bool useLegs = true;
        public string legsName = string.Empty;
        public Sprite legsIcon = null;
        public List<UMATextRecipe> legsUMARecipes = new List<UMATextRecipe>();

        public bool useFeet = true;
        public string feetName = string.Empty;
        public Sprite feetIcon = null;
        public List<UMATextRecipe> feetUMARecipes = new List<UMATextRecipe>();


        // private properties
        string equipmentSetObjectPath = string.Empty;
        string previousVendorCollectionName = string.Empty;
        string previousEquipmentSetName = string.Empty;
        string previousArmorClassName = string.Empty;
        Dictionary<string, Dictionary<string, string>> equipmentDefaultNames = new Dictionary<string, Dictionary<string, string>>();

        [MenuItem("Tools/AnyRPG/Wizard/New Equipment Set Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewEquipmentSetWizard>("New Equipment Set Wizard", "Create");
        }

        void OnEnable() {

            systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);

            previousEquipmentSetName = equipmentSetName;
            previousVendorCollectionName = vendorCollectionName;

            // setup equipment name defaults
            equipmentDefaultNames.Add("", new Dictionary<string, string>());
            equipmentDefaultNames[""].Add("Head", "Hat");
            equipmentDefaultNames[""].Add("Shoulders", "Scarf");
            equipmentDefaultNames[""].Add("Chest", "Shirt");
            equipmentDefaultNames[""].Add("Wrists", "Bracelets");
            equipmentDefaultNames[""].Add("Hands", "Gloves");
            equipmentDefaultNames[""].Add("Waist", "Belt");
            equipmentDefaultNames[""].Add("Legs", "Pants");
            equipmentDefaultNames[""].Add("Feet", "Shoes");

            equipmentDefaultNames.Add("Cloth", new Dictionary<string, string>());
            equipmentDefaultNames["Cloth"].Add("Head", "Hood");
            equipmentDefaultNames["Cloth"].Add("Shoulders", "Mantle");
            equipmentDefaultNames["Cloth"].Add("Chest", "Robe");
            equipmentDefaultNames["Cloth"].Add("Wrists", "Bracelets");
            equipmentDefaultNames["Cloth"].Add("Hands", "Gloves");
            equipmentDefaultNames["Cloth"].Add("Waist", "Sash");
            equipmentDefaultNames["Cloth"].Add("Legs", "Pants");
            equipmentDefaultNames["Cloth"].Add("Feet", "Shoes");

            equipmentDefaultNames.Add("Leather", new Dictionary<string, string>());
            equipmentDefaultNames["Leather"].Add("Head", "Headband");
            equipmentDefaultNames["Leather"].Add("Shoulders", "Shoulders");
            equipmentDefaultNames["Leather"].Add("Chest", "Armor");
            equipmentDefaultNames["Leather"].Add("Wrists", "Bracers");
            equipmentDefaultNames["Leather"].Add("Hands", "Gloves");
            equipmentDefaultNames["Leather"].Add("Waist", "Belt");
            equipmentDefaultNames["Leather"].Add("Legs", "Pants");
            equipmentDefaultNames["Leather"].Add("Feet", "Boots");

            equipmentDefaultNames.Add("Plate", new Dictionary<string, string>());
            equipmentDefaultNames["Plate"].Add("Head", "Helm");
            equipmentDefaultNames["Plate"].Add("Shoulders", "Pauldrons");
            equipmentDefaultNames["Plate"].Add("Chest", "Breastplate");
            equipmentDefaultNames["Plate"].Add("Wrists", "Vambraces");
            equipmentDefaultNames["Plate"].Add("Hands", "Gauntlets");
            equipmentDefaultNames["Plate"].Add("Waist", "Girdle");
            equipmentDefaultNames["Plate"].Add("Legs", "Cuisses");
            equipmentDefaultNames["Plate"].Add("Feet", "Greaves");
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Checking parameters...", 0.1f);

            try {
                if (RunWizard() == false) {
                    PrintErrorMesage();
                    return;
                }
            } catch {
                PrintErrorMesage();
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Equipment Set Wizard", "New Equipment Set Wizard Complete! The equipment set can be found at " + equipmentSetObjectPath, "OK");

        }

        private void PrintErrorMesage() {
            // do nothing
            Debug.LogWarning("An error was detected while running wizard.  See console log for details");
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Equipment Set Wizard",
                "Error!  See console log for details",
                "OK");
        }

        private bool RunWizard() {

            // check that templates exist
            if (CheckRequiredTemplatesExist() == false) {
                return false;
            }

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            // Determine root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);

            // create armor folder
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/Item/Equipment/Armor");

            // create armor scriptable objects
            List<string> setItemNames = new List<string>();
            if (useHead == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Head Object...", 0.2f);
                //Texture2D headDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + headDefaultImagePath) as Texture2D;
                Sprite headDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + headDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, headName, (headIcon != null ? headIcon : headDefaultSprite), headUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Helm");
                setItemNames.Add(headName);
            }
            if (useShoulders == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Shoulders Object...", 0.25f);
                //Image shouldersDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + shouldersDefaultImagePath) as Image;
                Sprite shouldersDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + shouldersDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, shouldersName, (shouldersIcon != null ? shouldersIcon : shouldersDefaultSprite), shouldersUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Pauldrons");
                setItemNames.Add(shouldersName);
            }
            if (useChest == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Chest Object...", 0.3f);
                //Image chestDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + chestDefaultImagePath) as Image;
                Sprite chestDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + chestDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, chestName, (chestIcon != null ? chestIcon : chestDefaultSprite), chestUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Armor");
                setItemNames.Add(chestName);
            }
            if (useWrists == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Wrists Object...", 0.35f);
                //Image wristsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + wristsDefaultImagePath) as Image;
                Sprite wristsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + wristsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, wristsName, (wristsIcon != null ? wristsIcon : wristsDefaultSprite), wristsUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Bracers");
                setItemNames.Add(wristsName);
            }
            if (useHands == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Hands Object...", 0.4f);
                //Image handsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + handsDefaultImagePath) as Image;
                Sprite handsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + handsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, handsName, (handsIcon != null ? handsIcon : handsDefaultSprite), handsUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Gloves");
                setItemNames.Add(handsName);
            }
            if (useWaist == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Waist Object...", 0.45f);
                //Image waistDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + waistDefaultImagePath) as Image;
                Sprite waistDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + waistDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, waistName, (waistIcon != null ? waistIcon : waistDefaultSprite), waistUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Belt");
                setItemNames.Add(waistName);
            }
            if (useLegs == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Legs Object...", 0.5f);
                //Image legsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + legsDefaultImagePath) as Image;
                Sprite legsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + legsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, legsName, (legsIcon != null ? legsIcon : legsDefaultSprite), legsUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Pants");
                setItemNames.Add(legsName);
            }
            if (useFeet == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Feet Object...", 0.6f);
                //Image feetDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + feetDefaultImagePath) as Image;
                Sprite feetDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + feetDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, feetName, (feetIcon != null ? feetIcon : feetDefaultSprite), feetUMARecipes, (useSetBonuses == true ? equipmentSetName : ""), "Boots");
                setItemNames.Add(feetName);
            }

            // create equipment set folder
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/EquipmentSet");

            // create equipment set
            EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Equipment Set...", 0.7f);
            CreateEquipmentSetScriptableObject(fileSystemGameName, setItemNames, equipmentSetName, setBonuses);

            if (createVendorCollection == true) {
                // create vendor collection folder
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Vendor Collection...", 0.8f);
                WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/VendorCollection");

                // create vendor collection
                CreateVendorCollection(fileSystemGameName, setItemNames, vendorCollectionName);
            }

            if (createLootTable == true) {
                // create loot table folder
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Loot Table...", 0.9f);
                WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/LootTable");

                // create loot table
                CreateLootTable(fileSystemGameName, setItemNames, equipmentSetName);

            }

            AssetDatabase.Refresh();

            return true;
        }

        private bool CheckRequiredTemplatesExist() {

            
            // Check for presence of default ui images
            if (WizardUtilities.CheckFileExists(headDefaultImagePath, "Head Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(shouldersDefaultImagePath, "Shoulders Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(wristsDefaultImagePath, "Wrists Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(handsDefaultImagePath, "Hands Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(chestDefaultImagePath, "Chest Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(waistDefaultImagePath, "Waist Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(legsDefaultImagePath, "Legs Default Image") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(feetDefaultImagePath, "Feet Default Image") == false) {
                return false;
            }


            return true;
        }

        

        private void CreateEquipmentScriptableObject(string fileSystemGameName, string equipmentName, Sprite icon, List<UMATextRecipe> uMATextRecipes, string equipmentSetname, string equipmentSlotType) {

            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(equipmentName);

            // create item
            Armor equipmentItem = ScriptableObject.CreateInstance("Armor") as Armor;
            equipmentItem.ResourceName = equipmentName;
            equipmentItem.EquipmentSetName = equipmentSetname;
            equipmentItem.EquipmentSlotTypeName = equipmentSlotType;
            equipmentItem.Icon = icon;

            // armor
            equipmentItem.ArmorClassName = armorClass;
            equipmentItem.RequireArmorClass = requireArmorClass;
            equipmentItem.UseArmorModifier = true;

            // uma properties
            if (uMATextRecipes.Count > 0) {
                equipmentItem.UMARecipeProfileProperties.UMARecipes = uMATextRecipes;
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

            // item quality properties
            equipmentItem.ItemQualityName = itemQuality;
            equipmentItem.RandomItemQuality = randomItemQuality;

            // item level properties
            equipmentItem.DynamicLevel = dynamicLevel;
            equipmentItem.FreezeDropLevel = freezeDropLevel;
            equipmentItem.LevelCap = levelCap;
            equipmentItem.ItemLevel = itemLevel;
            equipmentItem.UseLevel = useLevel;

            // stats
            equipmentItem.PrimaryStats = primaryStats;
            equipmentItem.RandomSecondaryStats = randomSecondaryStats;
            equipmentItem.SecondaryStats = secondaryStats;

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/Item/Equipment/Armor/" + itemFileSystemName + "Item.asset";
            AssetDatabase.CreateAsset(equipmentItem, scriptableObjectPath);
        }

        private void CreateEquipmentSetScriptableObject(string fileSystemGameName, List<string> equipmentNames, string equipmentSetname, List<string> setBonuses) {

            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(equipmentSetname);

            // create equipment set
            EquipmentSet equipmentSet = ScriptableObject.CreateInstance("EquipmentSet") as EquipmentSet;
            equipmentSet.ResourceName = equipmentSetname;
            equipmentSet.EquipmentNames = equipmentNames;
            equipmentSet.TraitNames = setBonuses;

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/EquipmentSet/" + itemFileSystemName + "EquipmentSet.asset";
            
            equipmentSetObjectPath = scriptableObjectPath;
            AssetDatabase.CreateAsset(equipmentSet, scriptableObjectPath);
        }

        private void CreateVendorCollection(string fileSystemGameName, List<string> equipmentNames, string vendorCollectionName) {

            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(vendorCollectionName);

            // create vendor collection
            VendorCollection vendorCollection = ScriptableObject.CreateInstance("VendorCollection") as VendorCollection;
            vendorCollection.ResourceName = vendorCollectionName;
            foreach (string equipmentName in equipmentNames) {
                VendorItem vendorItem = new VendorItem();
                vendorItem.ItemName = equipmentName;
                vendorItem.Unlimited = true;
                vendorCollection.VendorItems.Add(vendorItem);
            }

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/VendorCollection/" + itemFileSystemName + "VendorCollection.asset";
            AssetDatabase.CreateAsset(vendorCollection, scriptableObjectPath);
        }

        private void CreateLootTable(string fileSystemGameName, List<string> equipmentNames, string lootTableName) {

            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(lootTableName);

            // create loot table
            LootTable lootTable = ScriptableObject.CreateInstance("LootTable") as LootTable;
            lootTable.ResourceName = lootTableName;

            // create loot group
            LootGroup lootGroup = new LootGroup();
            lootGroup.GuaranteedDrop = false;
            lootGroup.GroupChance = 100f;
            lootGroup.DropLimit = 1;
            lootTable.LootGroups.Add(lootGroup);

            foreach (string equipmentName in equipmentNames) {
                Loot loot = new Loot();
                loot.ItemName = equipmentName;
                loot.DropChance = 20f;
                loot.MinDrops = 1;
                loot.MaxDrops = 1;
                lootGroup.Loot.Add(loot);
            }

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/LootTable/" + itemFileSystemName + "Loot.asset";
            AssetDatabase.CreateAsset(lootTable, scriptableObjectPath);
        }

        void OnWizardUpdate() {
            helpString = "Creates an equipment set and equipment scriptable objects";
            
            SetEquipmentNames();
            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        private void SetEquipmentNames() {
            string usedKey = string.Empty;
            if (armorClass == "Cloth"
                || armorClass == "Leather"
                || armorClass == "Plate") {
                usedKey = armorClass;
            }

            if (createVendorCollection == true) {
                if (vendorCollectionName == string.Empty) {
                    vendorCollectionName = equipmentSetName + " Equipment";
                } else if (vendorCollectionName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    vendorCollectionName = equipmentSetName + vendorCollectionName.Substring(previousEquipmentSetName.Length);
                }
                /*
                if (previousArmorClassName != "Eq &&
                    headName.Substring(headName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) == equipmentDefaultNames[previousArmorClassName]["Head"]) {
                    headName = headName.Substring(0, headName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) + equipmentDefaultNames[usedKey]["Head"];
                }
                */
            } else {
                vendorCollectionName = string.Empty;
            }

            if (useHead == true) {
                if (headName == string.Empty) {
                    headName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Head"];
                } else if (headName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    headName = equipmentSetName + headName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    headName.Substring(headName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) == equipmentDefaultNames[previousArmorClassName]["Head"]) {
                    headName = headName.Substring(0, headName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) + equipmentDefaultNames[usedKey]["Head"];
                }
            } else {
                headName = string.Empty;
            }

            if (useShoulders == true) {
                if (shouldersName == string.Empty) {
                    shouldersName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Shoulders"];
                } else if (shouldersName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    shouldersName = equipmentSetName + shouldersName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    shouldersName.Substring(shouldersName.Length - equipmentDefaultNames[previousArmorClassName]["Shoulders"].Length) == equipmentDefaultNames[previousArmorClassName]["Shoulders"]) {
                    shouldersName = shouldersName.Substring(0, shouldersName.Length - equipmentDefaultNames[previousArmorClassName]["Shoulders"].Length) + equipmentDefaultNames[usedKey]["Shoulders"];
                }
            } else {
                shouldersName = string.Empty;
            }

            if (useChest == true) {
                if (chestName == string.Empty) {
                    chestName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Chest"];
                } else if (chestName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    chestName = equipmentSetName + chestName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    chestName.Substring(chestName.Length - equipmentDefaultNames[previousArmorClassName]["Chest"].Length) == equipmentDefaultNames[previousArmorClassName]["Chest"]) {
                    chestName = chestName.Substring(0, chestName.Length - equipmentDefaultNames[previousArmorClassName]["Chest"].Length) + equipmentDefaultNames[usedKey]["Chest"];
                }
            } else {
                chestName = string.Empty;
            }

            if (useWrists == true) {
                if (wristsName == string.Empty) {
                    wristsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Wrists"];
                } else if (wristsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    wristsName = equipmentSetName + wristsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    wristsName.Substring(wristsName.Length - equipmentDefaultNames[previousArmorClassName]["Wrists"].Length) == equipmentDefaultNames[previousArmorClassName]["Wrists"]) {
                    wristsName = wristsName.Substring(0, wristsName.Length - equipmentDefaultNames[previousArmorClassName]["Wrists"].Length) + equipmentDefaultNames[usedKey]["Wrists"];
                }
            } else {
                wristsName = string.Empty;
            }

            if (useHands == true) {
                if (handsName == string.Empty) {
                    handsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Hands"];
                } else if (handsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    handsName = equipmentSetName + handsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    handsName.Substring(handsName.Length - equipmentDefaultNames[previousArmorClassName]["Hands"].Length) == equipmentDefaultNames[previousArmorClassName]["Hands"]) {
                    handsName = handsName.Substring(0, handsName.Length - equipmentDefaultNames[previousArmorClassName]["Hands"].Length) + equipmentDefaultNames[usedKey]["Hands"];
                }
            } else {
                handsName = string.Empty;
            }

            if (useWaist == true) {
                if (waistName == string.Empty) {
                    waistName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Waist"];
                } else if (waistName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    waistName = equipmentSetName + waistName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    waistName.Substring(waistName.Length - equipmentDefaultNames[previousArmorClassName]["Waist"].Length) == equipmentDefaultNames[previousArmorClassName]["Waist"]) {
                    waistName = waistName.Substring(0, waistName.Length - equipmentDefaultNames[previousArmorClassName]["Waist"].Length) + equipmentDefaultNames[usedKey]["Waist"];
                }
            } else {
                waistName = string.Empty;
            }

            if (useLegs == true) {
                if (legsName == string.Empty) {
                    legsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Legs"];
                } else if (legsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    legsName = equipmentSetName + legsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    legsName.Substring(legsName.Length - equipmentDefaultNames[previousArmorClassName]["Legs"].Length) == equipmentDefaultNames[previousArmorClassName]["Legs"]) {
                    legsName = legsName.Substring(0, legsName.Length - equipmentDefaultNames[previousArmorClassName]["Legs"].Length) + equipmentDefaultNames[usedKey]["Legs"];
                }
            } else {
                legsName = string.Empty;
            }

            if (useFeet == true) {
                if (feetName == string.Empty) {
                    feetName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Feet"];
                } else if (feetName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    feetName = equipmentSetName + feetName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    feetName.Substring(feetName.Length - equipmentDefaultNames[previousArmorClassName]["Feet"].Length) == equipmentDefaultNames[previousArmorClassName]["Feet"]) {
                    feetName = feetName.Substring(0, feetName.Length - equipmentDefaultNames[previousArmorClassName]["Feet"].Length) + equipmentDefaultNames[usedKey]["Feet"];
                }
            } else {
                feetName = string.Empty;
            }

            previousVendorCollectionName = vendorCollectionName;
            previousEquipmentSetName = equipmentSetName;
            previousArmorClassName = usedKey;
        }

        string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            // check that weapon name is not empty
            if (equipmentSetName == "") {
                return "Equipment Set Name must not be empty";
            }

            if (useHead == true && headName == string.Empty) {
                return "Head Name must not be empty";
            }

            if (useShoulders == true && shouldersName == string.Empty) {
                return "Shoulders Name must not be empty";
            }

            if (useChest == true && chestName == string.Empty) {
                return "Chest Name must not be empty";
            }

            if (useWrists == true && wristsName == string.Empty) {
                return "Wrists Name must not be empty";
            }

            if (useHands == true && handsName == string.Empty) {
                return "Hands Name must not be empty";
            }

            if (useWaist == true && waistName == string.Empty) {
                return "Waist Name must not be empty";
            }

            if (useLegs == true && legsName == string.Empty) {
                return "Legs Name must not be empty";
            }

            if (useFeet == true && feetName == string.Empty) {
                return "Feet Name must not be empty";
            }

            if (createVendorCollection == true && vendorCollectionName == string.Empty) {
                return "Vendor Collection Name must not be empty";
            }

            return null;
        }

        /*
        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);

            EditorGUILayout.LabelField("Scene Options", EditorStyles.boldLabel);

            weaponName = EditorGUILayout.TextField("Scene Name", weaponName);
            copyExistingScene = EditorGUILayout.Toggle("Copy Existing Scene", copyExistingScene);

            if (copyExistingScene) {
                existingScene = EditorGUILayout.ObjectField("Existing Scene", existingScene, typeof(SceneAsset), false) as SceneAsset;
            }

            newSceneAmbientSounds = EditorGUILayout.ObjectField("First Scene Ambient Sounds", newSceneAmbientSounds, typeof(AudioClip), false) as AudioClip;
            newSceneMusic = EditorGUILayout.ObjectField("First Scene Music", newSceneMusic, typeof(AudioClip), false) as AudioClip;

            return true;
        }
        */
        
    }

   


}
