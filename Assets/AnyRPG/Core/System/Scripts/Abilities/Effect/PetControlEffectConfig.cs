using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PetControlEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private PetControlEffectProperties effectProperties = new PetControlEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "PetControl";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            PetControlEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as PetControlEffect;
            newAbilityEffect.petControlEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}
