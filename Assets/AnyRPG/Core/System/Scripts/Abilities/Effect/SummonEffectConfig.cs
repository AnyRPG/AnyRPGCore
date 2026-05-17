using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SummonEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private SummonEffectProperties effectProperties = new SummonEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Summon";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            SummonEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as SummonEffect;
            newAbilityEffect.summonEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}
