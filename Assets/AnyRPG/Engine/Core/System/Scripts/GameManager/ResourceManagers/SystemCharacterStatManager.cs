using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCharacterStatManager : SystemResourceManager {

        #region Singleton
        private static SystemCharacterStatManager instance;

        public static SystemCharacterStatManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCharacterStatManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CharacterStat";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CharacterStat>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CharacterStat>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CharacterStat GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CharacterStat);
                }
            }
            return null;
        }

        public List<CharacterStat> GetResourceList() {
            List<CharacterStat> returnList = new List<CharacterStat>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CharacterStat);
            }
            return returnList;
        }
    }

}