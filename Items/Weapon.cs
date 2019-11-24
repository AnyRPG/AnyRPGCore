using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon", menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
    public class Weapon : Equipment {

        [SerializeField]
        protected AnimationProfile defaultAttackAnimationProfile;

        /// <summary>
        /// The ability to cast when the weapon hits a target
        /// </summary>
        [SerializeField]
        private InstantEffectAbility onHitAbility;

        [SerializeField]
        private AnyRPGWeaponAffinity weaponAffinity;

        // the skill required to use this weapon
        [SerializeField]
        private string weaponSkill;

        [SerializeField]
        private AudioClip defaultHitSoundEffect;

        public InstantEffectAbility OnHitAbility {
            get {
                return onHitAbility;
            }
        }

        public AnimationProfile MyDefaultAttackAnimationProfile { get => defaultAttackAnimationProfile; set => defaultAttackAnimationProfile = value; }
        public AnyRPGWeaponAffinity MyWeaponAffinity { get => weaponAffinity; set => weaponAffinity = value; }
        public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }

        public override int MyDamageModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyDamageModifier * 2;
                }
                return base.MyDamageModifier;
            }
            set => base.MyDamageModifier = value;
        }
        public override int MyArmorModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyArmorModifier * 2;
                }
                return base.MyArmorModifier;
            }
            set => base.MyArmorModifier = value;
        }

        public string MyWeaponSkill { get => weaponSkill; set => weaponSkill = value; }

        public override int MyIntellectModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyIntellectModifier(currentLevel, baseCharacter) * 2;
                }
                return base.MyIntellectModifier(currentLevel, baseCharacter);
        }

        public override int MyStaminaModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyStaminaModifier(currentLevel, baseCharacter) * 2;
                }
                return base.MyStaminaModifier(currentLevel, baseCharacter);
        }

        public override int MyStrengthModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyStrengthModifier(currentLevel, baseCharacter) * 2;
                }
                return base.MyStrengthModifier(currentLevel, baseCharacter);
        }

        public override int MyAgilityModifier(int currentLevel, BaseCharacter baseCharacter) {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyAgilityModifier(currentLevel, baseCharacter) * 2;
                }
                return base.MyAgilityModifier(currentLevel, baseCharacter);
        }

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            if (onHitAbility != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", onHitAbility.MyName));
            }
            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            List<string> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses.Count > 0) {
                string colorString = "red";
                /*
                CharacterClass playerClass = null;
                if (PlayerManager.MyInstance.MyCharacter.MyCharacterClassName != null && (PlayerManager.MyInstance.MyCharacter.MyCharacterClassName != string.Empty) {
                    playerClass = SystemCharacterClassManager.MyInstance.GetResource(PlayerManager.MyInstance.MyCharacter.MyCharacterClassName);
                }
                */
                if (allowedCharacterClasses.Contains(PlayerManager.MyInstance.MyCharacter.MyCharacterClassName)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>Required Skill: {1}</color>", colorString, weaponAffinity);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<string> GetAllowedCharacterClasses() {
            List<string> returnValue = new List<string>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.MyWeaponAffinityList != null && characterClass.MyWeaponAffinityList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.MyWeaponAffinityList.Contains(weaponAffinity)) {
                        returnValue.Add(characterClass.MyName);
                    }
                }
            }
            return returnValue;
        }

        public override bool CanEquip(BaseCharacter baseCharacter) {
            bool returnValue = base.CanEquip(baseCharacter);
            if (returnValue == false) {
                return false;
            }
            List<string> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.MyCharacterClassName)) {
                MessageFeedManager.MyInstance.WriteMessage("You do not have the right weapon skill to equip " + MyName);
                return false;
            }
            return true;
        }


    }

}