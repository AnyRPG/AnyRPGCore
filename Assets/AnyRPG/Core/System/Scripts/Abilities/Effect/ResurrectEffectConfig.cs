using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ResurrectEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private ResurrectEffectProperties effectProperties = new ResurrectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Resurrect";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            ResurrectEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as ResurrectEffect;
            newAbilityEffect.resurrectEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}