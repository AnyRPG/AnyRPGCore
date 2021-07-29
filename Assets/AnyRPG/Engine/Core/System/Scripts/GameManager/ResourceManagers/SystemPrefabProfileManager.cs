using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemPrefabProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemPrefabProfileManager instance;

        public static SystemPrefabProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemPrefabProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "PrefabProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<PrefabProfile>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<PrefabProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public PrefabProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as PrefabProfile);
                }
            }
            return null;
        }


        public PrefabProfile GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    PrefabProfile returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as PrefabProfile;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }


        public List<PrefabProfile> GetResourceList() {
            List<PrefabProfile> returnList = new List<PrefabProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as PrefabProfile);
            }
            return returnList;
        }
    }

}