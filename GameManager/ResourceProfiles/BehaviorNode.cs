using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BehaviorNode {

        // time in seconds to show this text
        [SerializeField]
        private float startTime;

        [SerializeField]
        private List<BehaviorActionNode> behaviorActionNodes = new List<BehaviorActionNode>();

        private bool completed = false;


        public float MyStartTime { get => startTime; set => startTime = value; }
        public List<BehaviorActionNode> MyBehaviorActionNodes { get => behaviorActionNodes; set => behaviorActionNodes = value; }
        public bool MyCompleted { get => completed; set => completed = value; }
    }

}