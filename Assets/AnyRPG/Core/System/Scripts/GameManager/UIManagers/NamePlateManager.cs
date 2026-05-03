using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AnyRPG {
    public class NamePlateManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject namePlatePrefab = null;

        [SerializeField]
        private Transform namePlateContainer = null;

        /// <summary>
        /// The currently focused nameplate so we can highlight the outline
        /// </summary>
        private Interactable focus;

        private NativeArray<RaycastCommand> commands;
        private NativeArray<RaycastHit> results;
        private JobHandle jobHandle;
        private bool isJobRunning = false;
        private List<NamePlateController> raycastCandidates = new List<NamePlateController>();


        private List<NamePlateController> mouseOverList = new List<NamePlateController>();

        private Dictionary<Interactable, NamePlateController> namePlates = new Dictionary<Interactable, NamePlateController>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private LevelManagerClient levelManagerClient = null;
        private UIManager uIManager = null;
        private CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            levelManagerClient.OnLevelUnload += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            levelManagerClient = systemGameManager.LevelManagerClient;
            uIManager = systemGameManager.UIManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void AddMouseOver(NamePlateController namePlateController) {
            //Debug.Log($"NamePlateManager.AddMouseOver({namePlateController.gameObject?.name})");

            if (!mouseOverList.Contains(namePlateController)) {
                mouseOverList.Add(namePlateController);
            }
        }

        public void RemoveMouseOver(NamePlateController namePlateController) {
            //Debug.Log($"NamePlateManager.RemoveMouseOver({namePlateController.gameObject?.name})");

            mouseOverList.Remove(namePlateController);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateNameplates();
        }

        /*
        private void UpdateNamePlates() {
            foreach (NamePlateController namePlateController in namePlates.Values) {
                namePlateController.UpdatePosition();
            }
        }
        */

        private void UpdateNameplates() {
            if (isJobRunning) {
                if (jobHandle.IsCompleted) {
                    jobHandle.Complete();
                    for (int i = 0; i < raycastCandidates.Count; i++) {
                        var controller = raycastCandidates[i];

                        // Safety check: Did this object get destroyed while the job was running?
                        if (controller?.UnitNamePlateController != null) {
                            bool isBlocked = results[i].collider != null;
                            controller.FinalizeVisibility(!isBlocked);
                        }
                    }
                    CleanupNative();
                    isJobRunning = false;
                } else return;
            }

            raycastCandidates.Clear();
            Camera currentCamera = GetCurrentCamera();
            if (currentCamera == null) return;

            foreach (NamePlateController controller in namePlates.Values) {
                if (controller.PreFilter(currentCamera)) raycastCandidates.Add(controller);
            }

            if (raycastCandidates.Count > 0) {
                int count = raycastCandidates.Count;
                commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
                results = new NativeArray<RaycastHit>(count, Allocator.TempJob);

                // Modern QueryParameters for the standard Physics engine
                // Use 1 << layerIndex to create the mask
                QueryParameters queryParams = new QueryParameters(1 << LayerMask.NameToLayer("Default"), false, QueryTriggerInteraction.Ignore);
                Vector3 camPos = currentCamera.transform.position;

                for (int i = 0; i < count; i++) {
                    Vector3 targetPos = raycastCandidates[i].UnitNamePlateController.NameplatePosition;
                    Vector3 dir = targetPos - camPos;

                    // Use the new constructor that takes QueryParameters
                    commands[i] = new RaycastCommand(camPos, dir.normalized, queryParams, dir.magnitude);
                }

                jobHandle = RaycastCommand.ScheduleBatch(commands, results, 1);
                isJobRunning = true;
            }
        }

        private void CleanupNative() {
            if (commands.IsCreated) commands.Dispose();
            if (results.IsCreated) results.Dispose();
        }

        private Camera GetCurrentCamera() {
            if (uIManager.CutSceneBarController.CurrentCutscene != null)
                return cameraManager.CurrentCutsceneCameraController?.Camera;
            return cameraManager.ActiveMainCamera;
        }

        private void CleanupNativeArrays() {
            if (commands.IsCreated) commands.Dispose();
            if (results.IsCreated) results.Dispose();
        }

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            mouseOverList.Clear();
            if (isJobRunning) {
                jobHandle.Complete(); // Force finish immediately
                CleanupNative();      // Free the memory buffers
                isJobRunning = false;
                raycastCandidates.Clear(); // Clear the stale references
            }
        }
        /*
        public void LateUpdate() {
            if (systemConfigurationManager.UseThirdPartyCameraControl == true
                && cameraManager.ThirdPartyCamera.activeInHierarchy == true
                && playerManager.PlayerUnitSpawned == true) {
                UpdateNamePlates();
            }
        }
        */

        public void SetFocus(Interactable newInteractable) {
            ClearFocus();
            //Debug.Log("NamePlateManager.SetFocus(" + characterUnit.MyCharacter.MyCharacterName + ")");
            if (namePlates.ContainsKey(newInteractable)) {
                focus = newInteractable;
                // enemy could be dead so we need to check if they exist in the nameplates dictionary
                namePlates[newInteractable].Highlight();
            }
        }

        public void ClearFocus() {
            //Debug.Log("NamePlateManager.ClearFocus()");
            if (focus != null) {
                if (namePlates.ContainsKey(focus)) {
                    // enemy could be dead so we need to check if they exist in the nameplates dictionary
                    namePlates[focus].UnHighlight();
                }
            }
            focus = null;
        }

        public NamePlateController SpawnNamePlate(Interactable interactable, bool usePositionOffset) {
            //Debug.Log($"NamePlateManager.SpawnNamePlate({namePlateUnit.gameObject.name})");

            NamePlateController namePlate = objectPooler.GetPooledObject(namePlatePrefab, namePlateContainer).GetComponent<NamePlateController>();
            namePlate.Configure(systemGameManager);
            namePlates.Add(interactable, namePlate);
            namePlate.SetNamePlateUnit(interactable, usePositionOffset);

            // testing - so nameplates spawned after setting target don't end up in front of the target
            namePlate.transform.SetAsFirstSibling();
            return namePlate;
        }

        public NamePlateController AddNamePlate(Interactable interactable, bool usePositionOffset) {
            //Debug.Log($"NamePlateManager.AddNamePlate({interactable.gameObject.name})");

            if (namePlates.ContainsKey(interactable) == false) {
                return SpawnNamePlate(interactable, usePositionOffset);
            }
            //Debug.Log("NamePlateManager.AddNamePlate(" + interactable.gameObject.name + "): key already existed.  returning null!!!");
            return namePlates[interactable];
        }

        public void RemoveNamePlate(Interactable interactable) {
            //Debug.Log($"NamePlatemanager.RemoveNamePlate({namePlateUnit?.gameObject?.name})");

            if (namePlates.ContainsKey(interactable)) {
                //Debug.Log($"NamePlatemanager.RemoveNamePlate({namePlateUnit.gameObject.name}) namePlates contains key");
                if (namePlates[interactable] != null && namePlates[interactable].gameObject != null) {
                    namePlates[interactable].OnSendObjectToPoolManual();
                    objectPooler.ReturnObjectToPool(namePlates[interactable].gameObject);
                } else {
                    //Debug.Log($"NamePlatemanager.RemoveNamePlate({namePlateUnit.gameObject.name}) could not find nameplate gameobject");
                }
                namePlates.Remove(interactable);
            } else {
                //Debug.Log($"NamePlatemanager.RemoveNamePlate({namePlateUnit.gameObject.name}) namePlates did not contain key");
            }
        }

        public bool MouseOverNamePlate() {
            /*
            foreach (NamePlateController namePlateController in namePlates.Values) {
                if (namePlateController.NamePlateCanvasController.MouseOverNamePlate() == true) {
                    return true;
                }
            }
            return false;
            */
            return (mouseOverList.Count > 0);
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            levelManagerClient.OnLevelUnload -= HandleLevelUnload;
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
            if (isJobRunning) {
                jobHandle.Complete();
                CleanupNativeArrays();
            }
        }

    }

}