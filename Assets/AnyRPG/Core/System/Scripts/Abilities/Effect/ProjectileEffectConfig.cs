using AnyRPG;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ProjectileEffectConfig : AbilityEffectConfig {

        [SerializeField]
        private ProjectileEffectProperties effectProperties = new ProjectileEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => effectProperties; }

        public override string Convert(Ability ability, string pathName) {

            string effectType = "Projectile";
            Debug.Log($"{effectType}EffectConfig.Convert({ability.resourceName})");
            ProjectileEffect newAbilityEffect = ScriptableObject.CreateInstance($"{effectType}Effect") as ProjectileEffect;
            newAbilityEffect.projectileEffectProperties = effectProperties;

            CopyResourceProperties(ability, newAbilityEffect, effectType);
            string newScriptableObjectName = ability.resourceName.Replace(" ", "") + $"{effectType}Effect";
            string scriptableObjectPath = pathName + "/" + newScriptableObjectName + "2.asset";
            Debug.Log($"New Asset Path: {scriptableObjectPath}");
            //AssetDatabase.CreateAsset(newAbilityEffect, scriptableObjectPath);


            return $"{ability.resourceName} {effectType}";
        }
    }
}