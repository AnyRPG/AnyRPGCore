using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment", menuName = "AnyRPG/Inventory/Equipment/Equipment")]
    public class Equipment : Item {

        [Header("Equipment")]

        [SerializeField]
        protected string equipmentSlotType;

        private EquipmentSlotType realEquipmentSlotType;

        [SerializeField]
        private string umaRecipeProfileName = string.Empty;

        [SerializeField]
        private UMA.UMATextRecipe UMARecipe = null;

        [SerializeField]
        private List<UMA.UMATextRecipe> UMARecipes = new List<UMATextRecipe>();

        // The next 5 fiels are meant for weapons.  They are being left in the base equipment class for now in case we want to do something like attach a cape to the spine
        // However, this will likely not happen and these should probably just be moved to weapon.

        [SerializeField]
        private List<HoldableObjectAttachment> holdableObjectList = new List<HoldableObjectAttachment>();

        [Header("Base Armor")]

        [Tooltip("If true, this item will provide the wearer with armor")]
        [SerializeField]
        protected bool useArmorModifier = false;

        [Tooltip("If true, the armor value must be input manually and will not be calculated based on the item level and armor class")]
        [SerializeField]
        protected bool useManualArmor = false;

        [Tooltip("If true, the manual value is per level instead of a total")]
        [SerializeField]
        protected bool manualValueIsScale = false;

        [SerializeField]
        protected float armorModifier = 0f;

        [Header("Primary Stats")]

        [Tooltip("When equipped, the wearer will have these primary stats affected")]
        [SerializeField]
        protected List<ItemPrimaryStatNode> primaryStats = new List<ItemPrimaryStatNode>();

        [Header("Secondary Stats")]

        [Tooltip("If true, the secondary stats will be chosen randomly up to a limit defined by the item quality")]
        [SerializeField]
        protected bool randomSecondaryStats = false;

        [Tooltip("When equipped, the wearer will have these secondary stats affected")]
        [SerializeField]
        protected List<ItemSecondaryStatNode> secondaryStats = new List<ItemSecondaryStatNode>();

        [Header("Abilities")]

        [Tooltip("This ability will be cast when the item is equipped")]
        [SerializeField]
        private string onEquipAbilityName = string.Empty;

        private BaseAbility onEquipAbility;

        [Tooltip("These abilities will be learned when the item is equipped")]
        [SerializeField]
        private List<string> learnedAbilityNames = new List<string>();

        [Tooltip("The name of the equipment set this item belongs to, if any")]
        [SerializeField]
        private string equipmentSetName = string.Empty;

        private EquipmentSet equipmentSet = null;

        private List<int> randomStatIndexes = new List<int>();

        private List<ItemSecondaryStatNode> chosenSecondaryStats = new List<ItemSecondaryStatNode>();

        //[SerializeField]
        private List<BaseAbility> learnedAbilities = new List<BaseAbility>();

        public virtual float GetArmorModifier(int characterLevel) {
            if (!useArmorModifier) {
                return 0f;
            }
            if (useManualArmor) {
                if (manualValueIsScale) {
                    return (int)Mathf.Ceil(Mathf.Clamp(
                        (float)GetItemLevel(characterLevel) * (armorModifier * GetItemQualityNumber()),
                        0f,
                        Mathf.Infinity
                        ));

                }
                return armorModifier;
            }
            return 0f;
        }

        public virtual float GetPrimaryStatModifier(string statName, int currentLevel, BaseCharacter baseCharacter) {
            foreach (ItemPrimaryStatNode itemPrimaryStatNode in primaryStats) {
                if (statName == itemPrimaryStatNode.StatName) {
                    if (itemPrimaryStatNode.UseManualValue) {
                        return itemPrimaryStatNode.ManualModifierValue;
                    }
                    return (int)Mathf.Ceil(Mathf.Clamp(
                        (float)GetItemLevel(currentLevel) * (LevelEquations.GetPrimaryStatForLevel(statName, currentLevel, baseCharacter.CharacterClass) * (GetItemQualityNumber() - 1f)) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                        0f,
                        Mathf.Infinity
                        ));
                }
            }
            return 0f;
        }

        public virtual float GetSecondaryStatAddModifier(SecondaryStatType secondaryStatType, int characterLevel) {
            foreach (ItemSecondaryStatNode itemSecondaryStatNode in secondaryStats) {
                if (secondaryStatType == itemSecondaryStatNode.SecondaryStat) {
                    return itemSecondaryStatNode.BaseAmount + (itemSecondaryStatNode.AmountPerLevel * GetItemLevel(characterLevel));
                }
            }
            return 0f;
        }

        public virtual float GetSecondaryStatMultiplyModifier(SecondaryStatType secondaryStatType) {
            foreach (ItemSecondaryStatNode itemSecondaryStatNode in secondaryStats) {
                if (secondaryStatType == itemSecondaryStatNode.SecondaryStat) {
                    return itemSecondaryStatNode.BaseMultiplier;
                }
            }
            return 0;
        }

        public BaseAbility MyOnEquipAbility { get => onEquipAbility; set => onEquipAbility = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool MyManualValueIsScale { get => manualValueIsScale; set => manualValueIsScale = value; }
        public EquipmentSlotType MyEquipmentSlotType { get => realEquipmentSlotType; set => realEquipmentSlotType = value; }
        public List<HoldableObjectAttachment> MyHoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }
        public UMATextRecipe MyUMARecipe { get => UMARecipe; set => UMARecipe = value; }
        public EquipmentSet MyEquipmentSet { get => equipmentSet; set => equipmentSet = value; }
        public List<UMATextRecipe> MyUMARecipes { get => UMARecipes; set => UMARecipes = value; }
        public List<ItemPrimaryStatNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }

        public List<ItemSecondaryStatNode> SecondaryStats {
            get {
                if (randomSecondaryStats == true) {
                    return chosenSecondaryStats;
                }
                return secondaryStats;
            }
        }

        public List<ItemSecondaryStatNode> ChosenSecondaryStats { get => chosenSecondaryStats; set => chosenSecondaryStats = value; }
        public List<int> RandomStatIndexes { get => randomStatIndexes; set => randomStatIndexes = value; }

        public float GetTotalSlotWeights() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                returnValue += equipmentSlotProfile.MyStatWeight;
            }
            return returnValue;
        }

        /// <summary>
        /// return a multiplier value that is based on the item quality
        /// </summary>
        /// <returns></returns>
        public float GetItemQualityNumber() {
            float returnValue = 1;
            if (MyItemQuality != null) {
                returnValue = MyItemQuality.MyStatMultiplier;
            }
            return returnValue;
        }

        public override bool Use() {
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager != null) {
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                if (CanEquip(PlayerManager.MyInstance.MyCharacter)) {
                    PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Equip(this);
                    Remove();
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public virtual bool CanEquip(BaseCharacter baseCharacter) {
            if (!CharacterClassRequirementIsMet()) {
                MessageFeedManager.MyInstance.WriteMessage("You are not the right class to equip " + MyDisplayName);
                return false;
            }
            return true;
        }

        public override string GetSummary() {
            //Debug.Log(MyName + ".Equipment.GetSummary()");
            //string stats = string.Empty;
            List<string> abilitiesList = new List<string>();

            string itemRange = "";
            //Debug.Log(MyName + ": levelcap: " + levelCap + "; dynamicLevel: " + dynamicLevel);
            if (dynamicLevel == true) {
                itemRange = " (1 - " + (levelCap > 0 ? levelCap : SystemConfigurationManager.MyInstance.MyMaxLevel) + ")";
            }
            abilitiesList.Add(string.Format(" Item Level: {0}{1}", GetItemLevel(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level), itemRange));

            // armor
            if (useArmorModifier) {
                abilitiesList.Add(string.Format(" +{0} Armor", GetArmorModifier(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level)));
            }

            // primary stats
            foreach (ItemPrimaryStatNode itemPrimaryStatNode in primaryStats) {
                float primaryStatModifier = GetPrimaryStatModifier(itemPrimaryStatNode.StatName, PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, PlayerManager.MyInstance.MyCharacter);
                if (primaryStatModifier > 0f) {
                    abilitiesList.Add(string.Format(" +{0} {1}",
                        primaryStatModifier,
                        itemPrimaryStatNode.StatName));
                }
            }

            // secondary stats
            foreach (ItemSecondaryStatNode itemSecondaryStatNode in SecondaryStats) {
                abilitiesList.Add(string.Format("<color=green> +{0} {1}</color>",
                                   GetSecondaryStatAddModifier(itemSecondaryStatNode.SecondaryStat, PlayerManager.MyInstance.MyCharacter.CharacterStats.Level),
                                   itemSecondaryStatNode.SecondaryStat.ToString()));
            }

            // abilities
            if (onEquipAbility != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Equip: {0}</color>", onEquipAbility.MyDisplayName));
            }
            foreach (BaseAbility learnedAbility in MyLearnedAbilities) {
                abilitiesList.Add(string.Format("<color=green>Learn On Equip: {0}</color>", learnedAbility.MyDisplayName));
            }

            if (equipmentSet != null) {
                int equipmentCount = PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.GetEquipmentSetCount(equipmentSet);
                abilitiesList.Add(string.Format("\n<color=yellow>{0} ({1}/{2})</color>", equipmentSet.MyDisplayName, equipmentCount, equipmentSet.MyEquipmentList.Count));
                foreach (Equipment equipment in equipmentSet.MyEquipmentList) {
                    string colorName = "#888888";
                    if (PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.HasEquipment(equipment.MyDisplayName)) {
                        colorName = "yellow";
                    }
                    abilitiesList.Add(string.Format("  <color={0}>{1}</color>", colorName, equipment.MyDisplayName));
                }
                abilitiesList.Add(string.Format(""));
                for (int i = 0; i < equipmentSet.MyTraitList.Count; i++) {
                    if (equipmentSet.MyTraitList[i] != null) {
                        string colorName = "#888888";
                        if (equipmentCount > i) {
                            colorName = "green";
                        }
                        abilitiesList.Add(string.Format("<color={0}>({1}) {2}</color>", colorName, i+1, equipmentSet.MyTraitList[i].GetSummary()));
                    }
                }
                if (equipmentSet.MyTraitList.Count > 0) {
                    abilitiesList.Add(string.Format(""));
                }
            }

            return base.GetSummary() + "\n" + string.Join("\n", abilitiesList);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            onEquipAbility = null;
            if (onEquipAbilityName != null && onEquipAbilityName != string.Empty) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(onEquipAbilityName);
                if (baseAbility != null) {
                    onEquipAbility = baseAbility;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + onEquipAbilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            learnedAbilities = new List<BaseAbility>();
            if (learnedAbilityNames != null) {
                foreach (string baseAbilityName in learnedAbilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        learnedAbilities.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
            
            
            realEquipmentSlotType = null;
            if (equipmentSlotType != null && equipmentSlotType != string.Empty) {
                EquipmentSlotType tmpEquipmentSlotType = SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotType);
                if (tmpEquipmentSlotType != null) {
                    realEquipmentSlotType = tmpEquipmentSlotType;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment slot type : " + equipmentSlotType + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): EquipmentSlotType is a required field while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
            }

            equipmentSet = null;
            if (equipmentSetName != null && equipmentSetName != string.Empty) {
                EquipmentSet tmpEquipmentSet = SystemEquipmentSetManager.MyInstance.GetResource(equipmentSetName);
                if (tmpEquipmentSet != null) {
                    equipmentSet = tmpEquipmentSet;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment set : " + equipmentSetName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }


            if (holdableObjectList != null) {
                foreach (HoldableObjectAttachment holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects();
                    }
                }
            }

            if (umaRecipeProfileName != null && umaRecipeProfileName != string.Empty) {
                UMARecipeProfile umaRecipeProfile = SystemUMARecipeProfileManager.MyInstance.GetResource(umaRecipeProfileName);
                if (umaRecipeProfile != null && umaRecipeProfile.MyUMARecipes != null) {
                    UMARecipes = umaRecipeProfile.MyUMARecipes;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find uma recipe profile : " + umaRecipeProfileName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }


        }

        public override void InitializeNewItem() {
            base.InitializeNewItem();

            if (randomSecondaryStats == false) {
                return;
            }
            if (realItemQuality == null) {
                return;
            }
            if (realItemQuality.RandomStatCount == 0) {
                return;
            }
            // get the max number, and cycling through the list and adding them to our current list and index
            int maxCount = Mathf.Min(secondaryStats.Count, realItemQuality.RandomStatCount);
            while (RandomStatIndexes.Count < maxCount) {
                int randomNumber = UnityEngine.Random.Range(0, secondaryStats.Count);
                if (!RandomStatIndexes.Contains(randomNumber)) {
                    RandomStatIndexes.Add(randomNumber);
                }
            }
            InitializeRandomStatsFromIndex();
        }

        public void InitializeRandomStatsFromIndex() {
            chosenSecondaryStats.Clear();
            foreach (int randomIndex in RandomStatIndexes) {
                chosenSecondaryStats.Add(secondaryStats[randomIndex]);
            }
        }

    }

    [System.Serializable]
    public class ItemPrimaryStatNode {

        [Tooltip("The primary stat to increase when this item is equipped.  By default, this stat will be automatically set based on the item level")]
        [SerializeField]
        private string statName = string.Empty;

        [Tooltip("If true, the stat value entered in the manual modifier value field will be used instead of the automatically scaled value")]
        [SerializeField]
        private bool useManualValue = false;

        [Tooltip("If use manual value is true, the value in this field will be used instead of the automatically scaled value")]
        [SerializeField]
        private float manualModifierValue = 0;

        public string StatName { get => statName; set => statName = value; }
        public float ManualModifierValue { get => manualModifierValue; set => manualModifierValue = value; }
        public bool UseManualValue { get => useManualValue; set => useManualValue = value; }
    }

    [System.Serializable]
    public class ItemSecondaryStatNode {

        [Tooltip("The secondary stat to increase when this item is equipped.")]
        [SerializeField]
        private SecondaryStatType secondaryStat;

        [Tooltip("This value is constant, and does not scale with level")]
        [SerializeField]
        private float baseAmount = 0;

        [Tooltip("The value will be multiplied by the item level of the equipment")]
        [SerializeField]
        private float amountPerLevel = 0;

        [Tooltip("After amount values are added together, they will be multiplied by this number")]
        [SerializeField]
        private float baseMultiplier = 1f;

        public SecondaryStatType SecondaryStat { get => secondaryStat; set => secondaryStat = value; }
        public float BaseAmount { get => baseAmount; set => baseAmount = value; }
        public float AmountPerLevel { get => amountPerLevel; set => amountPerLevel = value; }
        public float BaseMultiplier { get => baseMultiplier; set => baseMultiplier = value; }
    }

    //public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }
}