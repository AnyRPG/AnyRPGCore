using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemVendorCollectionManager : SystemResourceManager {

        #region Singleton
        private static SystemVendorCollectionManager instance;

        public static SystemVendorCollectionManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemVendorCollectionManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "VendorCollection";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<VendorCollection>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<VendorCollection>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public VendorCollection GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as VendorCollection);
                }
            }
            return null;
        }

        public List<VendorCollection> GetResourceList() {
            List<VendorCollection> returnList = new List<VendorCollection>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as VendorCollection);
            }
            return returnList;
        }
    }

}