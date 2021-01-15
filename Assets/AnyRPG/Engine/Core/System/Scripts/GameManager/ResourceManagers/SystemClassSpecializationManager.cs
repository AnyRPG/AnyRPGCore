using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemClassSpecializationManager : SystemResourceManager {

        #region Singleton
        private static SystemClassSpecializationManager instance;

        public static SystemClassSpecializationManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemClassSpecializationManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "ClassSpecialization";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<ClassSpecialization>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<ClassSpecialization>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public ClassSpecialization GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as ClassSpecialization);
                }
            }
            return null;
        }

        public List<ClassSpecialization> GetResourceList() {
            List<ClassSpecialization> returnList = new List<ClassSpecialization>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as ClassSpecialization);
            }
            return returnList;
        }
    }

}