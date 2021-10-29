using AnyRPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class CutsceneCameraController : AutoConfiguredMonoBehaviour {

        private Camera thisCamera = null;

        public Camera Camera { get => thisCamera; set => thisCamera = value; }

        // game manager references
        private InputManager inputManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;

            thisCamera = GetComponent<Camera>();
        }

        private void LateUpdate() {
            if (systemGameManager == null) {
                Debug.LogError("InputManager not found in scene.  Is the GameManager in the scene?");
                return;
            }
            if (inputManager.KeyBindWasPressed("CANCEL")
                || inputManager.KeyBindWasPressed("CANCELALL")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")) {
                //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
                uIManager.confirmCancelCutsceneMenuWindow.OpenWindow();
            }

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
        }

        public void EndCutScene() {
            //Debug.Log("CutsceneCameraController.EndCutScene()");
            if (uIManager != null && uIManager.CutSceneBarController != null) {
                uIManager.CutSceneBarController.EndCutScene();
            }
        }

        public void AnimationFinished() {
            //Debug.Log("AnyRPGCutsceneCameraController.AnimationFinished(): re-activating in game UI");
            //SystemGameManager.Instance.UIManager.ActivateInGameUI();
            EndCutScene();
        }

        public void AdvanceDialog() {
            //Debug.Log("AnyRPGCutsceneCameraController.AdvanceDialog()");
            uIManager.CutSceneBarController.AdvanceDialog();
        }

        public void ActivateEnvironmentStateByIndex(int index) {

            SceneNode currentNode = levelManager.GetActiveSceneNode();
            if (currentNode != null && currentNode.EnvironmentStates != null && currentNode.EnvironmentStates.Count > index && currentNode.EnvironmentStates[index].MySkyBoxMaterial != null) {
                SystemEnvironmentManager.SetSkyBox(currentNode.EnvironmentStates[index].MySkyBoxMaterial);
            }
        }

    }

}