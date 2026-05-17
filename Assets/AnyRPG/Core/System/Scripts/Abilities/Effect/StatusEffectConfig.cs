using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class StatusEffectConfig : AbilityEffectConfig {
        // abstract for now because we can't have inline status effects due to the need to do a database lookup to re-apply saved effects on game load
        // making it abstract will prevent it from showing up on dropdown lists

        [SerializeField]
        private StatusEffectProperties effectProperties = new StatusEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {
            /*
            string effectType = "Status";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            StatusEffectBase newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as StatusEffectBase;
            newAbilityEffect.statusEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
            */
            return string.Empty;
        }

    }
}
