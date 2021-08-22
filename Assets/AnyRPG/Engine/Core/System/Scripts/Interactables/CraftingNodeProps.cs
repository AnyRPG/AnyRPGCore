using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CraftingNodeProps : InteractableOptionProps {

        [Tooltip("The ability to cast in order to craft with this node")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private string abilityName = string.Empty;

        private BaseAbility ability;


        // crafting nodes are special.  The image is based on what ability it supports
        public override Sprite Icon {
            get {
                return (Ability.Icon != null ? Ability.Icon : base.Icon);
            }
        }

        public override Sprite NamePlateImage {
            get {
                return (Ability.Icon != null ? Ability.Icon : base.NamePlateImage);
            }
        }

        public override string GetInteractionPanelTitle(int optionIndex = 0) {
            return (Ability != null ? Ability.DisplayName : base.GetInteractionPanelTitle(optionIndex));
        }

        public BaseAbility Ability { get => ability; set => ability = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            return new CraftingNodeComponent(interactable, this, systemGameManager);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (abilityName != null && abilityName != string.Empty) {
                BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("CraftingNodeComponent.SetupScriptableObjects(): COULD NOT FIND ABILITY " + abilityName + " while initializing ");
                }
            }
        }
    }

}