using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class LootableNode : InteractableOption {

        [SerializeField]
        protected List<string> lootTableNames = new List<string>();

        [SerializeField]
        protected float spawnTimer = 5f;


    }

}