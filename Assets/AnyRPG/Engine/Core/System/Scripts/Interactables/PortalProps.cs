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

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.PortalInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.PortalInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.PortalNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.PortalNamePlateImage : base.NamePlateImage); }
        public string LocationTag { get => locationTag; set => locationTag = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            return null;
        }
    }

}