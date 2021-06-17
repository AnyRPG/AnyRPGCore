using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemWeaponSkillManager : SystemResourceManager {

        #region Singleton
        private static SystemWeaponSkillManager instance;

        public static SystemWeaponSkillManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemWeaponSkillManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "WeaponSkill";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<WeaponSkill>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<WeaponSkill>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public WeaponSkill GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as WeaponSkill);
                }
            }
            return null;
        }

        public List<WeaponSkill> GetResourceList() {
            List<WeaponSkill> returnList = new List<WeaponSkill>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as WeaponSkill);
            }
            return returnList;
        }
    }

}