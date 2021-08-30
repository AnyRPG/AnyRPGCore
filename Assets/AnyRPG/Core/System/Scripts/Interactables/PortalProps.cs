using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class PortalProps : InteractableOptionProps {

        [Header("Location Override")]

        [Tooltip("If this is set, the player will spawn at the location of the object in the scene with this tag, instead of the default spawn location for the scene.")]
        [SerializeField]
        protected string locationTag = string.Empty;

        [Tooltip("If true, the player will spawn at the Vector location set in the Spawn Location field below.")]
        [SerializeField]
        protected bool overrideSpawnLocation = false;

        [Tooltip("The world space position to spawn at. Only used if Override Spawn Location box is checked")]
        [SerializeField]
        protected Vector3 spawnLocation = Vector3.zero;

        [Tooltip("If true, the player will spawn facing the world space direction specified in the Spawn Forward Direction field")]
        [SerializeField]
        protected bool overrideSpawnDirection = false;

        [Tooltip("The world space forward direction to face when spawning.  Only used if Override Spawn Direction box is checked")]
        [SerializeField]
        protected Vector3 spawnForwardDirection = Vector3.zero;

        public override Sprite Icon { get => (systemConfigurationManager.PortalInteractionPanelImage != null ? systemConfigurationManager.PortalInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.PortalNamePlateImage != null ? systemConfigurationManager.PortalNamePlateImage : base.NamePlateImage); }
        public string LocationTag { get => locationTag; set => locationTag = value; }
        public bool OverrideSpawnLocation { get => overrideSpawnLocation; set => overrideSpawnLocation = value; }
        public Vector3 SpawnLocation { get => spawnLocation; set => spawnLocation = value; }
        public bool OverrideSpawnDirection { get => overrideSpawnDirection; set => overrideSpawnDirection = value; }
        public Vector3 SpawnForwardDirection { get => spawnForwardDirection; set => spawnForwardDirection = value; }


        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            return null;
        }
    }

}