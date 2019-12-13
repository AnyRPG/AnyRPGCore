using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon", menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
    public class Weapon : Equipment {

        [SerializeField]
        protected bool useDamagePerSecond = true;

        [SerializeField]
        protected bool useManualDamagePerSecond;

        [SerializeField]
        protected float damagePerSecond;

        [SerializeField]
        protected string defaultAttackAnimationProfileName;

        //[SerializeField]
        protected AnimationProfile defaultAttackAnimationProfile;

        /*
        /// <summary>
        /// The ability to cast when the weapon hits a target
        /// </summary>
        [SerializeField]
        private InstantEffectAbility onHitAbility;
        */

        [SerializeField]
        private string onHitEffectName;

        //[SerializeField]
        private AbilityEffect onHitEffect;

        // the skill required to use this weapon
        [SerializeField]
        private string weaponSkill;

        [SerializeField]
        private AudioClip defaultHitSoundEffect;
        /*
        public InstantEffectAbility OnHitAbility {
            get {
                return onHitAbility;
            }
        }
        */

        public AnimationProfile MyDefaultAttackAnimationProfile { get => defaultAttackAnimationProfile; set => defaultAttackAnimationProfile = value; }
        public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }

        /*
        public override int MyDamageModifier {
            get {
                if (!useManualDamage) {
                    return (int)(base.MyDamageModifier * MyRealEquipmentSlotType.MyStatWeight);
                }
                return base.MyDamageModifier;
            }
            set => base.MyDamageModifier = value;
        }
        */

        public string MyWeaponSkill { get => weaponSkill; set => weaponSkill = value; }
        public bool MyUseManualDamagePerSecond { get => useManualDamagePerSecond; set => useManualDamagePerSecond = value; }
        public bool MyUseDamagePerSecond { get => useDamagePerSecond; set => useDamagePerSecond = value; }
        public AbilityEffect MyOnHitEffect { get => onHitEffect; set => onHitEffect = value; }

        public float MyDamagePerSecond () {
            if (!MyUseDamagePerSecond) {
                return 0f;
            }
            if (useManualDamagePerSecond) {
                return damagePerSecond;
            }
            return Mathf.Ceil(Mathf.Clamp(
                (float)MyItemLevel * (SystemConfigurationManager.MyInstance.MyWeaponDPSBudgetPerLevel * GetItemQualityNumber() * MyRealEquipmentSlotType.MyStatWeight),
                0f,
                Mathf.Infinity
                ));
        }
        /*
        public override int MyIntellectModifier(int currentLevel, BaseCharacter baseCharacter) {
            if (!useManualIntellect) {
                return (int)(base.MyIntellectModifier(currentLevel, baseCharacter) * MyRealEquipmentSlotType.MyStatWeight);
            }
            return base.MyIntellectModifier(currentLevel, baseCharacter);
        }

        public override int MyStaminaModifier(int currentLevel, BaseCharacter baseCharacter) {
            if (!useManualStamina) {
                return (int)(base.MyStaminaModifier(currentLevel, baseCharacter) * MyRealEquipmentSlotType.MyStatWeight);
            }
            return base.MyStaminaModifier(currentLevel, baseCharacter);
        }

        public override int MyStrengthModifier(int currentLevel, BaseCharacter baseCharacter) {
            if (!useManualStrength) {
                return (int)(base.MyStrengthModifier(currentLevel, baseCharacter) * MyRealEquipmentSlotType.MyStatWeight);
            }
            return base.MyStrengthModifier(currentLevel, baseCharacter);
        }

        public override int MyAgilityModifier(int currentLevel, BaseCharacter baseCharacter) {
            if (!useManualAgility) {
                return (int)(base.MyAgilityModifier(currentLevel, baseCharacter) * MyRealEquipmentSlotType.MyStatWeight);
            }
            return base.MyAgilityModifier(currentLevel, baseCharacter);
        }
        */

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            if (useDamagePerSecond) {
                abilitiesList.Add(string.Format("Damage Per Second: {0}", MyDamagePerSecond()));
            }
            if (onHitEffect != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", onHitEffect.MyName));
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
                abilitiesString += string.Format("\n<color={0}>Required Skill: {1}</color>", colorString, weaponSkill);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<string> GetAllowedCharacterClasses() {
            List<string> returnValue = new List<string>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.MyWeaponSkillList != null && characterClass.MyWeaponSkillList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.MyWeaponSkillList.Contains(weaponSkill)) {
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

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            onHitEffect = null;
            if (onHitEffectName != null) {
                AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(onHitEffectName);
                if (abilityEffect != null) {
                    onHitEffect = abilityEffect;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability effect : " + onHitEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            defaultAttackAnimationProfile = null;
            if (defaultAttackAnimationProfileName != null) {
                AnimationProfile animationProfile = SystemAnimationProfileManager.MyInstance.GetResource(defaultAttackAnimationProfileName);
                if (animationProfile != null) {
                    defaultAttackAnimationProfile = animationProfile;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + defaultAttackAnimationProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}