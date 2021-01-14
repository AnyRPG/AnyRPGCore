using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemUnitTypeManager : SystemResourceManager {

        #region Singleton
        private static SystemUnitTypeManager instance;

        public static SystemUnitTypeManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemUnitTypeManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "UnitType";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<UnitType>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<UnitType>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public UnitType GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as UnitType);
                }
            }
            return null;
        }

        public List<UnitType> GetResourceList() {
            List<UnitType> returnList = new List<UnitType>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as UnitType);
            }
            return returnList;
        }
    }

}