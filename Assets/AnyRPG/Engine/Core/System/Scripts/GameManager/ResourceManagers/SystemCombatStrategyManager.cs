using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCombatStrategyManager : SystemResourceManager {

        #region Singleton
        private static SystemCombatStrategyManager instance;

        public static SystemCombatStrategyManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCombatStrategyManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CombatStrategy";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CombatStrategy>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CombatStrategy>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CombatStrategy GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CombatStrategy);
                }
            }
            return null;
        }


        public CombatStrategy GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    CombatStrategy returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as CombatStrategy;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }


        public List<CombatStrategy> GetResourceList() {
            List<CombatStrategy> returnList = new List<CombatStrategy>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CombatStrategy);
            }
            return returnList;
        }
    }

}