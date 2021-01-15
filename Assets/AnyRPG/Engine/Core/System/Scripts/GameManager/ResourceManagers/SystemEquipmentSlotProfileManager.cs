using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEquipmentSlotProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemEquipmentSlotProfileManager instance;

        public static SystemEquipmentSlotProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEquipmentSlotProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "EquipmentSlotProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<EquipmentSlotProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<EquipmentSlotProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public EquipmentSlotProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as EquipmentSlotProfile);
                }
            }
            return null;
        }

        public List<EquipmentSlotProfile> GetResourceList() {
            List<EquipmentSlotProfile> returnList = new List<EquipmentSlotProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as EquipmentSlotProfile);
            }
            return returnList;
        }
    }

}