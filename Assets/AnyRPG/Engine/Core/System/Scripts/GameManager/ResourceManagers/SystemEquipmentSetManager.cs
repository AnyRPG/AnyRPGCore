using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEquipmentSetManager : SystemResourceManager {

        #region Singleton
        private static SystemEquipmentSetManager instance;

        public static SystemEquipmentSetManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEquipmentSetManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "EquipmentSet";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<EquipmentSet>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<EquipmentSet>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public EquipmentSet GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as EquipmentSet);
                }
            }
            return null;
        }

        public List<EquipmentSet> GetResourceList() {
            List<EquipmentSet> returnList = new List<EquipmentSet>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as EquipmentSet);
            }
            return returnList;
        }
    }

}