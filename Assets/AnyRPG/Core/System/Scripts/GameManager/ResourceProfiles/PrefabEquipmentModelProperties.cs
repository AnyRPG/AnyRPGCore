using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PrefabEquipmentModelProperties : ConfiguredClass {

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<HoldableObjectAttachment> holdableObjectList = new List<HoldableObjectAttachment>();

        public List<HoldableObjectAttachment> HoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }

        public void SetupScriptableObjects(IDescribable describable) {

            foreach (HoldableObjectAttachment holdableObjectAttachment in holdableObjectList) {
                if (holdableObjectAttachment != null) {
                    holdableObjectAttachment.SetupScriptableObjects(systemGameManager, describable);
                }
            }
        }

    }
}

