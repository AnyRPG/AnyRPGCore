using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemMusicProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemMusicProfileManager instance;

        public static SystemMusicProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemMusicProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "MusicProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<MusicProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<MusicProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public MusicProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as MusicProfile);
                }
            }
            return null;
        }

        public List<MusicProfile> GetResourceList() {
            List<MusicProfile> returnList = new List<MusicProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as MusicProfile);
            }
            return returnList;
        }
    }

}