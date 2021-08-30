using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Prefab Profile", menuName = "AnyRPG/UnitPrefabProfile")]
    public class UnitPrefabProfile : DescribableResource {

        [SerializeField]
        private UnitPrefabProps unitPrefabProps = new UnitPrefabProps();

        public UnitPrefabProps UnitPrefabProps { get => unitPrefabProps; set => unitPrefabProps = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            unitPrefabProps.SetupScriptableObjects(systemDataFactory);
        }
    }


}