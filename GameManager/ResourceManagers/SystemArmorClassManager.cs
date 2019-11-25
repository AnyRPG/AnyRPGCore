using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemArmorClassManager : SystemResourceManager {

        #region Singleton
        private static SystemArmorClassManager instance;

        public static SystemArmorClassManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemArmorClassManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "ArmorClass";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<ArmorClass>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<ArmorClass>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public ArmorClass GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as ArmorClass);
                }
            }
            return null;
        }

        public List<ArmorClass> GetResourceList() {
            List<ArmorClass> returnList = new List<ArmorClass>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as ArmorClass);
            }
            return returnList;
        }
    }

}