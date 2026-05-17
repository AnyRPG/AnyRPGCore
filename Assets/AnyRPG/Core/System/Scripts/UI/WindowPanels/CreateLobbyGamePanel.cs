using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CreateLobbyGamePanel : WindowPanel {

        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        
        //[SerializeField]
        //private HighlightButton cancelButton = null;

        [SerializeField]
        private HighlightButton createGameButton = null;

        [SerializeField]
        private TextMeshProUGUI statusText = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        [SerializeField]
        private TextMeshProUGUI descriptionText = null;

        [SerializeField]
        private Image previewImage = null;

        [SerializeField]
        private Toggle allowLateJoin = null;


        private List<SceneButton> sceneButtons = new List<SceneButton>();

        private SceneButton selectedSceneButton = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;
        private NetworkManagerClient networkManagerClient = null;

        public SceneButton SelectedSceneButton { get => selectedSceneButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            //Debug.Log("CreateLobbyGamePanel.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CreateLobbyGamePanel.RecieveClosedWindowNotification()");

            ClearSceneButtons();
            DisableCreateButton();
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CreateLobbyGamePanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();

            SetStatusLabel();
            ShowSceneButtonsCommon();
        }

        public void SetStatusLabel() {
            statusText.text = $"Logged In As: {networkManagerClient.Username}";
        }


        private void DisableCreateButton() {

            createGameButton.Button.interactable = false;
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void ShowScene(SceneButton sceneButton) {
            //Debug.Log($"CreateLobbyGamePanel.ShowScene({sceneButton.SceneNode.SceneName})");

            selectedSceneButton = sceneButton;

            createGameButton.Button.interactable = true;

            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(sceneButton);

            nameText.text = sceneButton.SceneNode.DisplayName;
            descriptionText.text = sceneButton.SceneNode.Description;

            if (sceneButton.SceneNode.LoadingScreenImage != null) {
                previewImage.sprite = sceneButton.SceneNode.LoadingScreenImage;
                previewImage.color = Color.white;
            } else {
                previewImage.sprite = null;
                previewImage.color = Color.black;
            }
        }


        public void ClearSceneButtons() {
            //Debug.Log("CreateLobbyGamePanel.ClearLoadButtons()");

            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            foreach (SceneButton sceneButton in sceneButtons) {
                if (sceneButton != null) {
                    sceneButton.DeSelect();
                    objectPooler.ReturnObjectToPool(sceneButton.gameObject);
                }
            }
            sceneButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons();
            selectedSceneButton = null;
            createGameButton.Button.interactable = false;
            nameText.text = "";
            descriptionText.text = "";
        }


        public void ShowSceneButtonsCommon() {
            //Debug.Log("CreateLobbyGamePanel.ShowSceneButtonsCommon()");
            ClearSceneButtons();
            int count = 0;
            foreach (SceneNode sceneNode in systemConfigurationManager.LobbyGameScenes) {
                AddSceneButton(sceneNode);
                count++;
            }
            uINavigationControllers[1].UpdateNavigationList();
            if (sceneButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            } else {
                SetNavigationController(uINavigationControllers[1]);
            }
        }

        private void AddSceneButton(SceneNode sceneNode) {
            GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
            SceneButton sceneButton = go.GetComponent<SceneButton>();
            sceneButton.Configure(systemGameManager);
            sceneButton.AddSceneData(this, sceneNode);
            sceneButtons.Add(sceneButton);
            uINavigationControllers[0].AddActiveButton(sceneButton);
        }

        public void CreateLobbyGame() {
            //Debug.Log("CreateLobbyGamePanel.CreateLobbyGame()");
            if (selectedSceneButton != null) {
                // this variable will be set to null in the Close() call so save the property we need first
                string sceneResourceName = selectedSceneButton.SceneNode.ResourceName;
                Close();
                networkManagerClient.RequestCreateLobbyGame(sceneResourceName, allowLateJoin.isOn);
            }
        }

    }

}