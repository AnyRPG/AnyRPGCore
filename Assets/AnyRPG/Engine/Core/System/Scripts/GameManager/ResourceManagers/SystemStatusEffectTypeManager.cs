using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemStatusEffectTypeManager : SystemResourceManager {

        #region Singleton
        private static SystemStatusEffectTypeManager instance;

        public static SystemStatusEffectTypeManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemStatusEffectTypeManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "StatusEffectType";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<StatusEffectType>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<StatusEffectType>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public StatusEffectType GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as StatusEffectType);
                }
            }
            return null;
        }

        public List<StatusEffectType> GetResourceList() {
            List<StatusEffectType> returnList = new List<StatusEffectType>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as StatusEffectType);
            }
            return returnList;
        }
    }

}