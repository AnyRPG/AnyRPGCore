using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Portal Config", menuName = "AnyRPG/Interactable/PortalConfig")]
    [System.Serializable]
    public class PortalConfig : InteractableOptionConfig {

        [Header("Location Override")]

        [Tooltip("If this is set, the player will spawn at the location of the object in the scene with this tag, instead of the default spawn location for the scene.")]
        [SerializeField]
        protected string locationTag = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyPortalInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyPortalNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyPortalNamePlateImage : base.NamePlateImage); }

        public virtual InteractableOption GetInteractableOption(Interactable interactable) {
            return null;
        }
    }

}