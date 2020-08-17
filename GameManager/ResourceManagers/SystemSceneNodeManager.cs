using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemSceneNodeManager : SystemResourceManager {

        #region Singleton
        private static SystemSceneNodeManager instance;

        public static SystemSceneNodeManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemSceneNodeManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "SceneNode";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<SceneNode>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<SceneNode>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public SceneNode GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as SceneNode);
                }
            }
            return null;
        }


        public List<SceneNode> GetResourceList() {
            List<SceneNode> returnList = new List<SceneNode>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as SceneNode);
            }
            return returnList;
        }

        public void LoadSceneNode(SceneNodeSaveData sceneNodeSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            SceneNode sceneNode = GetResource(sceneNodeSaveData.MyName);
            if (sceneNode == null) {
                //Debug.LogError("SystemSceneNodeManager.LoadSceneNode(): Scene " + sceneNodeSaveData.MyName + " could not be found.");
                return;
            }
            sceneNode.Visited = sceneNodeSaveData.visited;
            if (sceneNodeSaveData.persistentObjects != null) {
                foreach (PersistentObjectSaveData persistentObjectSaveData in sceneNodeSaveData.persistentObjects) {
                    if (sceneNode.PersistentObjects.ContainsKey(persistentObjectSaveData.UUID) == false) {
                        sceneNode.PersistentObjects.Add(persistentObjectSaveData.UUID, persistentObjectSaveData);
                    }
                }
            }
        }
    }

}