using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Equipment Model Profile", menuName = "AnyRPG/Inventory/Equipment/Equipment Model Profile")]
    public class EquipmentModelProfile : DescribableResource {

        [Tooltip("Inline equipment model definitions.")]
        [SerializeReference]
        [SerializeReferenceButton]
        private List<EquipmentModel> equipmentModels = new List<EquipmentModel>();

        [SerializeField]
        private EquipmentModelProperties equipmentModelProperties = new EquipmentModelProperties();

        public List<EquipmentModel> EquipmentModels { get => equipmentModels; set => equipmentModels = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            foreach (EquipmentModel equipmentModel in equipmentModels) {
                if (equipmentModel != null) {
                    equipmentModel.Configure(systemGameManager);
                    equipmentModel.SetupScriptableObjects(this);
                }
            }
        }

    }
}

