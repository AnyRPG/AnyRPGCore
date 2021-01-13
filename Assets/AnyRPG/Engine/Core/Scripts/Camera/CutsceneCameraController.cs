using AnyRPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class CutsceneCameraController : MonoBehaviour {

        #region Singleton
        private static CutsceneCameraController instance;

        public static CutsceneCameraController MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CutsceneCameraController>();
                }

                return instance;
            }
        }
        #endregion

        // public variables

        private void Awake() {
            //Debug.Log("CutsceneCameraController.Awake()");
        }

        private void Start() {
            //Debug.Log("CutsceneCameraController.Start()");
        }

        public void OnEnable() {
            //Debug.Log("CutsceneCameraController.OnEnable()");
        }

        private void OnDisable() {
            //Debug.Log("CutsceneCameraController.OnDisable()");
        }

        private void OnDestroy() {
            //Debug.Log("AnyRPGCutsceneCameraController.OnDestroy()");
        }

        private void LateUpdate() {
            if (InputManager.MyInstance == null) {
                Debug.LogError("InputManager not found in scene.  Is the GameManager in the scene?");
                return;
            }
            if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
                //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
                SystemWindowManager.MyInstance.confirmCancelCutsceneMenuWindow.OpenWindow();
            }

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
        }

        public void EndCutScene() {
            //Debug.Log("CutsceneCameraController.EndCutScene()");
            if (UIManager.MyInstance != null && UIManager.MyInstance.MyCutSceneBarController != null) {
                UIManager.MyInstance.MyCutSceneBarController.EndCutScene();
            }
        }

        public void AnimationFinished() {
            //Debug.Log("AnyRPGCutsceneCameraController.AnimationFinished(): re-activating in game UI");
            //UIManager.MyInstance.ActivateInGameUI();
            EndCutScene();
        }

        public void AdvanceDialog() {
            //Debug.Log("AnyRPGCutsceneCameraController.AdvanceDialog()");
            UIManager.MyInstance.MyCutSceneBarController.AdvanceDialog();
        }

        public void ActivateEnvironmentStateByIndex(int index) {

            SceneNode currentNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (currentNode != null && currentNode.EnvironmentStates != null && currentNode.EnvironmentStates.Count > index && currentNode.EnvironmentStates[index].MySkyBoxMaterial != null) {
                SystemEnvironmentManager.MyInstance.SetSkyBox(currentNode.EnvironmentStates[index].MySkyBoxMaterial);
            }
        }

    }

}