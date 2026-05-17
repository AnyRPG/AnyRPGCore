using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class KnockBackEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private KnockBackEffectProperties effectProperties = new KnockBackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "KnockBack";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            KnockBackEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as KnockBackEffect;
            newAbilityEffect.knockBackEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}
