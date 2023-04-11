using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace AnyRPG {
    public abstract class NewEquipmentSetWizardBase : ScriptableWizard {

        protected SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        protected string gameParentFolder = "/Games/";

        // paths to default equipment images
        protected const string headDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotHead.png";
        protected const string shouldersDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotShoulder.png";
        protected const string wristsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotWrist.png";
        protected const string handsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotHand.png";
        protected const string chestDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotChest.png";
        protected const string waistDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotWaist.png";
        protected const string legsDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotLegs.png";
        protected const string feetDefaultImagePath = "/AnyRPG/Core/System/Images/UI/Pixel/UISlotFeet.png";

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

        // private properties
        protected string equipmentSetObjectPath = string.Empty;
        protected string previousVendorCollectionName = string.Empty;
        protected string previousEquipmentSetName = string.Empty;
        protected string previousArmorClassName = string.Empty;
        protected Dictionary<string, Dictionary<string, string>> equipmentDefaultNames = new Dictionary<string, Dictionary<string, string>>();
        //protected EquipmentSetWizardSettings equipmentSetWizardSettings = null;

        public abstract bool UseHead { get; set; }
        public abstract string HeadName { get; set; }
        public abstract Sprite HeadIcon { get; set; }

        public abstract bool UseShoulders { get; set; }
        public abstract string ShouldersName { get; set; }
        public abstract Sprite ShouldersIcon { get; set; }

        public abstract bool UseChest { get; set; }
        public abstract string ChestName { get; set; }
        public abstract Sprite ChestIcon { get; set; }

        public abstract bool UseWrists { get; set; }
        public abstract string WristsName { get; set; }
        public abstract Sprite WristsIcon { get; set; }

        public abstract bool UseHands { get; set; }
        public abstract string HandsName { get; set; }
        public abstract Sprite HandsIcon { get; set; }

        public abstract bool UseWaist { get; set; }
        public abstract string WaistName { get; set; }
        public abstract Sprite WaistIcon { get; set; }

        public abstract bool UseLegs { get; set; }
        public abstract string LegsName { get; set; }
        public abstract Sprite LegsIcon { get; set; }

        public abstract bool UseFeet { get; set; }
        public abstract string FeetName { get; set; }
        public abstract Sprite FeetIcon { get; set; }

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
            
            //equipmentSetWizardSettings = new EquipmentSetWizardSettings();

            PopulateEquipmentSetWizardSettings();
        }

        protected virtual void PopulateEquipmentSetWizardSettings() {
            // nothing here for now
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
            if (UseHead == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Head Object...", 0.2f);
                //Texture2D headDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + headDefaultImagePath) as Texture2D;
                Sprite headDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + headDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, HeadName, (HeadIcon != null ? HeadIcon : headDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Helm");
                setItemNames.Add(HeadName);
            }
            if (UseShoulders == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Shoulders Object...", 0.25f);
                //Image shouldersDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + shouldersDefaultImagePath) as Image;
                Sprite shouldersDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + shouldersDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, ShouldersName, (ShouldersIcon != null ? ShouldersIcon : shouldersDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Pauldrons");
                setItemNames.Add(ShouldersName);
            }
            if (UseChest == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Chest Object...", 0.3f);
                //Image chestDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + chestDefaultImagePath) as Image;
                Sprite chestDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + chestDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, ChestName, (ChestIcon != null ? ChestIcon : chestDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Armor");
                setItemNames.Add(ChestName);
            }
            if (UseWrists == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Wrists Object...", 0.35f);
                //Image wristsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + wristsDefaultImagePath) as Image;
                Sprite wristsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + wristsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, WristsName, (WristsIcon != null ? WristsIcon : wristsDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Bracers");
                setItemNames.Add(WristsName);
            }
            if (UseHands == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Hands Object...", 0.4f);
                //Image handsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + handsDefaultImagePath) as Image;
                Sprite handsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + handsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, HandsName, (HandsIcon != null ? HandsIcon : handsDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Gloves");
                setItemNames.Add(HandsName);
            }
            if (UseWaist == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Waist Object...", 0.45f);
                //Image waistDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + waistDefaultImagePath) as Image;
                Sprite waistDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + waistDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, WaistName, (WaistIcon != null ? WaistIcon : waistDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Belt");
                setItemNames.Add(WaistName);
            }
            if (UseLegs == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Legs Object...", 0.5f);
                //Image legsDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + legsDefaultImagePath) as Image;
                Sprite legsDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + legsDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, LegsName, (LegsIcon != null ? LegsIcon : legsDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Pants");
                setItemNames.Add(LegsName);
            }
            if (UseFeet == true) {
                EditorUtility.DisplayProgressBar("New Equipment Set Wizard", "Creating Feet Object...", 0.6f);
                //Image feetDefaultImage = AssetDatabase.LoadMainAssetAtPath("Assets" + feetDefaultImagePath) as Image;
                Sprite feetDefaultSprite = AssetDatabase.LoadAllAssetsAtPath("Assets" + feetDefaultImagePath).OfType<Sprite>().First();
                CreateEquipmentScriptableObject(fileSystemGameName, FeetName, (FeetIcon != null ? FeetIcon : feetDefaultSprite), (useSetBonuses == true ? equipmentSetName : ""), "Boots");
                setItemNames.Add(FeetName);
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

        

        private void CreateEquipmentScriptableObject(string fileSystemGameName, string equipmentName, Sprite icon, string equipmentSetname, string equipmentSlotType) {

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

            CustomizeEquipmentItem(equipmentItem, equipmentSlotType);

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/Item/Equipment/Armor/" + itemFileSystemName + "Item.asset";
            AssetDatabase.CreateAsset(equipmentItem, scriptableObjectPath);
        }

        protected virtual void CustomizeEquipmentItem(Armor equipmentItem, string equipmentSlotType) {
            // nothing here for now
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

            if (UseHead == true) {
                if (HeadName == string.Empty) {
                    HeadName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Head"];
                } else if (HeadName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    HeadName = equipmentSetName + HeadName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    HeadName.Substring(HeadName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) == equipmentDefaultNames[previousArmorClassName]["Head"]) {
                    HeadName = HeadName.Substring(0, HeadName.Length - equipmentDefaultNames[previousArmorClassName]["Head"].Length) + equipmentDefaultNames[usedKey]["Head"];
                }
            } else {
                HeadName = string.Empty;
            }

            if (UseShoulders == true) {
                if (ShouldersName == string.Empty) {
                    ShouldersName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Shoulders"];
                } else if (ShouldersName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    ShouldersName = equipmentSetName + ShouldersName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    ShouldersName.Substring(ShouldersName.Length - equipmentDefaultNames[previousArmorClassName]["Shoulders"].Length) == equipmentDefaultNames[previousArmorClassName]["Shoulders"]) {
                    ShouldersName = ShouldersName.Substring(0, ShouldersName.Length - equipmentDefaultNames[previousArmorClassName]["Shoulders"].Length) + equipmentDefaultNames[usedKey]["Shoulders"];
                }
            } else {
                ShouldersName = string.Empty;
            }

            if (UseChest == true) {
                if (ChestName == string.Empty) {
                    ChestName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Chest"];
                } else if (ChestName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    ChestName = equipmentSetName + ChestName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    ChestName.Substring(ChestName.Length - equipmentDefaultNames[previousArmorClassName]["Chest"].Length) == equipmentDefaultNames[previousArmorClassName]["Chest"]) {
                    ChestName = ChestName.Substring(0, ChestName.Length - equipmentDefaultNames[previousArmorClassName]["Chest"].Length) + equipmentDefaultNames[usedKey]["Chest"];
                }
            } else {
                ChestName = string.Empty;
            }

            if (UseWrists == true) {
                if (WristsName == string.Empty) {
                    WristsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Wrists"];
                } else if (WristsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    WristsName = equipmentSetName + WristsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    WristsName.Substring(WristsName.Length - equipmentDefaultNames[previousArmorClassName]["Wrists"].Length) == equipmentDefaultNames[previousArmorClassName]["Wrists"]) {
                    WristsName = WristsName.Substring(0, WristsName.Length - equipmentDefaultNames[previousArmorClassName]["Wrists"].Length) + equipmentDefaultNames[usedKey]["Wrists"];
                }
            } else {
                WristsName = string.Empty;
            }

            if (UseHands == true) {
                if (HandsName == string.Empty) {
                    HandsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Hands"];
                } else if (HandsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    HandsName = equipmentSetName + HandsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    HandsName.Substring(HandsName.Length - equipmentDefaultNames[previousArmorClassName]["Hands"].Length) == equipmentDefaultNames[previousArmorClassName]["Hands"]) {
                    HandsName = HandsName.Substring(0, HandsName.Length - equipmentDefaultNames[previousArmorClassName]["Hands"].Length) + equipmentDefaultNames[usedKey]["Hands"];
                }
            } else {
                HandsName = string.Empty;
            }

            if (UseWaist == true) {
                if (WaistName == string.Empty) {
                    WaistName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Waist"];
                } else if (WaistName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    WaistName = equipmentSetName + WaistName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    WaistName.Substring(WaistName.Length - equipmentDefaultNames[previousArmorClassName]["Waist"].Length) == equipmentDefaultNames[previousArmorClassName]["Waist"]) {
                    WaistName = WaistName.Substring(0, WaistName.Length - equipmentDefaultNames[previousArmorClassName]["Waist"].Length) + equipmentDefaultNames[usedKey]["Waist"];
                }
            } else {
                WaistName = string.Empty;
            }

            if (UseLegs == true) {
                if (LegsName == string.Empty) {
                    LegsName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Legs"];
                } else if (LegsName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    LegsName = equipmentSetName + LegsName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    LegsName.Substring(LegsName.Length - equipmentDefaultNames[previousArmorClassName]["Legs"].Length) == equipmentDefaultNames[previousArmorClassName]["Legs"]) {
                    LegsName = LegsName.Substring(0, LegsName.Length - equipmentDefaultNames[previousArmorClassName]["Legs"].Length) + equipmentDefaultNames[usedKey]["Legs"];
                }
            } else {
                LegsName = string.Empty;
            }

            if (UseFeet == true) {
                if (FeetName == string.Empty) {
                    FeetName = equipmentSetName + " " + equipmentDefaultNames[usedKey]["Feet"];
                } else if (FeetName.Substring(0, previousEquipmentSetName.Length) == previousEquipmentSetName) {
                    FeetName = equipmentSetName + FeetName.Substring(previousEquipmentSetName.Length);
                }
                if (previousArmorClassName != usedKey &&
                    FeetName.Substring(FeetName.Length - equipmentDefaultNames[previousArmorClassName]["Feet"].Length) == equipmentDefaultNames[previousArmorClassName]["Feet"]) {
                    FeetName = FeetName.Substring(0, FeetName.Length - equipmentDefaultNames[previousArmorClassName]["Feet"].Length) + equipmentDefaultNames[usedKey]["Feet"];
                }
            } else {
                FeetName = string.Empty;
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

            if (UseHead == true && HeadName == string.Empty) {
                return "Head Name must not be empty";
            }

            if (UseShoulders == true && ShouldersName == string.Empty) {
                return "Shoulders Name must not be empty";
            }

            if (UseChest == true && ChestName == string.Empty) {
                return "Chest Name must not be empty";
            }

            if (UseWrists == true && WristsName == string.Empty) {
                return "Wrists Name must not be empty";
            }

            if (UseHands == true && HandsName == string.Empty) {
                return "Hands Name must not be empty";
            }

            if (UseWaist == true && WaistName == string.Empty) {
                return "Waist Name must not be empty";
            }

            if (UseLegs == true && LegsName == string.Empty) {
                return "Legs Name must not be empty";
            }

            if (UseFeet == true && FeetName == string.Empty) {
                return "Feet Name must not be empty";
            }

            if (createVendorCollection == true && vendorCollectionName == string.Empty) {
                return "Vendor Collection Name must not be empty";
            }

            return null;
        }
        
    }

}
