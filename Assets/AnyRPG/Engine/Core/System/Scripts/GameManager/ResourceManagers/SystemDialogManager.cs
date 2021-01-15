using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemDialogManager : SystemResourceManager {

        #region Singleton
        private static SystemDialogManager instance;

        public static SystemDialogManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemDialogManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "Dialog";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<Dialog>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Dialog>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Dialog GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Dialog);
                }
            }
            return null;
        }

        public List<Dialog> GetResourceList() {
            List<Dialog> returnList = new List<Dialog>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Dialog);
            }
            return returnList;
        }

        /*
        public void LoadDialog(DialogSaveData dialogSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            Dialog dialog = GetResource(dialogSaveData.MyName);
            if (dialog != null) {
                dialog.TurnedIn = dialogSaveData.turnedIn;
            }
        }
        */
    }

}