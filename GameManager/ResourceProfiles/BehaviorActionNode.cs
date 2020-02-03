using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BehaviorActionNode {

        [SerializeField]
        private string behaviorMethod;

        
        [SerializeField]
        private string behaviorParameter;

        public string MyBehaviorMethod { get => behaviorMethod; set => behaviorMethod = value; }
        public string MyBehaviorParameter { get => behaviorParameter; set => behaviorParameter = value; }
    }

}