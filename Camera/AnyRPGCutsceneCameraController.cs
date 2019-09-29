using UnityEngine;
using UnityEngine.EventSystems;

public class AnyRPGCutsceneCameraController : MonoBehaviour {

    #region Singleton
    private static AnyRPGCutsceneCameraController instance;

    public static AnyRPGCutsceneCameraController MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<AnyRPGCutsceneCameraController>();
            }

            return instance;
        }
    }
    #endregion

    // public variables

    private void Awake() {
        //Debug.Log("AnyRPGCutsceneCameraController.Awake()");
    }

    private void Start() {
        //Debug.Log("AnyRPGCutsceneCameraController.Start()");
    }

    private void OnDisable() {
        //Debug.Log("AnyRPGCutsceneCameraController.OnDisable()");
    }

    private void OnDestroy() {
        //Debug.Log("AnyRPGCutsceneCameraController.OnDestroy()");
    }

    private void LateUpdate() {
        if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
            //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
            SystemWindowManager.MyInstance.confirmCancelCutsceneMenuWindow.OpenWindow();
        }
    }

    public void AnimationFinished() {
        //Debug.Log("AnyRPGCutsceneCameraController.AnimationFinished(): re-activating in game UI");
        //UIManager.MyInstance.ActivateInGameUI();
        UIManager.MyInstance.MyCutSceneBarController.EndCutScene();
    }

}
