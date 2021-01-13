using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCharacterRaceManager : SystemResourceManager {

        #region Singleton
        private static SystemCharacterRaceManager instance;

        public static SystemCharacterRaceManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCharacterRaceManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "CharacterRace";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<CharacterRace>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<CharacterRace>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public CharacterRace GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as CharacterRace);
                }
            }
            return null;
        }

        public List<CharacterRace> GetResourceList() {
            List<CharacterRace> returnList = new List<CharacterRace>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as CharacterRace);
            }
            return returnList;
        }
    }

}