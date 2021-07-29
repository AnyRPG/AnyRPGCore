using AnyRPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class CutsceneCameraController : MonoBehaviour {

        #region Singleton
        private static CutsceneCameraController instance;

        public static CutsceneCameraController Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CutsceneCameraController>();
                }

                return instance;
            }
        }
        #endregion

        // public variables

        private void LateUpdate() {
            if (SystemGameManager.Instance.InputManager == null) {
                Debug.LogError("InputManager not found in scene.  Is the GameManager in the scene?");
                return;
            }
            if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("CANCEL")) {
                //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
                SystemGameManager.Instance.UIManager.SystemWindowManager.confirmCancelCutsceneMenuWindow.OpenWindow();
            }

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
        }

        public void EndCutScene() {
            //Debug.Log("CutsceneCameraController.EndCutScene()");
            if (SystemGameManager.Instance.UIManager != null && SystemGameManager.Instance.UIManager.CutSceneBarController != null) {
                SystemGameManager.Instance.UIManager.CutSceneBarController.EndCutScene();
            }
        }

        public void AnimationFinished() {
            //Debug.Log("AnyRPGCutsceneCameraController.AnimationFinished(): re-activating in game UI");
            //SystemGameManager.Instance.UIManager.ActivateInGameUI();
            EndCutScene();
        }

        public void AdvanceDialog() {
            //Debug.Log("AnyRPGCutsceneCameraController.AdvanceDialog()");
            SystemGameManager.Instance.UIManager.CutSceneBarController.AdvanceDialog();
        }

        public void ActivateEnvironmentStateByIndex(int index) {

            SceneNode currentNode = SystemGameManager.Instance.LevelManager.GetActiveSceneNode();
            if (currentNode != null && currentNode.EnvironmentStates != null && currentNode.EnvironmentStates.Count > index && currentNode.EnvironmentStates[index].MySkyBoxMaterial != null) {
                SystemEnvironmentManager.SetSkyBox(currentNode.EnvironmentStates[index].MySkyBoxMaterial);
            }
        }

    }

}