using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemAnimationProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemAnimationProfileManager instance;

        public static SystemAnimationProfileManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemAnimationProfileManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "AnimationProfile";

        protected override void Awake() {
            base.Awake();
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<AnimationProfile>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<AnimationProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public AnimationProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as AnimationProfile);
                }
            }
            return null;
        }

        public List<AnimationProfile> GetResourceList() {
            List<AnimationProfile> returnList = new List<AnimationProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as AnimationProfile);
            }
            return returnList;
        }

    }

}