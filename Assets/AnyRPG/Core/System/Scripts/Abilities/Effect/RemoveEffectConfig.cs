using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class RemoveEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private RemoveEffectProperties effectProperties = new RemoveEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Remove";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            RemoveEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as RemoveEffect;
            newAbilityEffect.removeEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}