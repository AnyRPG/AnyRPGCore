using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment", menuName = "AnyRPG/Inventory/Equipment")]
    public class Equipment : Item {

        public EquipmentSlot equipSlot;
        //public UMASlot UMASlotAffinity;
        public UMA.UMATextRecipe UMARecipe = null;

        // The next 5 fiels are meant for weapons.  They are being left in the base equipment class for now in case we want to do something like attach a cape to the spine
        // However, this will likely not happen and these should probably just be moved to weapon.

        [SerializeField]
        private string holdableObjectName;

        [SerializeField]
        private bool useArmorModifier;

        [SerializeField]
        private bool useManualArmor;

        [SerializeField]
        private int armorModifier;

        [SerializeField]
        private bool useDamageModifier;

        [SerializeField]
        private bool useManualDamage;

        [SerializeField]
        private int damageModifier;

        [SerializeField]
        private bool useIntellectModifier;

        [SerializeField]
        private bool useManualIntellect;

        [SerializeField]
        private int intellectModifier;

        [SerializeField]
        private bool useStaminaModifier;

        [SerializeField]
        private bool useManualStamina;

        [SerializeField]
        private int staminaModifier;

        [SerializeField]
        private bool useStrengthModifier;

        [SerializeField]
        private bool useManualStrength;

        [SerializeField]
        private int strengthModifier;

        [SerializeField]
        private bool useAgilityModifier;

        [SerializeField]
        private bool useManualAgility;

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
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length)),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => damageModifier = value;
        }
        public virtual int MyArmorModifier {
            get {
                if (!useArmorModifier) {
                    return 0;
                }
                if (useManualArmor) {
                    return armorModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length)),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => armorModifier = value;
        }

        public virtual int MyIntellectModifier {
            get {
                if (!useIntellectModifier) {
                    return 0;
                }
                if (useManualIntellect) {
                    return intellectModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f) ) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length) ),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => intellectModifier = value;
        }
        public virtual int MyStaminaModifier {
            get {
                if (!useStaminaModifier) {
                    return 0;
                }
                if (useManualStamina) {
                    return staminaModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length)),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => staminaModifier = value;
        }
        public virtual int MyStrengthModifier {
            get {
                if (!useStrengthModifier) {
                    return 0;
                }
                if (useManualStrength) {
                    return strengthModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length)),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => strengthModifier = value;
        }
        public virtual int MyAgilityModifier {
            get {
                if (!useAgilityModifier) {
                    return 0;
                }
                if (useManualAgility) {
                    return agilityModifier;
                }
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel * (GetItemQualityNumber() - 1f)) * (1f / (float)(Enum.GetNames(typeof(EquipmentSlot)).Length)),
                    0f,
                    Mathf.Infinity
                    ));
            }
            set => agilityModifier = value;
        }
        public BaseAbility MyOnEquipAbility { get => onEquipAbility; set => onEquipAbility = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public string MyHoldableObjectName { get => holdableObjectName; set => holdableObjectName = value; }
        public bool MyUseManualIntellect { get => useManualIntellect; set => useManualIntellect = value; }
        public bool MyUseManualStamina { get => useManualStamina; set => useManualStamina = value; }
        public bool MyUseManualStrength { get => useManualStrength; set => useManualStrength = value; }
        public bool MyUseManualAgility { get => useManualAgility; set => useManualAgility = value; }

        public override void Start() {
            base.Start();
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
                PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(this);
                Remove();
            }
        }

        public override string GetSummary() {
            //string stats = string.Empty;
            List<string> abilitiesList = new List<string>();

            abilitiesList.Add(string.Format(" Item Level: {0}", MyItemLevel));

            if (useArmorModifier) {
                abilitiesList.Add(string.Format(" +{0} Armor", MyArmorModifier));
            }
            if (useDamageModifier) {
                abilitiesList.Add(string.Format(" +{0} Damage", MyDamageModifier));
            }
            if (useStaminaModifier) {
                abilitiesList.Add(string.Format(" +{0} Stamina", MyStaminaModifier));
            }
            if (useStrengthModifier) {
                abilitiesList.Add(string.Format(" +{0} Strength", MyStrengthModifier));
            }
            if (useIntellectModifier) {
                abilitiesList.Add(string.Format(" +{0} Intellect", MyIntellectModifier));
            }
            if (useAgilityModifier) {
                abilitiesList.Add(string.Format(" +{0} Agility", MyAgilityModifier));
            }

            if (onEquipAbility != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Equip: {0}</color>", onEquipAbility.MyName));
            }
            foreach (BaseAbility learnedAbility in MyLearnedAbilities) {
                abilitiesList.Add(string.Format("<color=green>Learn On Equip: {0}</color>", learnedAbility.MyName));
            }

            return base.GetSummary() + "\n" + string.Join("\n", abilitiesList);
        }
    }

    public enum EquipmentSlot { Helm, Chest, Legs, MainHand, OffHand, Feet, Hands, Shoulders }
    //public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }
}