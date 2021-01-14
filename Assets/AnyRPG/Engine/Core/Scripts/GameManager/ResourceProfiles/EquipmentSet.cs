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
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Set Bonuses")]

        [Tooltip("the name of the trait should be associated with the list spot that matches the number of gear pieces required")]
        [SerializeField]
        private List<string> traitNames = new List<string>();

        private List<StatusEffect> traitList = new List<StatusEffect>();

        public List<StatusEffect> MyTraitList { get => traitList; set => traitList = value; }
        public List<Equipment> MyEquipmentList { get => equipmentList; set => equipmentList = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            equipmentList = new List<Equipment>();
            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = SystemItemManager.MyInstance.GetResource(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            traitList = new List<StatusEffect>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    StatusEffect tmpStatusEffect = null;
                    if (traitName == string.Empty) {
                        traitList.Add(null);
                    } else {
                        tmpStatusEffect = SystemAbilityEffectManager.MyInstance.GetResource(traitName) as StatusEffect;
                        if (tmpStatusEffect != null) {
                            traitList.Add(tmpStatusEffect);
                        } else {
                            Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find status effect : " + traitName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    }
                }
            }

        }
    }

}