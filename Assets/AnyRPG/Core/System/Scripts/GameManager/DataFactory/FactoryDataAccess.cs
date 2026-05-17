using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// access ResourceProfiles of a specific class
    /// </summary>
    public class FactoryDataAccess {

        public Dictionary<string, ResourceProfile> resourceDictionary = new Dictionary<string, ResourceProfile>();

        public void Setup<TDataType>(SystemGameManager systemGameManager) where TDataType : ResourceProfile {
            //Debug.Log($"FactoryDataAccess.Setup<{typeof(TDataType).Name}>()");

            FactoryDataLoader<TDataType> factoryData = new FactoryDataLoader<TDataType>(typeof(TDataType).Name, systemGameManager);
            resourceDictionary = factoryData.LoadResourceList();
        }

        /// <summary>
        /// return a count of resources stored in this factory
        /// </summary>
        /// <returns></returns>
        public int GetResourceCount() {
            return resourceDictionary.Count;
        }

        /// <summary>
        /// return all resources of the provided class in the data factory
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <returns></returns>
        public List<TDataType> GetResourceList<TDataType>() where TDataType : ResourceProfile {
            List<TDataType> returnList = new List<TDataType>();

            foreach (UnityEngine.Object listItem in resourceDictionary.Values) {
                returnList.Add(listItem as TDataType);
            }
            
            return returnList;
        }

        /// <summary>
        /// return the requested resource of the provided class from the data factory
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public TDataType GetResource<TDataType>(string resourceName) where TDataType : ResourceProfile {
            if (!SystemDataUtility.RequestIsEmpty(resourceName)) {
                string keyName = SystemDataUtility.PrepareStringForMatch(resourceName);
                if (resourceDictionary.ContainsKey(keyName)) {
                    return (resourceDictionary[keyName] as TDataType);
                }
            }

            return default(TDataType);
        }

        /// <summary>
        /// setup the scriptable objects in the resource dictionary
        /// </summary>
        /// <param name="systemGameManager"></param>
        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            foreach (ResourceProfile resourceProfile in resourceDictionary.Values) {
                resourceProfile.SetupScriptableObjects(systemGameManager);
            }
        }

        public void AddResource<TDataType>(TDataType resourceProfile) where TDataType : ResourceProfile {
            //Debug.Log($"FactoryDataAccess.AddResource({resourceProfile.ResourceName}) {typeof(TDataType).Name}");

            if (resourceDictionary.ContainsKey(SystemDataUtility.PrepareStringForMatch(resourceProfile.ResourceName))) {
                //Debug.Log($"FactoryDataAccess.AddResource({resourceProfile.ResourceName}) already in dictionary!");
                return;
            } else {
                resourceDictionary.Add(SystemDataUtility.PrepareStringForMatch(resourceProfile.ResourceName), resourceProfile);
                //Debug.Log($"FactoryDataAccess.AddResource({resourceProfile.ResourceName}) adding to dictionary");
            }
        }
    }

}