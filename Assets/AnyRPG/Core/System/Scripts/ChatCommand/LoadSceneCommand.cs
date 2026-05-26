using UnityEngine;

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
        LevelManagerClient levelManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"{resourceName}.LoadSceneCommand.ExecuteCommand({commandParameters}, {accountId})");

            // load a fixed scene
            if (fixedScene == true) {
                LoadScene(sceneName, accountId);
                return;
            }

            // the scene comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // load a scene from parameters
            LoadScene(commandParameters, accountId);
        }

        private void LoadScene(string sceneName, int accountId) {
            //Debug.Log($"{resourceName}.LoadSceneCommand.LoadScene({sceneName}, {accountId})");

            playerManagerServer.AddSpawnRequest(accountId, new SpawnPlayerRequest());
            playerManagerServer.LoadScene(sceneName, accountId);
        }

    }

}