using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    public class FactoryResource {

        protected ResourceProfile[] rawResourceList;
        protected List<UnityEngine.Object[]> masterList = new List<UnityEngine.Object[]>();

        protected Dictionary<string, ResourceProfile> resourceList = new Dictionary<string, ResourceProfile>();

        protected bool eventSubscriptionsInitialized = false;

        public Dictionary<string, ResourceProfile> ResourceList { get => resourceList; set => resourceList = value; }
        
        /*
        public virtual void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }
        */

            /*
        public void ReloadResourceList() {
            //Debug.Log(gameObject.name + ".SystemResourceManager.ReloadResourceList()");
            CleanupScriptableObjects();
            resourceList.Clear();
            LoadResourceList();
        }
        */

        public virtual void LoadResourceList() {
            //Debug.Log(gameObject.name + ".SystemResourceManager.LoadResourceList()");
            int tmpLength = 0;
            foreach (ResourceProfile[] subList in masterList) {
                tmpLength += subList.Length;
            }
            rawResourceList = new ResourceProfile[tmpLength];
            int indexPosition = 0;
            foreach (ResourceProfile[] subList in masterList) {
                Array.Copy(subList, 0, rawResourceList, indexPosition, subList.Length);
                indexPosition += subList.Length;
            }
            masterList.Clear();

            // do this after the parent function so it's properly set
            foreach (ResourceProfile resource in rawResourceList) {
                if (resource.ResourceName == null) {
                    Debug.Log(resource.name + " had empty ResourceName value");
                    (resource as ResourceProfile).ResourceName = resource.name;
                }
                if (resource.Description == null) {
                    //Debug.Log(resource.name + " had empty description value");
                    resource.Description = string.Empty;
                }
                string keyName = SystemDataFactory.PrepareStringForMatch(resource.ResourceName);
                if (!resourceList.ContainsKey(keyName)) {
                    resourceList[keyName] = ScriptableObject.Instantiate(resource);
                } else {
                    Debug.LogError("SystemResourceManager.LoadResourceList(): duplicate name key: " + keyName + " in " + resource.name + ". Other item: " + resourceList[keyName].name);
                }
            }
        }

        /*
        public virtual void SetupScriptableObjects() {
            foreach (ResourceProfile listItem in resourceList.Values) {
                listItem.SetupScriptableObjects();
            }
        }
        */
        /*
        public virtual void CleanupScriptableObjects() {
            foreach (ResourceProfile listItem in resourceList.Values) {
                listItem.CleanupScriptableObjects();
            }
        }
        */

    }

}