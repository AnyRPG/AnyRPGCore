using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon", menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
    public class Weapon : Equipment {

        [Header("Weapon Type")]

        [Tooltip("Weapon Type controls defaults for the weapon, and also the skill required to use this weapon")]
        [FormerlySerializedAs("weaponSkill")]
        [SerializeField]
        private string weaponType = string.Empty;

        private WeaponSkill weaponSkill;

        [Header("Weapon Effect Defaults")]

        [Tooltip("Ability effects to cast on the target when the weapon does damage from a standard (auto) attack")]
        [SerializeField]
        private List<string> defaultHitEffects = new List<string>();

        private List<AbilityEffect> defaultHitEffectList = new List<AbilityEffect>();

        [Tooltip("Ability effects to cast on the target when the weapon does damage from any attack, including standard (auto) attacks")]
        [SerializeField]
        private List<string> onHitEffects = new List<string>();

        private List<AbilityEffect> onHitEffectList = new List<AbilityEffect>();

        [Header("Animation and Sound Defaults")]

        [Tooltip("An animation profile that can overwrite default animations to match the weapon")]
        [SerializeField]
        protected string defaultAttackAnimationProfileName = string.Empty;

        protected AnimationProfile defaultAttackAnimationProfile = null;

        [Tooltip("An audio effect that can be used by any physical ability cast while this weapon is equippped")]
        [SerializeField]
        private string defaultHitAudioProfile = string.Empty;

        private AudioClip defaultHitSoundEffect;

        [Header("Ability Prefab Defaults")]

        [Tooltip("Disable this to manually specify ability objects in the list below")]
        [SerializeField]
        protected bool useWeaponTypeObjects = true;

        [Tooltip("Physical prefabs to attach to bones on the character unit when this weapon is being used during an attack.  This could be arrows, special spell or glow effects, etc")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityObjectList = new List<AbilityAttachmentNode>();

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

        public AnimationProfile DefaultAttackAnimationProfile { get => defaultAttackAnimationProfile; set => defaultAttackAnimationProfile = value; }
        public AudioClip DefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }

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

        public WeaponSkill WeaponSkill { get => weaponSkill; set => weaponSkill = value; }
        public bool UseManualDamagePerSecond { get => useManualDamagePerSecond; set => useManualDamagePerSecond = value; }
        public bool UseDamagePerSecond { get => useDamagePerSecond; set => useDamagePerSecond = value; }
        public List<AbilityEffect> DefaultHitEffectList { get => defaultHitEffectList; set => defaultHitEffectList = value; }
        public List<AbilityEffect> OnHitEffectList { get => onHitEffectList; set => onHitEffectList = value; }
        public List<AbilityAttachmentNode> AbilityObjectList {
            get {
                if (useWeaponTypeObjects && weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.AbilityObjectList;
                }
                return abilityObjectList;
            }
        }

        public float GetDamagePerSecond(int characterLevel) {
            if (!UseDamagePerSecond) {
                return 0f;
            }
            if (useManualDamagePerSecond) {
                return damagePerSecond;
            }
            return Mathf.Ceil(Mathf.Clamp(
                (float)GetItemLevel(characterLevel) * (SystemConfigurationManager.MyInstance.MyWeaponDPSBudgetPerLevel * GetItemQualityNumber() * EquipmentSlotType.MyStatWeight),
                0f,
                Mathf.Infinity
                ));
        }

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            if (useDamagePerSecond) {
                abilitiesList.Add(string.Format("Damage Per Second: {0}", GetDamagePerSecond(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level)));
            }
            if (onHitEffectList != null) {
                foreach (AbilityEffect abilityEffect in onHitEffectList) {
                    abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", abilityEffect.DisplayName));
                }
            }
            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            // TODO: this code does not yet account for all the new capabilityProviders and will show red if something like faction provides the capability
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
                abilitiesString += string.Format("\n<color={0}>Required Skill: {1}</color>", colorString, weaponSkill.DisplayName);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<CharacterClass> GetAllowedCharacterClasses() {
            List<CharacterClass> returnValue = new List<CharacterClass>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter).WeaponSkillList != null && characterClass.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter).WeaponSkillList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter).WeaponSkillList.Contains(weaponSkill)) {
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
                MessageFeedManager.MyInstance.WriteMessage("You do not have the right weapon skill to equip " + DisplayName);
                return false;
            }
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (onHitEffects != null) {
                foreach (string onHitEffectName in onHitEffects) {
                    if (onHitEffectName != null && onHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(onHitEffectName);
                        if (abilityEffect != null) {
                            onHitEffectList.Add(abilityEffect);
                        } else {
                            Debug.LogError("Weapon.SetupScriptableObjects(): Could not find ability effect : " + onHitEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("Weapon.SetupScriptableObjects(): null or empty on hit effect found while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (defaultHitEffects != null) {
                foreach (string defaultHitEffectName in defaultHitEffects) {
                    if (defaultHitEffectName != null && defaultHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(defaultHitEffectName);
                        if (abilityEffect != null) {
                            defaultHitEffectList.Add(abilityEffect);
                        } else {
                            Debug.LogError("Weapon.SetupScriptableObjects(): Could not find ability effect : " + defaultHitEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("Weapon.SetupScriptableObjects(): null or empty default hit effect found while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


            defaultAttackAnimationProfile = null;
            if (defaultAttackAnimationProfileName != null && defaultAttackAnimationProfileName != string.Empty) {
                AnimationProfile animationProfile = SystemAnimationProfileManager.MyInstance.GetResource(defaultAttackAnimationProfileName);
                if (animationProfile != null) {
                    defaultAttackAnimationProfile = animationProfile;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find attack animation profile : " + defaultAttackAnimationProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (defaultHitAudioProfile != null && defaultHitAudioProfile != string.Empty) {
                defaultHitSoundEffect = null;
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(defaultHitAudioProfile);
                if (audioProfile != null) {
                    defaultHitSoundEffect = audioProfile.AudioClip;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find audio profile : " + defaultHitAudioProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (weaponType != null && weaponType != string.Empty) {
                WeaponSkill tmpWeaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponType);
                if (tmpWeaponSkill != null) {
                    weaponSkill = tmpWeaponSkill;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find weapon skill : " + weaponType + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (abilityObjectList != null) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityObjectList) {
                    if (abilityAttachmentNode != null) {
                        abilityAttachmentNode.SetupScriptableObjects();
                    }
                }
            }

        }

    }

}