using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemCutsceneManager : SystemResourceManager {

        #region Singleton
        private static SystemCutsceneManager instance;

        public static SystemCutsceneManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemCutsceneManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "Cutscene";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<Cutscene>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Cutscene>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Cutscene GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Cutscene);
                }
            }
            return null;
        }

        public List<Cutscene> GetResourceList() {
            List<Cutscene> returnList = new List<Cutscene>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Cutscene);
            }
            return returnList;
        }

        public void LoadCutscene(CutsceneSaveData cutsceneSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            Cutscene cutScene = GetResource(cutsceneSaveData.MyName);
            if (cutScene == null) {
                //Debug.LogError("SystemSceneNodeManager.LoadSceneNode(): Scene " + sceneNodeSaveData.MyName + " could not be found.");
                return;
            }
            cutScene.Viewed = cutsceneSaveData.isCutSceneViewed;
        }
    }

}