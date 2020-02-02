using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemPatrolProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemPatrolProfileManager instance;

        public static SystemPatrolProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemPatrolProfileManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "PatrolProfile";

        // the icon shown when a player has no faction
        [SerializeField]
        private Sprite defaultIcon;

        public Sprite MyDefaultIcon { get => defaultIcon; set => defaultIcon = value; }

        protected override void Awake() {
            base.Awake();
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<PatrolProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<PatrolProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public PatrolProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as PatrolProfile);
                }
            }
            return null;
        }

        public PatrolProfile GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    PatrolProfile returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as PatrolProfile;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }

        public List<PatrolProfile> GetResourceList() {
            List<PatrolProfile> returnList = new List<PatrolProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as PatrolProfile);
            }
            return returnList;
        }

    }

}