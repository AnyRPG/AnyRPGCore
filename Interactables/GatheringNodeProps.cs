using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class GatheringNodeProps : LootableNodeProps {

        [Header("Gathering Node")]

        [Tooltip("The ability to cast in order to gather from this node")]
        [SerializeField]
        private string abilityName = string.Empty;

        private GatherAbility baseAbility = null;

        // gathering nodes are special.  The image is based on what ability it supports
        public override Sprite Icon {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.Icon);
            }
        }

        public override Sprite NamePlateImage {
            get {
                return (BaseAbility.Icon != null ? BaseAbility.Icon : base.NamePlateImage);
            }
        }
        public override string InteractionPanelTitle { get => (BaseAbility != null ? BaseAbility.DisplayName : base.InteractionPanelTitle); }

        public GatherAbility BaseAbility { get => baseAbility; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new GatheringNodeComponent(interactable, this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (abilityName != null && abilityName != string.Empty) {
                GatherAbility tmpBaseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName) as GatherAbility;
                if (tmpBaseAbility != null) {
                    baseAbility = tmpBaseAbility;
                } else {
                    Debug.LogError("GatheringNode.SetupScriptableObjects(): could not find ability " + abilityName);
                }
            }

        }
    }

}