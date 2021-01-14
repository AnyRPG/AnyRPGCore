using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {

    [System.Serializable]
    public class WeaponSkillProps {

        [Header("Weapon Skill")]

        [Tooltip("this skill is considered to be in use by an unarmed character if set to true")]
        [SerializeField]
        private bool defaultWeaponSkill = false;

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
        protected string animationProfileName = string.Empty;

        private AnimationProfile animationProfile = null;

        [Tooltip("Audio effects that can be used by any physical ability cast while this weapon is equippped")]
        [SerializeField]
        private List<string> onHitAudioProfiles = new List<string>();

        private List<AudioClip> onHitSoundEffects = new List<AudioClip>();

        [Header("Ability Prefab Defaults")]

        [Tooltip("Physical prefabs to attach to bones on the character unit when this weapon is being used during an attack.  This could be arrows, special spell or glow effects, etc")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityObjectList = new List<AbilityAttachmentNode>();


        // properties
        public bool DefaultWeaponSkill { get => defaultWeaponSkill; set => defaultWeaponSkill = value; }
        public List<AbilityEffect> DefaultHitEffectList { get => defaultHitEffectList; set => defaultHitEffectList = value; }
        public List<AbilityEffect> OnHitEffectList { get => onHitEffectList; set => onHitEffectList = value; }
        public AnimationProfile AnimationProfile { get => animationProfile; set => animationProfile = value; }
        public List<AudioClip> DefaultHitSoundEffects { get => onHitSoundEffects; set => onHitSoundEffects = value; }
        public List<AbilityAttachmentNode> AbilityObjectList { get => abilityObjectList; set => abilityObjectList = value; }

        // methods
        public void SetupScriptableObjects() {

            if (onHitEffects != null) {
                foreach (string onHitEffectName in onHitEffects) {
                    if (onHitEffectName != null && onHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(onHitEffectName);
                        if (abilityEffect != null) {
                            onHitEffectList.Add(abilityEffect);
                        } else {
                            Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): Could not find ability effect : " + onHitEffectName + " while inititalizing.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): null or empty on hit effect found while inititalizing.  CHECK INSPECTOR");
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
                            Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): Could not find ability effect : " + defaultHitEffectName + " while inititalizing.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): null or empty default hit effect found while inititalizing.  CHECK INSPECTOR");
                    }
                }
            }

            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = SystemAnimationProfileManager.MyInstance.GetResource(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): Could not find attack animation profile : " + animationProfileName + " while inititalizing.  CHECK INSPECTOR");
                }
            }

            if (onHitAudioProfiles != null) {
                foreach (string defaultHitAudioProfile in onHitAudioProfiles) {
                    if (defaultHitAudioProfile != null && defaultHitAudioProfile != string.Empty) {
                        AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(defaultHitAudioProfile);
                        if (audioProfile != null) {
                            onHitSoundEffects.Add(audioProfile.AudioClip);
                        } else {
                            Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): Could not find audio profile : " + defaultHitAudioProfile + " while inititalizing.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("WeaponSkillProps.SetupScriptableObjects(): null or empty default audio hit profile found while inititalizing.  CHECK INSPECTOR");
                    }
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