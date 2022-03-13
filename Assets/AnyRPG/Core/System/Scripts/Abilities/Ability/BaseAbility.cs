using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    //[CreateAssetMenu(fileName = "NewAbility",menuName = "AnyRPG/Abilities/Ability")]
    public abstract class BaseAbility : DescribableResource /*, IUseable, IMoveable, ILearnable*/ {

        //public event System.Action OnAbilityLearn = delegate { };
        //public event System.Action OnAbilityUsed = delegate { };


        public virtual BaseAbilityProperties AbilityProperties { get => null; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            AbilityProperties.SetupScriptableObjects(systemGameManager, this);

        }

    }


}