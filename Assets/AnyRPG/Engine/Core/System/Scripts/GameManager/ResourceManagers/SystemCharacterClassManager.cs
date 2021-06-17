using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCharacterClassManager : SystemResourceManager {

        #region Singleton
        private static SystemCharacterClassManager instance;

        public static SystemCharacterClassManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCharacterClassManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CharacterClass";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CharacterClass>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CharacterClass>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CharacterClass GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CharacterClass);
                }
            }
            return null;
        }

        public List<CharacterClass> GetResourceList() {
            List<CharacterClass> returnList = new List<CharacterClass>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CharacterClass);
            }
            return returnList;
        }
    }

}