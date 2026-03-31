using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ActivatableObjectProps : InteractableOptionProps {

        [Header("Activatable Object")]

        /*
        [SerializeField]
        protected float despawnTimer = 5f;
        */

        [Tooltip("The gameObject that will be enabled or disabled ")]
        [SerializeField]
        private GameObject spawnObject = null;

        public override Sprite Icon { get => (systemConfigurationManager.LootableCharacterInteractionPanelImage != null ? systemConfigurationManager.LootableCharacterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.LootableCharacterNamePlateImage != null ? systemConfigurationManager.LootableCharacterNamePlateImage : base.NamePlateImage); }
        //public float DespawnTimer { get => despawnTimer; set => despawnTimer = value; }
        public GameObject SpawnObject { get => spawnObject; }

    }

}