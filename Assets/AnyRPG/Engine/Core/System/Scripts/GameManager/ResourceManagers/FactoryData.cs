using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FactoryData<TDataType> : FactoryResource where TDataType : UnityEngine.Object {

        //public TDataType dataType { get; set; }
        private string resourceClassName = string.Empty;

        public FactoryData(string resourceClassName) {
            this.resourceClassName = resourceClassName;
        }


        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<TDataType>(resourceClassName));
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                foreach (string loadResourcesFolder in SystemGameManager.Instance.SystemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<TDataType>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }
        /*
        public TDataType GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as TDataType);
                }
            }
            return null;
        }
        */
        /*
        public List<TDataType> GetResourceList() {
            List<TDataType> returnList = new List<TDataType>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as TDataType);
            }
            return returnList;
        }
        */
        /*
        public Dictionary<string, TDataType> GetResourceDict() {
            Dictionary<string, TDataType> returnList = new Dictionary<string, TDataType>();
            foreach (KeyValuePair<string, UnityEngine.Object> listItem in resourceList) {
                returnList.Add(listItem.Key, listItem.Value as TDataType);
            }
            return returnList;
        }
        */
    }

}