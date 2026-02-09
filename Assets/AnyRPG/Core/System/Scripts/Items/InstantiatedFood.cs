using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AnyRPG {
    public class InstantiatedFood : InstantiatedCastableItem {

        private Food food = null;

        public InstantiatedFood(SystemGameManager systemGameManager, long instanceId, Food food, ItemQuality itemQuality) : base(systemGameManager, instanceId, food, itemQuality) {
            this.food = food;
        }

    }
}