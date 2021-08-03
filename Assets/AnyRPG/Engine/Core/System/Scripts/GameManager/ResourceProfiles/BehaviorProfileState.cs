using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    public class BehaviorProfileState {

        private Dictionary<BehaviorNode, BehaviorNodeState> behaviorNodeStates = new Dictionary<BehaviorNode, BehaviorNodeState>();

        public Dictionary<BehaviorNode, BehaviorNodeState> BehaviorNodeStates { get => behaviorNodeStates; set => behaviorNodeStates = value; }

        public BehaviorProfileState(BehaviorProfile behaviorProfile) {
            foreach (BehaviorNode behaviorNode in behaviorProfile.BehaviorNodes) {
                behaviorNodeStates.Add(behaviorNode, new BehaviorNodeState());
            }
        }
    }

}