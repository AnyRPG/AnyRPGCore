using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemInteractableOptionConfigManager : SystemResourceManager {

        #region Singleton
        private static SystemInteractableOptionConfigManager instance;

        public static SystemInteractableOptionConfigManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemInteractableOptionConfigManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "InteractableOptionConfig";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<InteractableOptionConfig>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<InteractableOptionConfig>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public InteractableOptionConfig GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as InteractableOptionConfig);
                }
            }
            return null;
        }

        public List<InteractableOptionConfig> GetResourceList() {
            List<InteractableOptionConfig> returnList = new List<InteractableOptionConfig>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as InteractableOptionConfig);
            }
            return returnList;
        }
    }

}