using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AOEEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private AOEEffectProperties effectProperties = new AOEEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "AOE";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            AOEEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as AOEEffect;
            newAbilityEffect.aoeEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}