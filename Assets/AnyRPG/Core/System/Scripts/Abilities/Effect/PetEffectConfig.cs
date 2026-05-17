using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PetEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private PetEffectProperties effectProperties = new PetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Pet";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            PetEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as PetEffect;
            newAbilityEffect.petEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}
