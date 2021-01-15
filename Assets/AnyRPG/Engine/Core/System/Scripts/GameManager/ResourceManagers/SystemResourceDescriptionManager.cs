using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemResourceDescriptionManager : SystemResourceManager {

        #region Singleton
        private static SystemResourceDescriptionManager instance;

        public static SystemResourceDescriptionManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemResourceDescriptionManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "ResourceDescription";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<ResourceDescription>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<ResourceDescription>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public ResourceDescription GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as ResourceDescription);
                }
            }
            return null;
        }

        public List<ResourceDescription> GetResourceList() {
            List<ResourceDescription> returnList = new List<ResourceDescription>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as ResourceDescription);
            }
            return returnList;
        }
    }

}