using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class HostServerScenesPanel : WindowPanel {

        [Header("Host Server Scenes Panel")]

        [SerializeField]
        protected GameObject loadedSceneTemplate = null;

        [SerializeField]
        protected Transform loadedSceneContainer = null;

        [SerializeField]
        protected UINavigationController sceneListNavigationController = null;

        // game manager references
        private NetworkManagerServer networkManagerServer = null;
        private ObjectPooler objectPooler = null;
        private AuthenticationService authenticationService = null;
        private LevelManagerServer levelManagerServer = null;

        /// <summary>
        /// sceneHandle, LoadedSceneButton
        /// </summary>
        private Dictionary<int, LoadedSceneButton> loadedSceneButtons = new Dictionary<int, LoadedSceneButton>();

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            objectPooler = systemGameManager.ObjectPooler;
            authenticationService = systemGameManager.AuthenticationService;
            levelManagerServer = systemGameManager.LevelManagerServer;
        }

        public void PopulateSceneList() {
            //Debug.Log($"HostServerPanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, SceneData> loadedScene in levelManagerServer.GetLoadedSceneData()) {
                AddSceneToList(loadedScene.Key, loadedScene.Value);
            }
        }

        public void AddSceneToList(int sceneHandle, SceneData sceneData) {
            //Debug.Log($"HostServerPanelController.AddPlayerToList({accountId}, {userName})");

            if (loadedSceneButtons.ContainsKey(sceneHandle)) {
                //Debug.Warning($"HostServerPanelController.AddPlayerToList() - player was already connected, and is reconnecting");
                loadedSceneButtons[sceneHandle].UpdateText();
                return;
            }
            GameObject go = objectPooler.GetPooledObject(loadedSceneTemplate, loadedSceneContainer);
            LoadedSceneButton loadedSceneButton = go.GetComponent<LoadedSceneButton>();
            loadedSceneButton.Configure(systemGameManager);
            loadedSceneButton.SetScenedata(sceneData);
            //sceneListNavigationController.AddActiveButton(loadedSceneButton.KickButton);
            loadedSceneButtons.Add(sceneHandle, loadedSceneButton);
        }

        public void RemoveSceneFromList(int sceneHandle) {
            //Debug.Log($"HostServerPanelController.RemovePlayerFromList({accountId})");

            if (loadedSceneButtons.ContainsKey(sceneHandle)) {
                //sceneListNavigationController.ClearActiveButton(loadedSceneButtons[sceneHandle].KickButton);
                if (loadedSceneButtons[sceneHandle].gameObject != null) {
                    loadedSceneButtons[sceneHandle].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(loadedSceneButtons[sceneHandle].gameObject);
                }
                loadedSceneButtons.Remove(sceneHandle);
            }
        }

        public void ClearSceneList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (LoadedSceneButton loadedSceneButton in loadedSceneButtons.Values) {
                if (loadedSceneButton.gameObject != null) {
                    loadedSceneButton.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(loadedSceneButton.gameObject);
                }
            }
            loadedSceneButtons.Clear();
            sceneListNavigationController.ClearActiveButtons();
        }

        public void SetSceneClientCount(int sceneHandle, int clientCount) {
            if (loadedSceneButtons.ContainsKey(sceneHandle) == true) {
                loadedSceneButtons[sceneHandle].UpdateText();
            }
        }
    }
}