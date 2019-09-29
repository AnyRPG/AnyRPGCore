using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour {

    #region Singleton
    private static CameraManager instance;

    public static CameraManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<CameraManager>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameObject mainCameraGameObject;

    [SerializeField]
    private Camera miniMapCamera;

    [SerializeField]
    private Camera mainMapCamera;

    [SerializeField]
    private Camera characterPortraitCamera;

    [SerializeField]
    private Camera focusPortraitCamera;

    [SerializeField]
    private Camera characterCreatorCamera;

    [SerializeField]
    private Camera characterPreviewCamera;

    private AnyRPGCameraController mainCameraController;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    public Camera MyMainCamera { get => mainCamera; set => mainCamera = value; }
    public GameObject MyMainCameraGameObject { get => mainCameraGameObject; }
    public Camera MyMiniMapCamera { get => miniMapCamera; set => miniMapCamera = value; }
    public Camera MyMainMapCamera { get => mainMapCamera; set => mainMapCamera = value; }
    public Camera MyCharacterPortraitCamera { get => characterPortraitCamera; set => characterPortraitCamera = value; }
    public Camera MyFocusPortraitCamera { get => focusPortraitCamera; set => focusPortraitCamera = value; }
    public Camera MyCharacterCreatorCamera { get => characterCreatorCamera; set => characterCreatorCamera = value; }
    public Camera MyCharacterPreviewCamera { get => characterPreviewCamera; set => characterPreviewCamera = value; }
    public AnyRPGCameraController MyMainCameraController { get => mainCameraController; set => mainCameraController = value; }

    private void Awake() {
        //Debug.Log("CameraManager.Awake()");
        // attach camera to player
        mainCameraController = mainCameraGameObject.GetComponent<AnyRPGCameraController>();
    }

    private void Start() {
        //Debug.Log("CameraManager.Start()");
        startHasRun = true;
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
        eventReferencesInitialized = false;
    }

    public void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventReferences();
    }

    public void HandlePlayerUnitSpawn() {
        //Debug.Log("CameraManager.HandlePlayerUnitSpawn()");
        if (LevelManager.MyInstance.GetActiveSceneNode() == null) {
            //Debug.Log("CameraManager.HandlePlayerUnitSpawn(): ACTIVE SCENE NODE WAS NULL");
            return;
        }

        if (LevelManager.MyInstance.GetActiveSceneNode().MySuppressMainCamera != true) {
            //Debug.Log("CameraManager.HandlePlayerUnitSpawn(): suppressed by level = false, spawning camera");
            mainCamera.GetComponent<AnyRPGCameraController>().InitializeCamera(PlayerManager.MyInstance.MyPlayerUnitObject.transform);
        }
    }

    public void HandlePlayerUnitDespawn() {
        mainCamera.GetComponent<AnyRPGCameraController>().ClearTarget();
    }
}
