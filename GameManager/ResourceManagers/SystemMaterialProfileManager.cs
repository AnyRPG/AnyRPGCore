using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemMaterialProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemMaterialProfileManager instance;

        public static SystemMaterialProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemMaterialProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "MaterialProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<MaterialProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<MaterialProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public MaterialProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as MaterialProfile);
                }
            }
            return null;
        }


        public MaterialProfile GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    MaterialProfile returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as MaterialProfile;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }


        public List<MaterialProfile> GetResourceList() {
            List<MaterialProfile> returnList = new List<MaterialProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as MaterialProfile);
            }
            return returnList;
        }
    }

}