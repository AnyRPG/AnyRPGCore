using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MoveableObject : InteractableOption {

        [SerializeField]
        private MoveableObjectProps moveableObjectProps = new MoveableObjectProps();

        public override InteractableOptionProps InteractableOptionProps { get => moveableObjectProps; }
    }

}