using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class BehaviorProps : InteractableOptionProps {

        [Header("Behavior")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Behaviour))]
        private List<string> behaviorNames = new List<string>();

        [Tooltip("instantiate a new behavior profile or not when loading behavior profiles")]
        [SerializeField]
        private bool useBehaviorCopy = false;

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.DialogInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.DialogInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.DialogNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.DialogNamePlateImage : base.NamePlateImage); }

        public bool UseBehaviorCopy { get => useBehaviorCopy; }
        public List<string> BehaviorNames { get => behaviorNames; }

        public override string GetInteractionPanelTitle(int optionIndex = 0) {
            return (behaviorNames.Count > optionIndex ? behaviorNames[optionIndex] : base.GetInteractionPanelTitle(optionIndex));
        }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new BehaviorComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        /*
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (behaviorNames != null) {
                foreach (string behaviorName in behaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                    if (useBehaviorCopy == true) {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.Instance.GetNewResource(behaviorName);
                    } else {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.Instance.GetResource(behaviorName);
                    }
                    if (tmpBehaviorProfile != null) {
                        behaviorList.Add(tmpBehaviorProfile);
                    }
                }
            }
        }
        */

    }

}