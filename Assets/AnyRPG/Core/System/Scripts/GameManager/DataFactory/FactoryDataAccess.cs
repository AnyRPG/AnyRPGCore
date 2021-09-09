using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class FactoryDataAccess {

        public Dictionary<string, ResourceProfile> factoryData = new Dictionary<string, ResourceProfile>();

        public void Setup<TDataType>(string dataName, SystemGameManager systemGameManager) where TDataType : ResourceProfile {
            FactoryData<TDataType> abilityFactory = new FactoryData<TDataType>(dataName, systemGameManager);
            abilityFactory.LoadResourceList();
            factoryData = abilityFactory.ResourceList;
            //return abilityFactory.GetResourceDict();
        }

        public int GetResourceCount() {

            return factoryData.Count;
        }

        public List<TDataType> GetResourceList<TDataType>() where TDataType : ResourceProfile {
            List<TDataType> returnList = new List<TDataType>();

            foreach (UnityEngine.Object listItem in factoryData.Values) {
                returnList.Add(listItem as TDataType);
            }
            
            return returnList;
        }

        public TDataType GetResource<TDataType>(string resourceName) where TDataType : ResourceProfile {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!SystemDataFactory.RequestIsEmpty(resourceName)) {
                string keyName = SystemDataFactory.PrepareStringForMatch(resourceName);
                /*
                foreach (string dictKeyName in factoryData.Keys) {
                    Debug.Log("count: " + factoryData.Count + "; " + dictKeyName);
                }
                */
                if (factoryData.ContainsKey(keyName)) {
                    return (factoryData[keyName] as TDataType);
                }
            }
            return default(TDataType);
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("FactoryDataAccess.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            foreach (ResourceProfile resourceProfile in factoryData.Values) {
                resourceProfile.SetupScriptableObjects(systemGameManager);
            }
        }

        //private Dictionary<string, string> dataDictionary = new Dictionary<string, string>();


    }

}