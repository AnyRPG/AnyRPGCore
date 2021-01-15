using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEnvironmentStateProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemEnvironmentStateProfileManager instance;

        public static SystemEnvironmentStateProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEnvironmentStateProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "EnvironmentStateProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<EnvironmentStateProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<EnvironmentStateProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public EnvironmentStateProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as EnvironmentStateProfile);
                }
            }
            return null;
        }

        public List<EnvironmentStateProfile> GetResourceList() {
            List<EnvironmentStateProfile> returnList = new List<EnvironmentStateProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as EnvironmentStateProfile);
            }
            return returnList;
        }
    }

}