using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemLootTableManager : SystemResourceManager {

        #region Singleton
        private static SystemLootTableManager instance;

        public static SystemLootTableManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemLootTableManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "LootTable";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<LootTable>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<LootTable>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public LootTable GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as LootTable);
                }
            }
            return null;
        }

        public LootTable GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    LootTable returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as LootTable;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }


        public List<LootTable> GetResourceList() {
            List<LootTable> returnList = new List<LootTable>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as LootTable);
            }
            return returnList;
        }
    }

}