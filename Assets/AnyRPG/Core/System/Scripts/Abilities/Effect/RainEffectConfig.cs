using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class RainEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private RainEffectProperties effectProperties = new RainEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Rain";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            RainEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as RainEffect;
            newAbilityEffect.rainEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}