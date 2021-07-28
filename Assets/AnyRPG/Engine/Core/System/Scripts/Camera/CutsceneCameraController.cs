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
            if (InputManager.Instance == null) {
                Debug.LogError("InputManager not found in scene.  Is the GameManager in the scene?");
                return;
            }
            if (InputManager.Instance.KeyBindWasPressed("CANCEL")) {
                //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
                SystemWindowManager.Instance.confirmCancelCutsceneMenuWindow.OpenWindow();
            }

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
        }

        public void EndCutScene() {
            //Debug.Log("CutsceneCameraController.EndCutScene()");
            if (UIManager.Instance != null && UIManager.Instance.CutSceneBarController != null) {
                UIManager.Instance.CutSceneBarController.EndCutScene();
            }
        }

        public void AnimationFinished() {
            //Debug.Log("AnyRPGCutsceneCameraController.AnimationFinished(): re-activating in game UI");
            //UIManager.Instance.ActivateInGameUI();
            EndCutScene();
        }

        public void AdvanceDialog() {
            //Debug.Log("AnyRPGCutsceneCameraController.AdvanceDialog()");
            UIManager.Instance.CutSceneBarController.AdvanceDialog();
        }

        public void ActivateEnvironmentStateByIndex(int index) {

            SceneNode currentNode = LevelManager.Instance.GetActiveSceneNode();
            if (currentNode != null && currentNode.EnvironmentStates != null && currentNode.EnvironmentStates.Count > index && currentNode.EnvironmentStates[index].MySkyBoxMaterial != null) {
                SystemEnvironmentManager.SetSkyBox(currentNode.EnvironmentStates[index].MySkyBoxMaterial);
            }
        }

    }

}