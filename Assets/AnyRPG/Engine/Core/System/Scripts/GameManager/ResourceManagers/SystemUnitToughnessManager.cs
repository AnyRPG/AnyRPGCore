using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemUnitToughnessManager : SystemResourceManager {

        #region Singleton
        private static SystemUnitToughnessManager instance;

        public static SystemUnitToughnessManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemUnitToughnessManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "UnitToughness";

        protected override void Awake() {
            base.Awake();
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<UnitToughness>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<UnitToughness>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public UnitToughness GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as UnitToughness);
                }
            }
            return null;
        }

        public List<UnitToughness> GetResourceList() {
            List<UnitToughness> returnList = new List<UnitToughness>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as UnitToughness);
            }
            return returnList;
        }

    }

}