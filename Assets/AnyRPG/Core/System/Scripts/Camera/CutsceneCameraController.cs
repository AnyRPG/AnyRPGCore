using UnityEngine;

namespace AnyRPG {
    public class CutsceneCameraController : AutoConfiguredMonoBehaviour {

        private Camera thisCamera = null;

        public Camera Camera { get => thisCamera; set => thisCamera = value; }

        // game manager references
        private UIManager uIManager = null;
        private LevelManagerClient levelManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            uIManager = systemGameManager.UIManager;
            levelManagerClient = systemGameManager.LevelManagerClient;

            thisCamera = GetComponent<Camera>();
        }

        private void LateUpdate() {
            if (systemGameManager == null) {
                Debug.LogError("InputManager not found in scene.  Is the GameManager in the scene?");
                return;
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

            SceneNode currentNode = levelManagerClient.GetActiveSceneNode();
            if (currentNode != null && currentNode.EnvironmentStates != null && currentNode.EnvironmentStates.Count > index && currentNode.EnvironmentStates[index].MySkyBoxMaterial != null) {
                SystemEnvironmentManager.SetSkyBox(currentNode.EnvironmentStates[index].MySkyBoxMaterial);
            }
        }

    }

}