using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon", menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
    public class Weapon : Equipment {

        [Header("Damage")]

        [Tooltip("Automatic damage per second is based on the item level and item quality")]
        [SerializeField]
        protected bool useDamagePerSecond = true;

        [Tooltip("If true, manually specify the damage per second independent of item level and item quallity")]
        [SerializeField]
        protected bool useManualDamagePerSecond = false;

        [Tooltip("This value is used if manual damage per second is enabled")]
        [SerializeField]
        protected float damagePerSecond = 0f;

        [Header("Animation")]

        [Tooltip("An animation profile that can overwrite default animations to match the weapon")]
        [SerializeField]
        protected string defaultAttackAnimationProfileName = string.Empty;

        protected AnimationProfile defaultAttackAnimationProfile = null;

        [Header("On Hit")]

        [Tooltip("An ability effect to cast on the target when the weapon does damage")]
        [SerializeField]
        private string onHitEffectName = string.Empty;

        private AbilityEffect onHitEffect;

        [Tooltip("An audio effect that can be used by any physical ability cast while this weapon is equippped")]
        [SerializeField]
        private string defaultHitAudioProfile = string.Empty;

        private AudioClip defaultHitSoundEffect;

        [Header("Restrictions")]

        [Tooltip("the skill required to use this weapon")]
        [SerializeField]
        private string weaponSkill = string.Empty;

        private WeaponSkill realWeaponSkill;


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

        public WeaponSkill MyWeaponSkill { get => realWeaponSkill; set => realWeaponSkill = value; }
        public bool MyUseManualDamagePerSecond { get => useManualDamagePerSecond; set => useManualDamagePerSecond = value; }
        public bool MyUseDamagePerSecond { get => useDamagePerSecond; set => useDamagePerSecond = value; }
        public AbilityEffect MyOnHitEffect { get => onHitEffect; set => onHitEffect = value; }

        public float GetDamagePerSecond(int characterLevel) {
            if (!MyUseDamagePerSecond) {
                return 0f;
            }
            if (useManualDamagePerSecond) {
                return damagePerSecond;
            }
            return Mathf.Ceil(Mathf.Clamp(
                (float)GetItemLevel(characterLevel) * (SystemConfigurationManager.MyInstance.MyWeaponDPSBudgetPerLevel * GetItemQualityNumber() * MyEquipmentSlotType.MyStatWeight),
                0f,
                Mathf.Infinity
                ));
        }

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            if (useDamagePerSecond) {
                abilitiesList.Add(string.Format("Damage Per Second: {0}", GetDamagePerSecond(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level)));
            }
            if (onHitEffect != null) {
                abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", onHitEffect.MyDisplayName));
            }
            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses.Count > 0) {
                string colorString = "red";
                /*
                CharacterClass playerClass = null;
                if (PlayerManager.MyInstance.MyCharacter.MyCharacterClassName != null && (PlayerManager.MyInstance.MyCharacter.MyCharacterClassName != string.Empty) {
                    playerClass = SystemCharacterClassManager.MyInstance.GetResource(PlayerManager.MyInstance.MyCharacter.MyCharacterClassName);
                }
                */
                if (allowedCharacterClasses.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>Required Skill: {1}</color>", colorString, realWeaponSkill.MyDisplayName);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<CharacterClass> GetAllowedCharacterClasses() {
            List<CharacterClass> returnValue = new List<CharacterClass>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.MyWeaponSkillList != null && characterClass.MyWeaponSkillList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.MyWeaponSkillList.Contains(realWeaponSkill)) {
                        returnValue.Add(characterClass);
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
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.CharacterClass)) {
                MessageFeedManager.MyInstance.WriteMessage("You do not have the right weapon skill to equip " + MyDisplayName);
                return false;
            }
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            onHitEffect = null;
            if (onHitEffectName != null && onHitEffectName != string.Empty) {
                AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(onHitEffectName);
                if (abilityEffect != null) {
                    onHitEffect = abilityEffect;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability effect : " + onHitEffectName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            defaultAttackAnimationProfile = null;
            if (defaultAttackAnimationProfileName != null && defaultAttackAnimationProfileName != string.Empty) {
                AnimationProfile animationProfile = SystemAnimationProfileManager.MyInstance.GetResource(defaultAttackAnimationProfileName);
                if (animationProfile != null) {
                    defaultAttackAnimationProfile = animationProfile;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find attack animation profile : " + defaultAttackAnimationProfileName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (defaultHitAudioProfile != null && defaultHitAudioProfile != string.Empty) {
                defaultHitSoundEffect = null;
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(defaultHitAudioProfile);
                if (audioProfile != null) {
                    defaultHitSoundEffect = audioProfile.MyAudioClip;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find audio profile : " + defaultHitAudioProfile + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            realWeaponSkill = null;
            if (weaponSkill != null && weaponSkill != string.Empty) {
                WeaponSkill tmpWeaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkill);
                if (tmpWeaponSkill != null) {
                    realWeaponSkill = tmpWeaponSkill;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find weapon skill : " + weaponSkill + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}