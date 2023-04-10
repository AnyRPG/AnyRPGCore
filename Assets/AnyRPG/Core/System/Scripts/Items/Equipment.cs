using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment", menuName = "AnyRPG/Inventory/Equipment/Equipment")]
    public class Equipment : Item {

        [Header("Equipment")]

        [Tooltip("The equipment slot this item can be equippped in")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSlotType))]
        protected string equipmentSlotType;

        private EquipmentSlotType realEquipmentSlotType;

        [Tooltip("The name of the equipment set this item belongs to, if any")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentSet))]
        private string equipmentSetName = string.Empty;

        // keep a reference to the actual equipment set
        private EquipmentSet equipmentSet = null;

        [Header("UMA Equipment Models")]

        [FormerlySerializedAs("umaRecipeProfileName")]
        [SerializeField]
        private string deprecatedUmaRecipeProfileName = string.Empty;

        [FormerlySerializedAs("uMARecipeProfileProperties")]
        [SerializeField]
        private UMAEquipmentModelProperties deprecatedUMARecipeProfileProperties = new UMAEquipmentModelProperties();

        [Header("Prefab Equipment Models")]

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [FormerlySerializedAs("holdableObjectList")]
        [SerializeField]
        private List<HoldableObjectAttachment> deprecatedHoldableObjectList = new List<HoldableObjectAttachment>();

        [Header("Equipment Models")]

        [Tooltip("Inline equipment model definitions.")]
        [SerializeField]
        private EquipmentModelProperties inlineEquipmentModels = new EquipmentModelProperties();

        [Tooltip("Shared equipment model definitions.  This will override any inline equipment model properties.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EquipmentModelProfile))]
        private string sharedEquipmentModels = string.Empty;

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

        [Tooltip("This status effect will be cast on the character when the item is equipped.")]
        [FormerlySerializedAs("onEquipAbilityName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        private string onEquipStatusEffect = string.Empty;

        private StatusEffect onEquipStatusEffectRef;

        [Tooltip("These abilities will be learned when the item is equipped")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private List<string> learnedAbilityNames = new List<string>();

        private List<int> randomStatIndexes = new List<int>();

        private List<ItemSecondaryStatNode> chosenSecondaryStats = new List<ItemSecondaryStatNode>();

        //[SerializeField]
        private List<BaseAbilityProperties> learnedAbilities = new List<BaseAbilityProperties>();

        private Dictionary<Type, EquipmentModel> equipmentModelDictionary = new Dictionary<Type, EquipmentModel>();

        public float GetArmorModifier(int characterLevel) {
            return GetArmorModifier(characterLevel, realItemQuality);
        }

        public virtual float GetArmorModifier(int characterLevel, ItemQuality usedItemQuality) {
            if (!useArmorModifier) {
                return 0f;
            }
            if (useManualArmor) {
                if (manualValueIsScale) {
                    return (int)Mathf.Ceil(Mathf.Clamp(
                        (float)GetItemLevel(characterLevel) * (armorModifier * GetItemQualityNumber(usedItemQuality)),
                        0f,
                        Mathf.Infinity
                        ));

                }
                return armorModifier;
            }
            return 0f;
        }

        public float GetPrimaryStatModifier(string statName, int currentLevel, BaseCharacter baseCharacter) {
            return GetPrimaryStatModifier(statName, currentLevel, baseCharacter, realItemQuality);
        }

        public virtual float GetPrimaryStatModifier(string statName, int currentLevel, BaseCharacter baseCharacter, ItemQuality usedItemQuality) {
            foreach (ItemPrimaryStatNode itemPrimaryStatNode in primaryStats) {
                if (statName == itemPrimaryStatNode.StatName) {
                    if (itemPrimaryStatNode.UseManualValue) {
                        return itemPrimaryStatNode.ManualModifierValue;
                    }
                    return (int)Mathf.Ceil(Mathf.Clamp(
                        (float)GetItemLevel(currentLevel) * (LevelEquations.GetPrimaryStatForLevel(statName, currentLevel, baseCharacter, systemConfigurationManager) * (GetItemQualityNumber(usedItemQuality) - 1f)) * ((EquipmentSlotType.StatWeight * EquipmentSlotType.GetCompatibleSlotProfiles()[0].StatWeight) / GetTotalSlotWeights()),
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

        public StatusEffect OnEquipAbilityEffect { get => onEquipStatusEffectRef; set => onEquipStatusEffectRef = value; }
        public virtual List<BaseAbilityProperties> LearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool ManualValueIsScale { get => manualValueIsScale; set => manualValueIsScale = value; }
        public string EquipmentSlotTypeName { get => equipmentSlotType; set => equipmentSlotType = value; }
        public EquipmentSlotType EquipmentSlotType { get => realEquipmentSlotType; set => realEquipmentSlotType = value; }
        public List<HoldableObjectAttachment> DeprecatedHoldableObjectList { get => deprecatedHoldableObjectList; set => deprecatedHoldableObjectList = value; }
        public EquipmentSet EquipmentSet { get => equipmentSet; set => equipmentSet = value; }
        public List<ItemPrimaryStatNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public bool RandomSecondaryStats { get => randomSecondaryStats; set => randomSecondaryStats = value; }
        public List<ItemSecondaryStatNode> SecondaryStats {
            get {
                if (randomSecondaryStats == true) {
                    return chosenSecondaryStats;
                }
                return secondaryStats;
            }
            set {
                secondaryStats = value;
            }
        }

        public List<ItemSecondaryStatNode> ChosenSecondaryStats { get => chosenSecondaryStats; set => chosenSecondaryStats = value; }
        public List<int> RandomStatIndexes { get => randomStatIndexes; set => randomStatIndexes = value; }
        public UMAEquipmentModelProperties DeprecatedUMARecipeProfileProperties { get => deprecatedUMARecipeProfileProperties; set => deprecatedUMARecipeProfileProperties = value; }
        public string DeprecatedUmaRecipeProfileName { get => deprecatedUmaRecipeProfileName; set => deprecatedUmaRecipeProfileName = value; }
        public string EquipmentSetName { get => equipmentSetName; set => equipmentSetName = value; }
        public bool UseArmorModifier { get => useArmorModifier; set => useArmorModifier = value; }
        public string SharedEquipmentModels { get => sharedEquipmentModels; set => sharedEquipmentModels = value; }
        public EquipmentModelProperties InlineEquipmentModels { get => inlineEquipmentModels; set => inlineEquipmentModels = value; }

        public float GetTotalSlotWeights() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in systemDataFactory.GetResourceList<EquipmentSlotProfile>()) {
                returnValue += equipmentSlotProfile.StatWeight;
            }
            return returnValue;
        }

        /// <summary>
        /// return a multiplier value that is based on the item quality
        /// </summary>
        /// <returns></returns>
        public float GetItemQualityNumber(ItemQuality usedItemQuality) {
            float returnValue = 1;
            if (usedItemQuality != null) {
                returnValue = usedItemQuality.StatMultiplier;
            }
            return returnValue;
        }

        public override bool Use() {
            if (playerManager.MyCharacter?.CharacterEquipmentManager != null) {
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                if (playerManager.MyCharacter.CharacterEquipmentManager.Equip(this) == true) {
                    playerManager.UnitController.UnitModelController.RebuildModelAppearance();
                    Remove();
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public virtual bool CanEquip(BaseCharacter baseCharacter) {
            //Debug.Log(DisplayName + ".Equipment.CanEquip(" + baseCharacter.gameObject.name + ")");
            if (!CharacterClassRequirementIsMet(baseCharacter)) {
                //Debug.Log(baseCharacter.gameObject.name + "." + DisplayName + ".Equipment.CanEquip(): not the right character class");
                return false;
            }
            if (!CapabilityConsumerSupported(baseCharacter)) {
                //Debug.Log(baseCharacter.gameObject.name + "." + DisplayName + ".Equipment.CanEquip(): CapabilityConsumer unsupported");
                return false;
            }
            if (GetItemLevel(baseCharacter.CharacterStats.Level) > baseCharacter.CharacterStats.Level) {
                //Debug.Log(baseCharacter.gameObject.name + "." + DisplayName + ".Equipment.CanEquip(): character level too low (" + baseCharacter.CharacterStats.Level + ")");
                return false;
            }
            return true;
        }

        public virtual void HandleEquip(CharacterCombat characterCombat, EquipmentSlotProfile equipmentSlotProfile) {
            // nothing here yet
        }

        public virtual void HandleUnequip(CharacterCombat characterCombat, EquipmentSlotProfile equipmentSlotProfile) {
            // nothing here yet
        }

        public virtual void HandleEquip(CharacterEquipmentManager characterEquipmentManager) {
            // nothing here yet
        }

        public virtual void HandleUnequip(CharacterEquipmentManager characterEquipmentManager) {
            // nothing here yet
        }

        public bool HasEquipmentModel<T>() where T : EquipmentModel {
            if (equipmentModelDictionary.ContainsKey(typeof(T))) {
                return true;
            }

            return false;
        }

        public T GetEquipmentModel<T>() where T : EquipmentModel {
            if (equipmentModelDictionary.ContainsKey(typeof(T))) {
                return equipmentModelDictionary[typeof(T)] as T;
            }

            return null;
        }

        /// <summary>
        /// meant to be overwritten by specific equipment types
        /// </summary>
        /// <param name="capabilityConsumer"></param>
        /// <returns></returns>
        public virtual bool CapabilityConsumerSupported(ICapabilityConsumer capabilityConsumer) {
            //return false;
            // change default to true, or accessories cannot be equipped
            return true;
        }

        public override string GetDescription(ItemQuality usedItemQuality) {
            //Debug.Log(DisplayName + ".Equipment.GetSummary()");
            //string stats = string.Empty;
            List<string> summaryLines = new List<string>();

            string itemRange = "";
            string colorstring = string.Empty;
            //Debug.Log(DisplayName + ": levelcap: " + levelCap + "; dynamicLevel: " + dynamicLevel);
            if (dynamicLevel == true && freezeDropLevel == false) {
                itemRange = " (1 - " + (levelCap > 0 ? levelCap : systemConfigurationManager.MaxLevel) + ")";
            }
            if (GetItemLevel(playerManager.MyCharacter.CharacterStats.Level) > playerManager.MyCharacter.CharacterStats.Level) {
                colorstring = "red";
            } else {
                colorstring = "white";
            }
            summaryLines.Add(string.Format("<color={0}>Item Level: {1}{2}</color>", colorstring, GetItemLevel(playerManager.MyCharacter.CharacterStats.Level), itemRange));

            // armor
            if (useArmorModifier) {
                summaryLines.Add(string.Format(" +{0} Armor", GetArmorModifier(playerManager.MyCharacter.CharacterStats.Level, usedItemQuality)));
            }

            // primary stats
            foreach (ItemPrimaryStatNode itemPrimaryStatNode in primaryStats) {
                float primaryStatModifier = GetPrimaryStatModifier(itemPrimaryStatNode.StatName, playerManager.MyCharacter.CharacterStats.Level, playerManager.MyCharacter, usedItemQuality);
                if (primaryStatModifier > 0f) {
                    summaryLines.Add(string.Format(" +{0} {1}",
                        primaryStatModifier,
                        itemPrimaryStatNode.StatName));
                }
            }

            // secondary stats
            foreach (ItemSecondaryStatNode itemSecondaryStatNode in SecondaryStats) {
                summaryLines.Add(string.Format("<color=green> +{0} {1}</color>",
                                   GetSecondaryStatAddModifier(itemSecondaryStatNode.SecondaryStat, playerManager.MyCharacter.CharacterStats.Level),
                                   itemSecondaryStatNode.SecondaryStat.ToString()));
            }

            // abilities
            if (onEquipStatusEffectRef != null) {
                summaryLines.Add(string.Format("<color=green>Cast On Equip: {0}</color>", onEquipStatusEffectRef.DisplayName));
            }
            foreach (BaseAbilityProperties learnedAbility in LearnedAbilities) {
                summaryLines.Add(string.Format("<color=green>Learn On Equip: {0}</color>", learnedAbility.DisplayName));
            }

            if (equipmentSet != null) {
                int equipmentCount = playerManager.MyCharacter.CharacterEquipmentManager.GetEquipmentSetCount(equipmentSet);
                summaryLines.Add(string.Format("\n<color=yellow>{0} ({1}/{2})</color>", equipmentSet.DisplayName, equipmentCount, equipmentSet.EquipmentList.Count));
                foreach (Equipment equipment in equipmentSet.EquipmentList) {
                    string colorName = "#888888";
                    if (playerManager.MyCharacter.CharacterEquipmentManager.HasEquipment(equipment.ResourceName)) {
                        colorName = "yellow";
                    }
                    summaryLines.Add(string.Format("  <color={0}>{1}</color>", colorName, equipment.DisplayName));
                }
                summaryLines.Add(string.Format(""));
                for (int i = 0; i < equipmentSet.TraitList.Count; i++) {
                    if (equipmentSet.TraitList[i] != null) {
                        string colorName = "#888888";
                        if (equipmentCount > i) {
                            colorName = "green";
                        }
                        summaryLines.Add(string.Format("<color={0}>({1}) {2}</color>", colorName, i+1, equipmentSet.TraitList[i].GetDescription()));
                    }
                }
                if (equipmentSet.TraitList.Count > 0) {
                    summaryLines.Add(string.Format(""));
                }
            }

            return base.GetDescription(usedItemQuality) + "\n\n" + string.Join("\n", summaryLines);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (onEquipStatusEffect != null && onEquipStatusEffect != string.Empty) {
                StatusEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(onEquipStatusEffect) as StatusEffect;
                if (abilityEffect != null) {
                    onEquipStatusEffectRef = abilityEffect;
                } else {
                    Debug.LogError("Equipment.SetupScriptableObjects(): Could not find status effect : " + onEquipStatusEffect + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

            learnedAbilities = new List<BaseAbilityProperties>();
            if (learnedAbilityNames != null) {
                foreach (string baseAbilityName in learnedAbilityNames) {
                    BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(baseAbilityName);
                    if (baseAbility != null) {
                        learnedAbilities.Add(baseAbility.AbilityProperties);
                    } else {
                        Debug.LogError("Equipment.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }
            
            
            realEquipmentSlotType = null;
            if (equipmentSlotType != null && equipmentSlotType != string.Empty) {
                EquipmentSlotType tmpEquipmentSlotType = systemDataFactory.GetResource<EquipmentSlotType>(equipmentSlotType);
                if (tmpEquipmentSlotType != null) {
                    realEquipmentSlotType = tmpEquipmentSlotType;
                } else {
                    Debug.LogError("Equipment.SetupScriptableObjects(): Could not find equipment slot type : " + equipmentSlotType + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("Equipment.SetupScriptableObjects(): EquipmentSlotType is a required field while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
            }

            equipmentSet = null;
            if (equipmentSetName != null && equipmentSetName != string.Empty) {
                EquipmentSet tmpEquipmentSet = systemDataFactory.GetResource<EquipmentSet>(equipmentSetName);
                if (tmpEquipmentSet != null) {
                    equipmentSet = tmpEquipmentSet;
                } else {
                    Debug.LogError("Equipment.SetupScriptableObjects(): Could not find equipment set : " + equipmentSetName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

            if (sharedEquipmentModels != string.Empty) {
                EquipmentModelProfile tmpEquipmentModelProfile = systemDataFactory.GetResource<EquipmentModelProfile>(sharedEquipmentModels);
                if (tmpEquipmentModelProfile != null) {
                    inlineEquipmentModels = tmpEquipmentModelProfile.Properties;
                } else {
                    Debug.LogError($"Equipment.SetupScriptableObjects(): Could not find equipment model profile : {sharedEquipmentModels} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            foreach (EquipmentModel equipmentModel in inlineEquipmentModels.EquipmentModels) {
                if (equipmentModel != null) {
                    equipmentModel.Configure(systemGameManager);
                    equipmentModel.SetupScriptableObjects(this);
                    //Debug.Log($"Equipment.SetupScriptableObjects(): adding type {equipmentModel.GetType().Name} for {ResourceName}");
                    equipmentModelDictionary.Add(equipmentModel.GetType(), equipmentModel);
                }
            }
        }

        public override void InitializeNewItem(ItemQuality itemQuality = null) {
            base.InitializeNewItem(itemQuality);

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
        [ResourceSelector(resourceType = typeof(CharacterStat))]
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
        public float BaseMultiplier {
            get {
                if (baseMultiplier == 0f) {
                    // equipment should not be able to reduce stats to zero, so ignore zero values
                    return 1f;
                }
                return baseMultiplier;
            }
            set => baseMultiplier = value;
        }
    }

    //public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }
}