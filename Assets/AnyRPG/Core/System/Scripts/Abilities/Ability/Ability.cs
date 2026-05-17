using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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