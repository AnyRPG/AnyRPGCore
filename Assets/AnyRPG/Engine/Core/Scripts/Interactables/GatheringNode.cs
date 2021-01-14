using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNode : LootableNode {

        [SerializeField]
        private GatheringNodeProps gatheringNodeProps = new GatheringNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => gatheringNodeProps; }
    }

}