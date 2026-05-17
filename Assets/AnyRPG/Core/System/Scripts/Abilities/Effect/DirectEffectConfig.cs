using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    // NOTE: DIRECTEFFECT WILL CAST TICK AND COMPLETE, BUT NEVER HIT.  HIT MUST BE CAST BY PROJECTILE, AOE, OR CHANNELED
    [System.Serializable]
    public class DirectEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private DirectEffectProperties effectProperties = new DirectEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Direct";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            DirectEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as DirectEffect;
            newAbilityEffect.directEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);

            return $"{ability.resourceName} {effectType}";
        }

    }
}