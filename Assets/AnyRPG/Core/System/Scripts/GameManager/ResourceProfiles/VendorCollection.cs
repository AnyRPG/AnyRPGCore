using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Vendor Collection", menuName = "AnyRPG/VendorCollection")]
    [System.Serializable]
    public class VendorCollection : DescribableResource {

        [Header("Vendor Collection")]

        [Tooltip("List of items in this collection")]
        [SerializeField]
        private List<VendorItem> vendorItems = new List<VendorItem>();

        public List<VendorItem> VendorItems { get => vendorItems; set => vendorItems = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (vendorItems != null) {
                foreach (VendorItem vendorItem in vendorItems) {
                    if (vendorItem != null) {
                        vendorItem.SetupScriptableObjects(systemDataFactory);
                    }

                }
            }
        }
    }

}