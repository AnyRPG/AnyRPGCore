using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : SystemResourceManager {

        #region Singleton
        private static SystemItemManager instance;

        public static SystemItemManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemItemManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "Item";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<Item>(resourceClassName));
            if (SystemConfigurationManager.Instance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.Instance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Item>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Item GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Item);
                }
            }
            return null;
        }


        public Item GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    Item returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as Item;
                    returnValue.SetupScriptableObjects();
                    returnValue.InitializeNewItem();
                    return returnValue;
                }
            }
            return null;
        }


        public List<Item> GetResourceList() {
            List<Item> returnList = new List<Item>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Item);
            }
            return returnList;
        }
    }

}