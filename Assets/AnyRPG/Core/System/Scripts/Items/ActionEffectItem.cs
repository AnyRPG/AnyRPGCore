using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "ActionEffectItem", menuName = "AnyRPG/Inventory/Items/ActionEffectItem", order = 1)]
    public class ActionEffectItem : ActionItem {

        [Header("Effect")]

        [Tooltip("The resources to affect, and the amounts of the effects.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private string effectName = string.Empty;

        private AbilityEffectProperties abilityEffectProperties = null;

        public AbilityEffectProperties AbilityEffectProperties { get => abilityEffectProperties; set => abilityEffectProperties = value; }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemInstanceId, Item item, ItemQuality usedItemQuality) {
            if ((item is ActionEffectItem) == false) {
                return null;
            }
            return new InstantiatedActionEffectItem(systemGameManager, itemInstanceId, item as ActionEffectItem, usedItemQuality);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (effectName != null && effectName != string.Empty) {
                AbilityEffect tmpEffect = systemDataFactory.GetResource<AbilityEffect>(effectName);
                if (tmpEffect != null) {
                    abilityEffectProperties = tmpEffect.AbilityEffectProperties;
                } else {
                    Debug.LogError($"PowerResourcePotion.SetupScriptableObjects(): Could not find ability effect : {effectName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

        }
    }

}