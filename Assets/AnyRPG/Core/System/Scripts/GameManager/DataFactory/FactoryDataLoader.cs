using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// load resource profiles of a specific class
    /// </summary>
    /// <typeparam name="TDataType"></typeparam>
    public class FactoryDataLoader<TDataType> where TDataType : ResourceProfile {

        private string resourceClassName = string.Empty;

        // game manager references
        private SystemConfigurationManager systemConfigurationManager = null;

        public FactoryDataLoader(string resourceClassName, SystemGameManager systemGameManager) {
            this.resourceClassName = resourceClassName;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
        }

        /// <summary>
        /// load the ScriptableObjects of TDataType from disk into a dictionary
        /// </summary>
        public Dictionary<string, ResourceProfile> LoadResourceList() {
            List<ResourceProfile> resourceList = new List<ResourceProfile>();
            Dictionary<string, ResourceProfile> resourceDictionary = new Dictionary<string, ResourceProfile>();

            // add the scriptable objects stored in the root of all Resources folders to the master list
            resourceList.AddRange(Resources.LoadAll<TDataType>(resourceClassName));

            // add the scriptable objects stored specific subfolders of the Resources folders to the master list
            if (systemConfigurationManager != null) {
                foreach (string loadResourcesFolder in systemConfigurationManager.LoadResourcesFolders) {
                    resourceList.AddRange(Resources.LoadAll<TDataType>(loadResourcesFolder + "/" + resourceClassName));
                }
            }

            // populate the resource dictionary
            foreach (ResourceProfile resource in resourceList) {
                if (resource.ResourceName == null) {
                    Debug.Log($"{resource.name} had empty ResourceName value");
                    (resource as ResourceProfile).ResourceName = resource.name;
                }
                if (resource.Description == null) {
                    resource.Description = string.Empty;
                }
                string keyName = SystemDataUtility.PrepareStringForMatch(resource.ResourceName);
                if (!resourceDictionary.ContainsKey(keyName)) {
                    resourceDictionary[keyName] = ScriptableObject.Instantiate(resource);
                } else {
                    Debug.LogError($"SystemResourceManager.LoadResourceList(): duplicate name key: {keyName} in {resource.name}. Other item: {resourceDictionary[keyName].name}");
                }
            }

            return resourceDictionary;
        }

    }

}