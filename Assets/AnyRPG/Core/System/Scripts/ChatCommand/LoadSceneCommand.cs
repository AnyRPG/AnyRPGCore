using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Load Scene Command", menuName = "AnyRPG/Chat Commands/Load Scene Command")]
    public class LoadSceneCommand : ChatCommand {

        [Header("Load Scene Command")]

        [Tooltip("If true, all parameters will be ignored, and the scene loaded will be the scene listed below")]
        [SerializeField]
        private bool fixedScene = false;

        [Tooltip("Only applies if fixedScene is true")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string sceneName = string.Empty;

        // game manager references
        LevelManager levelManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManager = systemGameManager.LevelManager;
        }

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("LoadSceneCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // load a fixed scene
            if (fixedScene == true) {
                levelManager.LoadLevel(sceneName);
                return;
            }

            // the scene comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // load a scene from parameters
            LoadScene(commandParameters);
        }

        private void LoadScene(string sceneName) {
            levelManager.LoadLevel(sceneName);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            /*
            // check is disabled because you can load scenes directly by name without a scene node
            if (fixedScene == true && sceneName != null && sceneName != string.Empty) {
                SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(sceneName);
                if (sceneNode == null) {
                    Debug.LogError("LoadSceneCommand.SetupScriptableObjects(): Could not find scene node for : " + sceneName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
            */
        }

    }

}