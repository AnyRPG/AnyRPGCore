using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PrefabEquipmentModel : EquipmentModel {

        //[Header("Prefab Equipment Models")]

        [SerializeField]
        private PrefabEquipmentModelProperties properties = new PrefabEquipmentModelProperties();

        public PrefabEquipmentModelProperties Properties { get => properties; set => properties = value; }

        public override void SetupScriptableObjects(IDescribable describable) {
            base.SetupScriptableObjects(describable);

            properties.Configure(systemGameManager);
            properties.SetupScriptableObjects(describable);
        }

    }
}

