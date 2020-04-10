using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment", menuName = "AnyRPG/Inventory/Equipment/Equipment")]
    public class Equipment : Item {

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

        [SerializeField]
        protected bool useArmorModifier = false;

        [SerializeField]
        protected bool useManualArmor = false;

        // should the manual value be per level instead of a total
        [SerializeField]
        protected bool manualValueIsScale = false;

        [SerializeField]
        protected float armorModifier = 0f;

        [SerializeField]
        protected bool useDamageModifier = false;

        [SerializeField]
        protected bool useManualDamage = false;

        [SerializeField]
        private int damageModifier = 0;

        [SerializeField]
        private bool useIntellectModifier = false;

        [SerializeField]
        protected bool useManualIntellect = false;

        [SerializeField]
        private int intellectModifier = 0;

        [SerializeField]
        private bool useStaminaModifier = false;

        [SerializeField]
        protected bool useManualStamina = false;

        [SerializeField]
        private int staminaModifier = 0;

        [SerializeField]
        private bool useStrengthModifier = false;

        [SerializeField]
        protected bool useManualStrength = false;

        [SerializeField]
        private int strengthModifier = 0;

        [SerializeField]
        private bool useAgilityModifier = false;

        [SerializeField]
        protected bool useManualAgility = false;

        [SerializeField]
        private int agilityModifier = 0;

        [SerializeField]
        private string onEquipAbilityName = string.Empty;

        //[SerializeField]
        private BaseAbility onEquipAbility;

        [SerializeField]
        private List<string> learnedAbilityNames = new List<string>();

        [SerializeField]
        private string equipmentSetName = string.Empty;

        private EquipmentSet equipmentSet = null;

        //[SerializeField]
        private List<BaseAbility> learnedAbilities = new List<BaseAbility>();

        public virtual int MyDamageModifier {
            get {
                if (!useDamageModifier) {
                    return 0;
                }
                if (useManualDamage) {
                    return damageModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => damageModifier = value;
        }
        public virtual float MyArmorModifier {
            get {
                if (!useArmorModifier) {
                    return 0;
                }
                if (useManualArmor) {
                    if (manualValueIsScale) {
                        return (int)Mathf.Ceil(Mathf.Clamp(
                            (float)MyItemLevel * (armorModifier * GetItemQualityNumber()),
                            0f,
                            Mathf.Infinity
                            ));

                    }
                    return armorModifier;
                }
                return 0;
            }
            set => armorModifier = value;
        }

        public virtual int MyIntellectModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (!useIntellectModifier) {
                    return 0;
                }
                if (useManualIntellect) {
                    return intellectModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClass) * (GetItemQualityNumber() - 1f) ) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public virtual int MyStaminaModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (!useStaminaModifier || baseCharacter == null) {
                    return 0;
                }
                if (useManualStamina) {
                    return staminaModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClass) * (GetItemQualityNumber() - 1f)) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public virtual int MyStrengthModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (!useStrengthModifier) {
                    return 0;
                }
                if (useManualStrength) {
                    return strengthModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClass) * (GetItemQualityNumber() - 1f)) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public virtual int MyAgilityModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (!useAgilityModifier) {
                    return 0;
                }
                if (useManualAgility) {
                    return agilityModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClass) * (GetItemQualityNumber() - 1f)) * ((MyEquipmentSlotType.MyStatWeight * MyEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public BaseAbility MyOnEquipAbility { get => onEquipAbility; set => onEquipAbility = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool MyUseManualIntellect { get => useManualIntellect; set => useManualIntellect = value; }
        public bool MyUseManualStamina { get => useManualStamina; set => useManualStamina = value; }
        public bool MyUseManualStrength { get => useManualStrength; set => useManualStrength = value; }
        public bool MyUseManualAgility { get => useManualAgility; set => useManualAgility = value; }
        public bool MyManualValueIsScale { get => manualValueIsScale; set => manualValueIsScale = value; }
        public EquipmentSlotType MyEquipmentSlotType { get => realEquipmentSlotType; set => realEquipmentSlotType = value; }
        public List<HoldableObjectAttachment> MyHoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }
        public UMATextRecipe MyUMARecipe { get => UMARecipe; set => UMARecipe = value; }
        public EquipmentSet MyEquipmentSet { get => equipmentSet; set => equipmentSet = value; }
        public List<UMATextRecipe> MyUMARecipes { get => UMARecipes; set => UMARecipes = value; }

        public float GetTotalSlotWeights() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                returnValue += equipmentSlotProfile.MyStatWeight;
            }
            return returnValue;
        }

        public float GetItemQualityNumber() {
            float returnValue = 1;
            if (MyItemQuality != null) {
                returnValue = MyItemQuality.MyStatMultiplier;
            }
            return returnValue;
        }

        public override bool Use() {
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                bool returnValue = base.Use();
                if (returnValue == false) {
                    return false;
                }
                if (CanEquip(PlayerManager.MyInstance.MyCharacter)) {
                    PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(this);
                    Remove();
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public virtual bool CanEquip(BaseCharacter baseCharacter) {
            if (MyCharacterClassRequirementList != null && MyCharacterClassRequirementList.Count > 0 && !MyCharacterClassRequirementList.Contains(baseCharacter.MyCharacterClass)) {
                MessageFeedManager.MyInstance.WriteMessage("You are not the right class to equip " + MyName);
                return false;
            }
            return true;
        }

        public override string GetSummary() {
            //string stats = string.Empty;
            List<string> abilitiesList = new List<string>();

            abilitiesList.Add(string.Format(" Item Level: {0}", MyItemLevel));

            // stats
            if (useArmorModifier) {
                abilitiesList.Add(string.Format(" +{0} Armor", MyArmorModifier));
            }
            if (useDamageModifier) {
                abilitiesList.Add(string.Format(" +{0} Damage", MyDamageModifier));
            }
            if (useStaminaModifier) {
                abilitiesList.Add(string.Format(" +{0} Stamina", MyStaminaModifier(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, PlayerManager.MyInstance.MyCharacter)));
            }
            if (useStrengthModifier) {
                abilitiesList.Add(string.Format(" +{0} Strength", MyStrengthModifier(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, PlayerManager.MyInstance.MyCharacter)));
            }
            if (useIntellectModifier) {
                abilitiesList.Add(string.Format(" +{0} Intellect", MyIntellectModifier(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, PlayerManager.MyInstance.MyCharacter)));
            }
            if (useAgilityModifier) {
                abilitiesList.Add(string.Format(" +{0} Agility", MyAgilityModifier(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, PlayerManager.MyInstance.MyCharacter)));
            }

            // abilities

            if (onEquipAbility != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Equip: {0}</color>", onEquipAbility.MyName));
            }
            foreach (BaseAbility learnedAbility in MyLearnedAbilities) {
                abilitiesList.Add(string.Format("<color=green>Learn On Equip: {0}</color>", learnedAbility.MyName));
            }

            if (equipmentSet != null) {
                int equipmentCount = PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.GetEquipmentSetCount(equipmentSet);
                abilitiesList.Add(string.Format("\n<color=yellow>{0} ({1}/{2})</color>", equipmentSet.MyName, equipmentCount, equipmentSet.MyEquipmentList.Count));
                foreach (Equipment equipment in equipmentSet.MyEquipmentList) {
                    string colorName = "#888888";
                    if (PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.HasEquipment(equipment.MyName)) {
                        colorName = "yellow";
                    }
                    abilitiesList.Add(string.Format("  <color={0}>{1}</color>", colorName, equipment.MyName));
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
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + onEquipAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            learnedAbilities = new List<BaseAbility>();
            if (learnedAbilityNames != null) {
                foreach (string baseAbilityName in learnedAbilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        learnedAbilities.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }
            
            
            realEquipmentSlotType = null;
            if (equipmentSlotType != null && equipmentSlotType != string.Empty) {
                EquipmentSlotType tmpEquipmentSlotType = SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotType);
                if (tmpEquipmentSlotType != null) {
                    realEquipmentSlotType = tmpEquipmentSlotType;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment slot type : " + equipmentSlotType + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): EquipmentSlotType is a required field while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }

            equipmentSet = null;
            if (equipmentSetName != null && equipmentSetName != string.Empty) {
                EquipmentSet tmpEquipmentSet = SystemEquipmentSetManager.MyInstance.GetResource(equipmentSetName);
                if (tmpEquipmentSet != null) {
                    equipmentSet = tmpEquipmentSet;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment set : " + equipmentSetName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
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
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find uma recipe profile : " + umaRecipeProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }


        }

    }

    //public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }
}