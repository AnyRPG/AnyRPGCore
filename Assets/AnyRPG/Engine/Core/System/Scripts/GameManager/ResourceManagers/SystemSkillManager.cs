using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemSkillManager : SystemResourceManager {

        #region Singleton
        private static SystemSkillManager instance;

        public static SystemSkillManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemSkillManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "Skill";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<Skill>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Skill>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Skill GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Skill);
                }
            }
            return null;
        }

        public List<Skill> GetResourceList() {
            List<Skill> returnList = new List<Skill>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Skill);
            }
            return returnList;
        }

    }

}