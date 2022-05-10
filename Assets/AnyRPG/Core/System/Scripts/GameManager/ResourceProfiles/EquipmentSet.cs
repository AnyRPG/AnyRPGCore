using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Equipment Set", menuName = "AnyRPG/EquipmentSet")]
    [System.Serializable]
    public class EquipmentSet : DescribableResource {

        [Header("Equipment Set")]

        [Tooltip("The names of the equipment that belong to this set")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Equipment))]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Set Bonuses")]

        [Tooltip("the name of the trait should be associated with the list spot that matches the number of gear pieces required")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        private List<string> traitNames = new List<string>();

        private List<StatusEffectProperties> traitList = new List<StatusEffectProperties>();

        public List<StatusEffectProperties> TraitList { get => traitList; set => traitList = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public List<string> EquipmentNames { get => equipmentNames; set => equipmentNames = value; }
        public List<string> TraitNames { get => traitNames; set => traitNames = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            equipmentList = new List<Equipment>();
            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = systemDataFactory.GetResource<Item>(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            traitList = new List<StatusEffectProperties>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    StatusEffect tmpStatusEffect = null;
                    if (traitName == string.Empty) {
                        traitList.Add(null);
                    } else {
                        tmpStatusEffect = systemDataFactory.GetResource<AbilityEffect>(traitName) as StatusEffect;
                        if (tmpStatusEffect != null) {
                            traitList.Add(tmpStatusEffect.AbilityEffectProperties as StatusEffectProperties);
                        } else {
                            Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect : " + traitName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    }
                }
            }

        }
    }

}