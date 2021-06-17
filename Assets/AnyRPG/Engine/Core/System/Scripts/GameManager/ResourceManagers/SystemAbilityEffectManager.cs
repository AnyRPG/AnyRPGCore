using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    public class SystemAbilityEffectManager : SystemResourceManager {

        #region Singleton
        private static SystemAbilityEffectManager instance;

        public static SystemAbilityEffectManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemAbilityEffectManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "AbilityEffect";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<AbilityEffect>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<AbilityEffect>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public AbilityEffect GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as AbilityEffect);
                }
            }
            return null;
        }

        public List<AbilityEffect> GetResourceList() {
            List<AbilityEffect> returnList = new List<AbilityEffect>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as AbilityEffect);
            }
            return returnList;
        }

    }

}