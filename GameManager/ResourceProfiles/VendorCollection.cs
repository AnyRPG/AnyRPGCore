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

        [SerializeField]
        private List<VendorItem> vendorItems = new List<VendorItem>();

        public List<VendorItem> MyVendorItems { get => vendorItems; set => vendorItems = value; }
    }

}