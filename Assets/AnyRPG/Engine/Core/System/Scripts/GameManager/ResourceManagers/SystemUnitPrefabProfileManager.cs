using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemUnitPrefabProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemUnitPrefabProfileManager instance;

        public static SystemUnitPrefabProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemUnitPrefabProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "UnitPrefabProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<UnitPrefabProfile>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<UnitPrefabProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public UnitPrefabProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as UnitPrefabProfile);
                }
            }
            return null;
        }

        public List<UnitPrefabProfile> GetResourceList() {
            List<UnitPrefabProfile> returnList = new List<UnitPrefabProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as UnitPrefabProfile);
            }
            return returnList;
        }
    }

}