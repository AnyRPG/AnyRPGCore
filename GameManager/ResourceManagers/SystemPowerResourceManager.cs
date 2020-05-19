using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemPowerResourceManager : SystemResourceManager {

        #region Singleton
        private static SystemPowerResourceManager instance;

        public static SystemPowerResourceManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemPowerResourceManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "PowerResource";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<PowerResource>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<PowerResource>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public PowerResource GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as PowerResource);
                }
            }
            return null;
        }

        public List<PowerResource> GetResourceList() {
            List<PowerResource> returnList = new List<PowerResource>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as PowerResource);
            }
            return returnList;
        }
    }

}