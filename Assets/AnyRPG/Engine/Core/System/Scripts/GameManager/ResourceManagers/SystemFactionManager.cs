using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemFactionManager : SystemResourceManager {

        #region Singleton
        private static SystemFactionManager instance;

        public static SystemFactionManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemFactionManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "Faction";

        protected override void Awake() {
            base.Awake();
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<Faction>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Faction>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Faction GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Faction);
                }
            }
            return null;
        }

        public List<Faction> GetResourceList() {
            List<Faction> returnList = new List<Faction>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Faction);
            }
            return returnList;
        }

    }

}