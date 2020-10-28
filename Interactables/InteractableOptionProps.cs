using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [System.Serializable]
    public abstract class InteractableOptionProps {

        [Header("Interaction Panel")]

        [Tooltip("The text to display for the clickable option in the interaction panel if this object has multiple interaction options.")]
        [SerializeField]
        protected string interactionPanelTitle;

        [Tooltip("The image to display beside the text for the clickable option in the interaction panel if this object has multiple interaction options.")]
        [SerializeField]
        protected Sprite interactionPanelImage;

        [Header("Nameplate")]

        [Tooltip("If there is no system option set for the nameplate image of this interactable option type, this will be used instead.")]
        [SerializeField]
        protected Sprite namePlateImage;

        [Header("Interaction")]

        [Tooltip("These game conditions must be satisfied to be able to interact with this option.")]
        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        public virtual string InteractionPanelTitle { get => interactionPanelTitle; set => interactionPanelTitle = value; }
        public virtual Sprite Icon { get => interactionPanelImage; }
        public virtual Sprite NamePlateImage { get => namePlateImage; }

        public List<PrerequisiteConditions> PrerequisiteConditions { get => prerequisiteConditions; set => prerequisiteConditions = value; }


    }

}