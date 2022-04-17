using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class ResourceInputAmountNode {

        public ResourceInputAmountNode(string resourceName, float resourceAmount) {
            this.resourceName = resourceName;
            amount = resourceAmount;
        }

        public string resourceName = string.Empty;

        public float amount = 0f;

    }

}