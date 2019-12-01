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
        
        //public UMASlot UMASlotAffinity;
        public UMA.UMATextRecipe UMARecipe = null;

        // The next 5 fiels are meant for weapons.  They are being left in the base equipment class for now in case we want to do something like attach a cape to the spine
        // However, this will likely not happen and these should probably just be moved to weapon.

        [SerializeField]
        private string holdableObjectName;

        [SerializeField]
        private List<HoldableObjectAttachment> holdableObjectList = new List<HoldableObjectAttachment>();

        [SerializeField]
        protected bool useArmorModifier;

        [SerializeField]
        protected bool useManualArmor;

        // should the manual value be per level instead of a total
        [SerializeField]
        protected bool manualValueIsScale;

        [SerializeField]
        protected float armorModifier;

        [SerializeField]
        protected bool useDamageModifier;

        [SerializeField]
        protected bool useManualDamage;

        [SerializeField]
        private int damageModifier;

        [SerializeField]
        private bool useIntellectModifier;

        [SerializeField]
        protected bool useManualIntellect;

        [SerializeField]
        private int intellectModifier;

        [SerializeField]
        private bool useStaminaModifier;

        [SerializeField]
        protected bool useManualStamina;

        [SerializeField]
        private int staminaModifier;

        [SerializeField]
        private bool useStrengthModifier;

        [SerializeField]
        protected bool useManualStrength;

        [SerializeField]
        private int strengthModifier;

        [SerializeField]
        private bool useAgilityModifier;

        [SerializeField]
        protected bool useManualAgility;

        [SerializeField]
        private int agilityModifier;

        [SerializeField]
        private BaseAbility onEquipAbility;

        [SerializeField]
        private List<BaseAbility> learnedAbilities;

        public virtual int MyDamageModifier {
            get {
                if (!useDamageModifier) {
                    return 0;
                }
                if (useManualDamage) {
                    return damageModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * ((MyRealEquipmentSlotType.MyStatWeight * MyRealEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
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
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClassName) * (GetItemQualityNumber() - 1f) ) * ((MyRealEquipmentSlotType.MyStatWeight * MyRealEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public virtual int MyStaminaModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (!useStaminaModifier) {
                    return 0;
                }
                if (useManualStamina) {
                    return staminaModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClassName) * (GetItemQualityNumber() - 1f)) * ((MyRealEquipmentSlotType.MyStatWeight * MyRealEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
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
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClassName) * (GetItemQualityNumber() - 1f)) * ((MyRealEquipmentSlotType.MyStatWeight * MyRealEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
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
                    (float)MyItemLevel * (LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClassName) * (GetItemQualityNumber() - 1f)) * ((MyRealEquipmentSlotType.MyStatWeight * MyRealEquipmentSlotType.GetCompatibleSlotProfiles()[0].MyStatWeight) / GetTotalSlotWeights()),
                    0f,
                    Mathf.Infinity
                    ));
        }
        public BaseAbility MyOnEquipAbility { get => onEquipAbility; set => onEquipAbility = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public string MyHoldableObjectName { get => holdableObjectName; set => holdableObjectName = value; }
        public bool MyUseManualIntellect { get => useManualIntellect; set => useManualIntellect = value; }
        public bool MyUseManualStamina { get => useManualStamina; set => useManualStamina = value; }
        public bool MyUseManualStrength { get => useManualStrength; set => useManualStrength = value; }
        public bool MyUseManualAgility { get => useManualAgility; set => useManualAgility = value; }
        public bool MyManualValueIsScale { get => manualValueIsScale; set => manualValueIsScale = value; }
        public string MyEquipmentSlotType { get => equipmentSlotType; set => equipmentSlotType = value; }
        public List<HoldableObjectAttachment> MyHoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }
        public EquipmentSlotType MyRealEquipmentSlotType { get => SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotType);  }

        /*
        public Equipment() {
            realEquipmentSlotType = SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotType);
        }
        */

        public float GetTotalSlotWeights() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                returnValue += equipmentSlotProfile.MyStatWeight;
            }
            return returnValue;
        }

        public float GetItemQualityNumber() {
            float returnValue = 1;
            if (GetItemQuality() != null) {
                returnValue = GetItemQuality().MyStatMultiplier;
            }
            return returnValue;
        }

        public override void Use() {
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                base.Use();
                if (CanEquip(PlayerManager.MyInstance.MyCharacter)) {
                    PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(this);
                    Remove();
                } else {
                }
            }
        }

        public virtual bool CanEquip(BaseCharacter baseCharacter) {
            if (MyCharacterClassRequirementList != null && MyCharacterClassRequirementList.Count > 0 && !MyCharacterClassRequirementList.Contains(baseCharacter.MyCharacterClassName)) {
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

            return base.GetSummary() + "\n" + string.Join("\n", abilitiesList);
        }

    }

    //public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }
}