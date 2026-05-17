using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class CapturePetEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private CapturePetEffectProperties effectProperties = new CapturePetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "CapturePet";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            CapturePetEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as CapturePetEffect;
            newAbilityEffect.capturePetEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}