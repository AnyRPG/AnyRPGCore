using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BehaviorActionNode {

        [SerializeField]
        private string behaviorMethod = string.Empty;

        
        [SerializeField]
        private string behaviorParameter = string.Empty;

        public string BehaviorMethod { get => behaviorMethod; set => behaviorMethod = value; }
        public string BehaviorParameter { get => behaviorParameter; set => behaviorParameter = value; }
    }

}