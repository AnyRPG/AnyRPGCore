using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AttackEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private AttackEffectProperties effectProperties = new AttackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Attack";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            AttackEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as AttackEffect;
            newAbilityEffect.attackEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}
