using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    public class SystemResourceManager : MonoBehaviour {

        protected ResourceProfile[] rawResourceList;
        protected List<UnityEngine.Object[]> masterList = new List<UnityEngine.Object[]>();

        protected Dictionary<string, ResourceProfile> resourceList = new Dictionary<string, ResourceProfile>();

        protected bool eventSubscriptionsInitialized = false;

        public Dictionary<string, ResourceProfile> ResourceList { get => resourceList; set => resourceList = value; }

        protected virtual void Awake() {
            //LoadResourceList();
        }

        protected virtual void Start() {
            // reload all lists just to be safe
            CreateEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        public void ReloadResourceList() {
            //Debug.Log(gameObject.name + ".SystemResourceManager.ReloadResourceList()");
            resourceList.Clear();
            LoadResourceList();
        }

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
                    Debug.Log(resource.name + " had empty MyName value");
                    (resource as ResourceProfile).ResourceName = resource.name;
                }
                if (resource.MyDescription == null) {
                    //Debug.Log(resource.name + " had empty description value");
                    resource.MyDescription = string.Empty;
                }
                string keyName = prepareStringForMatch(resource.ResourceName);
                if (!resourceList.ContainsKey(keyName)) {
                    resourceList[keyName] = ScriptableObject.Instantiate(resource);
                } else {
                    Debug.LogError("SystemResourceManager.LoadResourceList(): duplicate name key: " + keyName);
                }
            }
        }

        public static string prepareStringForMatch(string oldString) {
            return oldString.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty);
        }

        public static bool MatchResource(string resourceName, string resourceMatchName) {
            if (resourceName != null && resourceMatchName != null) {
                if (prepareStringForMatch(resourceName) == prepareStringForMatch(resourceMatchName)) {
                    return true;
                }
            } else {
                //Debug.Log("SystemGameManager.MatchResource(" + (resourceName == null ? "null" : resourceName) + ", " + (resourceMatchName == null ? "null" : resourceMatchName) + ")");
            }
            return false;
        }

        public static bool RequestIsEmpty(string resourceName) {
            if (resourceName == null || resourceName == string.Empty) {
                //Debug.Log("SystemResourceManager.RequestIsEmpty(" + resourceName + "): EMPTY RESOURCE REQUESTED.  FIX THIS! DO NOT COMMENT THIS LINE");
                return true;
            }
            return false;
        }

        public virtual void SetupScriptableObjects() {
            foreach (ResourceProfile listItem in resourceList.Values) {
                listItem.SetupScriptableObjects();
            }
        }

        public virtual void CleanupScriptableObjects() {
            foreach (ResourceProfile listItem in resourceList.Values) {
                listItem.CleanupScriptableObjects();
            }
        }

    }

}