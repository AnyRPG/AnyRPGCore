using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SceneButton : HighlightButton {

        [Header("Scene Button")]

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI sceneName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        protected SceneNode sceneNode = null;

        CreateLobbyGamePanel createLobbyGamePanel = null;

        protected UnitProfile unitProfile;

        protected string currentScene = string.Empty;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;

        public SceneNode SceneNode { get => sceneNode; set => sceneNode = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AddSceneData(CreateLobbyGamePanel createLobbyGamePanel, SceneNode sceneNode) {
            //Debug.Log("LoadGameButton.AddSaveData()");
            this.createLobbyGamePanel = createLobbyGamePanel;
            this.sceneNode = sceneNode;

            if (sceneNode.LoadingScreenImage != null) {
                icon.sprite = sceneNode.LoadingScreenImage;
                icon.color = Color.white;
            } else {
                //icon.sprite = null;
                //icon.color = Color.black;
                icon.sprite = systemConfigurationManager.QuestGiverInteractionPanelImage;
                icon.color = Color.white;
            }
            sceneName.text = sceneNode.DisplayName;
            description.text = sceneNode.Description;
        }

        public void CommonSelect() {
            //Debug.Log($"{gameObject.name}.SceneButton.CommonSelect()");

            if (createLobbyGamePanel.SelectedSceneButton != null && createLobbyGamePanel.SelectedSceneButton != this) {
                createLobbyGamePanel.SelectedSceneButton.DeSelect();
            }
            if (createLobbyGamePanel.SelectedSceneButton != this) {
                createLobbyGamePanel.ShowScene(this);
            }
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.SceneButton.Select()");
            CommonSelect();
            base.Select();
        }

    }

}