using AnyRPG;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class SceneConfig : MonoBehaviour {

        public SystemConfigurationManager systemConfigurationManager = null;

        public bool loadGameOnPlay = false;

        private void OnEnable() {

            if (loadGameOnPlay == false) {
                return;
            }

            SystemGameManager systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
            if (systemGameManager != null) {
                //Debug.Log("Found a system game manager");
                return;
            }
            
            //Debug.Log("Did not find a system game manager");

            if (systemConfigurationManager != null && systemConfigurationManager.InitializationScene != string.Empty) {
                if (SceneManager.GetSceneByName(systemConfigurationManager.InitializationScene) != null) {
                    SceneManager.LoadScene(systemConfigurationManager.InitializationScene);
                } else if (SceneManager.GetSceneByName(systemConfigurationManager.InitializationScene.Replace(" ", "")) != null) {
                    SceneManager.LoadScene(systemConfigurationManager.InitializationScene.Replace(" ", ""));
                }
            }
        }

    }

}