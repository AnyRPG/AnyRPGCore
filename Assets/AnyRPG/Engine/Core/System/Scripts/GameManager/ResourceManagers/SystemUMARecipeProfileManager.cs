using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemUMARecipeProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemUMARecipeProfileManager instance;

        public static SystemUMARecipeProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemUMARecipeProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "UMARecipeProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<UMARecipeProfile>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<UMARecipeProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public UMARecipeProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as UMARecipeProfile);
                }
            }
            return null;
        }

        public List<UMARecipeProfile> GetResourceList() {
            List<UMARecipeProfile> returnList = new List<UMARecipeProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as UMARecipeProfile);
            }
            return returnList;
        }
    }

}