using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEquipmentSlotTypeManager : SystemResourceManager {

        #region Singleton
        private static SystemEquipmentSlotTypeManager instance;

        public static SystemEquipmentSlotTypeManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemEquipmentSlotTypeManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "EquipmentSlotType";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<EquipmentSlotType>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<EquipmentSlotType>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public EquipmentSlotType GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as EquipmentSlotType);
                }
            }
            return null;
        }

        public List<EquipmentSlotType> GetResourceList() {
            List<EquipmentSlotType> returnList = new List<EquipmentSlotType>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as EquipmentSlotType);
            }
            return returnList;
        }
    }

}