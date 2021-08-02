using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class LootableNodeProps : InteractableOptionProps {

        [Header("Lootable Node")]

        [SerializeField]
        protected List<string> lootTableNames = new List<string>();

        protected List<LootTable> lootTables = new List<LootTable>();

        [SerializeField]
        protected float spawnTimer = 5f;

        public float SpawnTimer { get => spawnTimer; set => spawnTimer = value; }
        public List<LootTable> LootTables { get => lootTables; set => lootTables = value; }

        /*
        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            return new LootableNodeComponent(interactable, this);
        }
        */

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            foreach (string lootTableName in lootTableNames) {
                LootTable lootTable = SystemDataFactory.Instance.GetResource<LootTable>(lootTableName);
                if (lootTable != null) {
                    lootTables.Add(lootTable);
                } else {
                    Debug.LogError("Could not find loot table " + lootTableName + " while initializing Loot Node");
                }
            }
        }
    }

}