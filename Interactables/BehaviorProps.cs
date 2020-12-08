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
        private List<string> behaviorNames = new List<string>();

        private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        [Tooltip("instantiate a new behavior profile or not when loading behavior profiles")]
        [SerializeField]
        private bool useBehaviorCopy = false;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyDialogNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyDialogNamePlateImage : base.NamePlateImage); }

        public bool UseBehaviorCopy { get => useBehaviorCopy; set => useBehaviorCopy = value; }
        public List<BehaviorProfile> BehaviorList { get => behaviorList; set => behaviorList = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new BehaviorComponent(interactable, this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (behaviorNames != null) {
                foreach (string behaviorName in behaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                    if (useBehaviorCopy == true) {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetNewResource(behaviorName);
                    } else {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetResource(behaviorName);
                    }
                    if (tmpBehaviorProfile != null) {
                        behaviorList.Add(tmpBehaviorProfile);
                    }
                }
            }

        }
    }

}