using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAudioProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemAudioProfileManager instance;

        public static SystemAudioProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemAudioProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "AudioProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<AudioProfile>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<AudioProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public AudioProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as AudioProfile);
                }
            }
            return null;
        }

        public List<AudioProfile> GetResourceList() {
            List<AudioProfile> returnList = new List<AudioProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as AudioProfile);
            }
            return returnList;
        }
    }

}