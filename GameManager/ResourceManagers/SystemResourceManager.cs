using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    public class SystemResourceManager : MonoBehaviour {

        protected UnityEngine.Object[] rawResourceList;
        protected List<UnityEngine.Object[]> masterList = new List<UnityEngine.Object[]>();

        protected Dictionary<string, UnityEngine.Object> resourceList = new Dictionary<string, UnityEngine.Object>();

        protected bool eventSubscriptionsInitialized = false;
        protected bool startHasRun = false;

        public Dictionary<string, UnityEngine.Object> MyResourceList { get => resourceList; set => resourceList = value; }

        protected virtual void Awake() {
            LoadResourceList();
        }

        protected virtual void Start() {
            // reload all lists just to be safe
            startHasRun = true;
            CreateEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized || !startHasRun) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ReloadResourceList;
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ReloadResourceList;
            eventSubscriptionsInitialized = false;
        }

        public virtual void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void ReloadResourceList() {
            resourceList.Clear();
            LoadResourceList();
        }

        public virtual void LoadResourceList() {
            int tmpLength = 0;
            foreach (UnityEngine.Object[] subList in masterList) {
                tmpLength += subList.Length;
            }
            rawResourceList = new UnityEngine.Object[tmpLength];
            int indexPosition = 0;
            foreach (UnityEngine.Object[] subList in masterList) {
                Array.Copy(subList, 0, rawResourceList, indexPosition, subList.Length);
                indexPosition += subList.Length;
            }
            masterList.Clear();

            // do this after the parent function so it's properly set
            foreach (UnityEngine.Object resource in rawResourceList) {
                if ((resource as DescribableResource).MyName == null) {
                    Debug.Log(resource.name + " had empty MyName value");
                    (resource as DescribableResource).MyName = resource.name;
                }
                if ((resource as DescribableResource).MyDescription == null) {
                    //Debug.Log(resource.name + " had empty description value");
                    (resource as DescribableResource).MyDescription = string.Empty;
                }
                string keyName = prepareStringForMatch((resource as DescribableResource).MyName);
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
                Debug.Log("SystemResourceManager.RequestIsEmpty(" + resourceName + "): EMPTY RESOURCE REQUESTED.  FIX THIS! DO NOT COMMENT THIS LINE");
                return true;
            }
            return false;
        }

    }

}