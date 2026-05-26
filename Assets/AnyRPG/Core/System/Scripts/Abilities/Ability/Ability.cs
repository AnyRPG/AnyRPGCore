using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "NewAbility",menuName = "AnyRPG/Abilities/Ability")]
    public class Ability : DescribableResource /*, IUseable, IMoveable, ILearnable*/ {

        [SerializeField]
        public AbilityProperties abilityProperties = new AbilityProperties();
        
        public virtual AbilityProperties AbilityProperties { get => abilityProperties; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            AbilityProperties.SetupScriptableObjects(systemGameManager, this);

        }

    }


}