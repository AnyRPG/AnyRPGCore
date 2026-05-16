using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class EquipmentModelProperties : ConfiguredClass {

        [Tooltip("The name of the equipment this model profile will be added to at run-time")]
        [ResourceSelector(resourceType = typeof(Equipment))]
        [SerializeField]
        private string applyToEquipmentName = string.Empty;

        [Tooltip("Inline equipment model definitions.")]
        [SerializeReference]
        [SerializeReferenceButton]
        private List<EquipmentModel> equipmentModels = new List<EquipmentModel>();

        public List<EquipmentModel> EquipmentModels { get => equipmentModels; set => equipmentModels = value; }
        public string ApplyToEquipmentName { get => applyToEquipmentName; set => applyToEquipmentName = value; }

        /*
        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            foreach (EquipmentModel equipmentModel in equipmentModels) {
                if (equipmentModel != null) {
                    equipmentModel.Configure(systemGameManager);
                    equipmentModel.SetupScriptableObjects(this);
                }
            }
        }
        */
    }
}

