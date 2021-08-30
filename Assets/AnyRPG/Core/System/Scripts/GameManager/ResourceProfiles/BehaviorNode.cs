using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BehaviorNode {

        // time in seconds to show this text
        [Tooltip("The time, in seconds, after the behavior starts playing to play this node.")]
        [SerializeField]
        private float startTime;

        [SerializeField]
        private List<BehaviorActionNode> behaviorActionNodes = new List<BehaviorActionNode>();

        public float MyStartTime { get => startTime; set => startTime = value; }
        public List<BehaviorActionNode> MyBehaviorActionNodes { get => behaviorActionNodes; set => behaviorActionNodes = value; }

        /// <summary>
        /// Reset the completion status
        /// </summary>
        public void ResetStatus(BehaviorNodeState behaviorNodeState) {
            behaviorNodeState.Completed = false;
        }
    }

}