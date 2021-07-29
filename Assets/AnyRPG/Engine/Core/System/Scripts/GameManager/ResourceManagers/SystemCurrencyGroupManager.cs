using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCurrencyGroupManager : SystemResourceManager {

        #region Singleton
        private static SystemCurrencyGroupManager instance;

        public static SystemCurrencyGroupManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCurrencyGroupManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CurrencyGroup";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CurrencyGroup>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CurrencyGroup>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CurrencyGroup GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CurrencyGroup);
                }
            }
            return null;
        }

        public List<CurrencyGroup> GetResourceList() {
            List<CurrencyGroup> returnList = new List<CurrencyGroup>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CurrencyGroup);
            }
            return returnList;
        }
    }

}