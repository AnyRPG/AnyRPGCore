using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon", menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
    public class Weapon : Equipment {

        [Header("Weapon Type")]

        [Tooltip("If true, the character must have a weapon skill that matches the weapon type to equip or use this weapon")]
        [SerializeField]
        private bool requireWeaponSkill = false;

        [Tooltip("Weapon Type controls defaults for the weapon, and also the skill required to use this weapon")]
        [FormerlySerializedAs("weaponSkill")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(WeaponSkill))]
        private string weaponType = string.Empty;

        private WeaponSkill weaponSkill;

        [Header("Weapon Effect Defaults")]

        [Tooltip("Ability effects to cast on the target when the weapon does damage from a standard (auto) attack")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> defaultHitEffects = new List<string>();

        private List<AbilityEffectProperties> defaultHitEffectList = new List<AbilityEffectProperties>();

        [Tooltip("Ability effects to cast on the target when the weapon does damage from any attack, including standard (auto) attacks")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> onHitEffects = new List<string>();

        private List<AbilityEffectProperties> onHitEffectList = new List<AbilityEffectProperties>();

        [Header("Animation and Sound Defaults")]

        [Tooltip("An animation profile that can overwrite default animations to match the weapon")]
        [FormerlySerializedAs("defaultAttackAnimationProfileName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        protected string animationProfileName = string.Empty;

        protected AnimationProfile animationProfile = null;

        [Tooltip("An audio effect that can be used by any physical ability cast while this weapon is equippped")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string defaultHitAudioProfile = string.Empty;

        private List<AudioClip> defaultHitSoundEffects = new List<AudioClip>();

        [Header("Ability Prefab Defaults")]

        [Tooltip("Disable this to manually specify ability objects in the list below")]
        [SerializeField]
        protected bool useWeaponTypeObjects = true;

        [Tooltip("Physical prefabs to attach to bones on the character unit when this weapon is being animated during an attack.  This could be arrows, special spell or glow effects, etc")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityAnimationObjectList = new List<AbilityAttachmentNode>();

        [Tooltip("Physical prefabs to use when this weapon is being used after the animation phase during an attack.  This could be arrows, special spell or glow effects, etc")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityObjectList = new List<AbilityAttachmentNode>();

        [Header("Damage")]

        [Tooltip("Automatic damage per second is added, based on the item level, item quality, and Weapon DPS Per Second setting the Game Manager")]
        [FormerlySerializedAs("useDamagePerSecond")]
        [SerializeField]
        protected bool addScaledDamagePerSecond = true;

        [Tooltip("Base Damage Per Second, unscaled")]
        [FormerlySerializedAs("damagePerSecond")]
        [SerializeField]
        protected float baseDamagePerSecond = 0f;

        public AnimationProfile AnimationProfile {
            get {
                if (animationProfile != null) {
                    return animationProfile;
                }
                if (weaponSkill != null && weaponSkill.WeaponSkillProps.AnimationProfile != null) {
                    return weaponSkill.WeaponSkillProps.AnimationProfile;
                }
                return null;
            }
        }
        public List<AudioClip> DefaultHitSoundEffects {
            get {
                if (defaultHitSoundEffects != null && defaultHitSoundEffects.Count > 0) {
                    return defaultHitSoundEffects;
                }
                if (weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.DefaultHitSoundEffects;
                }
                return new List<AudioClip>();
            }
        }

        public WeaponSkill WeaponSkill { get => weaponSkill; set => weaponSkill = value; }
        public bool AddScaledDamagePerSecond { get => addScaledDamagePerSecond; set => addScaledDamagePerSecond = value; }
        public List<AbilityEffectProperties> DefaultHitEffectList {
            get {
                if (defaultHitEffectList != null && defaultHitEffectList.Count > 0) {
                    return defaultHitEffectList;
                }
                if (weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.DefaultHitEffectList;
                }
                return new List<AbilityEffectProperties>();
            }
        }
        public List<AbilityEffectProperties> OnHitEffectList {
            get {
                if (onHitEffectList != null && onHitEffectList.Count > 0) {
                    return onHitEffectList;
                }
                if (weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.OnHitEffectList;
                }
                return new List<AbilityEffectProperties>();
            }
        }
        public List<AbilityAttachmentNode> AbilityAnimationObjectList {
            get {
                if (useWeaponTypeObjects == true && weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.AbilityAnimationObjectList;
                }
                return abilityAnimationObjectList;
            }
        }

        public List<AbilityAttachmentNode> AbilityObjectList {
            get {
                if (useWeaponTypeObjects && weaponSkill != null) {
                    return weaponSkill.WeaponSkillProps.AbilityObjectList;
                }
                return abilityObjectList;
            }
        }

        public bool RequireWeaponSkill { get => requireWeaponSkill; set => requireWeaponSkill = value; }
        public string WeaponType { get => weaponType; set => weaponType = value; }

        public float GetDamagePerSecond(int characterLevel) {
            return GetDamagePerSecond(characterLevel, realItemQuality);
        }

        public float GetDamagePerSecond(int characterLevel, ItemQuality usedItemQuality) {

            if (addScaledDamagePerSecond) {
                return baseDamagePerSecond + Mathf.Ceil(Mathf.Clamp(
                    (float)GetItemLevel(characterLevel) * (systemConfigurationManager.WeaponDPSBudgetPerLevel * GetItemQualityNumber(usedItemQuality) * EquipmentSlotType.StatWeight),
                    0f,
                    Mathf.Infinity
                    ));
            }
            return baseDamagePerSecond;
        }

        public override string GetSummary(ItemQuality usedItemQuality) {

            List<string> abilitiesList = new List<string>();

            if (addScaledDamagePerSecond) {
                abilitiesList.Add(string.Format("Damage Per Second: {0}", GetDamagePerSecond(playerManager.MyCharacter.CharacterStats.Level, usedItemQuality)));
            }
            if (onHitEffectList != null) {
                foreach (AbilityEffectProperties abilityEffect in onHitEffectList) {
                    abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", abilityEffect.DisplayName));
                }
            }
            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }

            if (weaponSkill != null && requireWeaponSkill == true) {
                string colorString = "white";
                if (!CanEquip(playerManager.ActiveCharacter)) {
                    colorString = "red";
                }
                abilitiesString += string.Format("\n<color={0}>Required Skill: {1}</color>", colorString, weaponSkill.DisplayName);
            }
            return base.GetSummary(usedItemQuality) + abilitiesString;
        }

        /*
        public override bool CanEquip(BaseCharacter baseCharacter) {
            bool returnValue = base.CanEquip(baseCharacter);
            if (returnValue == false) {
                return false;
            }
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.CharacterClass)) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("You do not have the right weapon skill to equip " + DisplayName);
                return false;
            }
            return true;
        }
        */

        public override void HandleEquip(CharacterCombat characterCombat, EquipmentSlotProfile equipmentSlotProfile) {
            base.HandleEquip(characterCombat, equipmentSlotProfile);

            if (OnHitEffectList != null && OnHitEffectList.Count > 0) {
                characterCombat.AddOnHitEffects(OnHitEffectList);
            }
            if (DefaultHitEffectList != null && DefaultHitEffectList.Count > 0) {
                characterCombat.AddDefaultHitEffects(DefaultHitEffectList);
            }
            if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                if (DefaultHitSoundEffects != null) {
                    characterCombat.AddDefaultHitSoundEffects(DefaultHitSoundEffects);
                }
            }

            characterCombat.SetAttackSpeed();
        }

        public override void HandleUnequip(CharacterCombat characterCombat, EquipmentSlotProfile equipmentSlotProfile) {
            base.HandleUnequip(characterCombat, equipmentSlotProfile);

            if (OnHitEffectList != null && OnHitEffectList.Count > 0) {
                foreach (AbilityEffectProperties abilityEffect in OnHitEffectList) {
                    characterCombat.RemoveOnHitEffect(abilityEffect);
                }
            }
            if (DefaultHitEffectList != null && DefaultHitEffectList.Count > 0) {
                foreach (AbilityEffectProperties abilityEffect in DefaultHitEffectList) {
                    characterCombat.RemoveDefaultHitEffect(abilityEffect);
                }
            }
            if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): clearing default hit effects");
                characterCombat.ClearDefaultHitSoundEffects();
            }
            characterCombat.SetAttackSpeed();
        }

        public override void HandleEquip(CharacterEquipmentManager characterEquipmentManager) {
            base.HandleEquip(characterEquipmentManager);

            // animation phase objects
            if (AbilityAnimationObjectList != null && AbilityAnimationObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in AbilityAnimationObjectList) {
                    characterEquipmentManager.AddWeaponAbilityAnimationObjects(abilityAttachmentNode);
                }
            }

            // attack phase objects
            if (AbilityObjectList != null && AbilityObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in AbilityObjectList) {
                    characterEquipmentManager.AddWeaponAbilityObjects(abilityAttachmentNode);
                }
            }

        }

        public override void HandleUnequip(CharacterEquipmentManager characterEquipmentManager) {
            base.HandleUnequip(characterEquipmentManager);

            // animation phase objects
            if (AbilityAnimationObjectList != null && AbilityAnimationObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in AbilityAnimationObjectList) {
                    characterEquipmentManager.RemoveWeaponAbilityAnimationObjects(abilityAttachmentNode);
                }
            }

            // attack phase objects
            if (AbilityObjectList != null && AbilityObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in AbilityObjectList) {
                    characterEquipmentManager.RemoveWeaponAbilityObjects(abilityAttachmentNode);
                }
            }

        }

        public override bool CapabilityConsumerSupported(ICapabilityConsumer capabilityConsumer) {
            //Debug.Log(DisplayName + ".Weapon.CapabilityConsumerSupported");
            return capabilityConsumer.CapabilityConsumerProcessor.IsWeaponSupported(this);
        }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (onHitEffects != null) {
                foreach (string onHitEffectName in onHitEffects) {
                    if (onHitEffectName != null && onHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(onHitEffectName);
                        if (abilityEffect != null) {
                            onHitEffectList.Add(abilityEffect.AbilityEffectProperties);
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
                        AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(defaultHitEffectName);
                        if (abilityEffect != null) {
                            defaultHitEffectList.Add(abilityEffect.AbilityEffectProperties);
                        } else {
                            Debug.LogError("Weapon.SetupScriptableObjects(): Could not find ability effect : " + defaultHitEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("Weapon.SetupScriptableObjects(): null or empty default hit effect found while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


            animationProfile = null;
            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = systemDataFactory.GetResource<AnimationProfile>(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find attack animation profile : " + animationProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (defaultHitAudioProfile != null && defaultHitAudioProfile != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(defaultHitAudioProfile);
                if (audioProfile != null) {
                    defaultHitSoundEffects = audioProfile.AudioClips;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find audio profile : " + defaultHitAudioProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (weaponType != null && weaponType != string.Empty) {
                WeaponSkill tmpWeaponSkill = systemDataFactory.GetResource<WeaponSkill>(weaponType);
                if (tmpWeaponSkill != null) {
                    weaponSkill = tmpWeaponSkill;
                } else {
                    Debug.LogError("Weapon.SetupScriptableObjects(): Could not find weapon skill : " + weaponType + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (abilityObjectList != null) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityObjectList) {
                    if (abilityAttachmentNode != null) {
                        abilityAttachmentNode.SetupScriptableObjects(DisplayName, systemGameManager);
                    }
                }
            }

        }

    }

}