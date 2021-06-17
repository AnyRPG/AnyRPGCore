using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemQualityManager : SystemResourceManager {

        #region Singleton
        private static SystemItemQualityManager instance;

        public static SystemItemQualityManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemItemQualityManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "ItemQuality";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<ItemQuality>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<ItemQuality>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public ItemQuality GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as ItemQuality);
                }
            }
            return null;
        }

        public List<ItemQuality> GetResourceList() {
            List<ItemQuality> returnList = new List<ItemQuality>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as ItemQuality);
            }
            return returnList;
        }
    }

}