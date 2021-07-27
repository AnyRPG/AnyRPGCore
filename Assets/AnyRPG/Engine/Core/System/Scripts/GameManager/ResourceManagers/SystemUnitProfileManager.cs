using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemUnitProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemUnitProfileManager instance;

        public static SystemUnitProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemUnitProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "UnitProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<UnitProfile>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<UnitProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public UnitProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as UnitProfile);
                }
            }
            return null;
        }

        public List<UnitProfile> GetResourceList() {
            List<UnitProfile> returnList = new List<UnitProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as UnitProfile);
            }
            return returnList;
        }
    }

}