using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCreditsCategoryManager : SystemResourceManager {

        #region Singleton
        private static SystemCreditsCategoryManager instance;

        public static SystemCreditsCategoryManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCreditsCategoryManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CreditsCategory";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CreditsCategory>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CreditsCategory>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CreditsCategory GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CreditsCategory);
                }
            }
            return null;
        }

        public List<CreditsCategory> GetResourceList() {
            List<CreditsCategory> returnList = new List<CreditsCategory>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CreditsCategory);
            }
            return returnList;
        }
    }

}